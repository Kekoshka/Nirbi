import { toast }                              from './toast.js';
import { tokenStore }                         from './tokenStore.js';
import { authApi }                            from './api.js';
import { confirmationsApi }                   from './confirmationsApi.js';
import { startNotifications, onNotification } from './notifications.js';
import { CONFIRMATION_TYPES }  from './config.js';
import { parseMetaData }       from './confirmationMeta.js';
import { openUserProfile }     from './userProfileModal.js';
import { openTaskDetail }      from './taskDetailModal.js';

// Локальные лейблы статусов
const STATUS_LABELS = {
  created:  'Ожидает',
  pending:  'Ожидает',
  accepted: 'Принята',
  rejected: 'Отклонена',
  revoked:  'Отозвана',
  expired:  'Истекла',
};

// Бэкенд возвращает "Created" как начальный статус — нормализуем
function isPending(status) {
  const s = (status || '').toLowerCase();
  return s === 'pending' || s === 'created';
}

// ── Guard ────────────────────────────────────────────────────────────────────
if (!tokenStore.hasSession()) {
  window.location.href = 'index.html#login';
}

const currentUserId = tokenStore.getUserId();

// ── State ────────────────────────────────────────────────────────────────────
let activeTab    = 'received';   // 'received' | 'sent'
let received    = [];            // confirmations where I'm reviewer
let sent        = [];            // confirmations where I'm initiator

// ── DOM ──────────────────────────────────────────────────────────────────────
const listEl       = document.getElementById('conf-list');
const emptyEl      = document.getElementById('empty-state');
const emptySub     = document.getElementById('empty-state-sub');
const cntLabel     = document.getElementById('conf-count-label');
const cntReceived  = document.getElementById('cnt-received');
const cntSent      = document.getElementById('cnt-sent');

// ── Helpers ──────────────────────────────────────────────────────────────────
function escapeHtml(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => ({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'
  }[c]));
}

function fmtDate(d) {
  if (!d) return '';
  try {
    return new Date(d).toLocaleString('ru-RU', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
  } catch { return ''; }
}

function statusBadge(status) {
  const s = (status || '').toLowerCase();
  const label = STATUS_LABELS[s] || status || '—';
  return `<span class="conf-status conf-status-${s || 'unknown'}">${escapeHtml(label)}</span>`;
}

function setLoading(btn, on) {
  if (!btn) return;
  btn.disabled = on;
  btn.classList.toggle('is-loading', on);
  const lbl = btn.querySelector('.btn-label');
  const sp  = btn.querySelector('.btn-spinner');
  if (lbl) lbl.hidden = on;
  if (sp)  sp.hidden  = !on;
}

// Имя задачи из обогащённого ответа Gateway (entityName),
// с фолбэком на metaData.taskName (на случай не-обогащённых данных).
function taskNameOf(c) {
  const meta = parseMetaData(c.metaData);
  return c.entityName ?? meta.taskName ?? '—';
}

// Имя «второй стороны» в зависимости от вкладки.
// received → кто откликнулся (initiator); sent → кому отправлено (reviewer).
function counterpartyOf(c, kind) {
  const meta = parseMetaData(c.metaData);
  return kind === 'received'
    ? (c.initiatorUsername ?? meta.applicantUsername ?? '')
    : (c.reviewerUsername  ?? '');
}

// ── Load data ────────────────────────────────────────────────────────────────
async function loadAll() {
  if (!currentUserId) return;
  try {
    // Gateway-агрегатор уже возвращает EnrichedConfirmationResponse:
    // entityName + initiatorUsername + reviewerUsername. Доп. запросов не нужно.
    const [inc, out] = await Promise.all([
      confirmationsApi.getByReviewer().catch(() => []),
      confirmationsApi.getByInitiator().catch(() => []),
    ]);
    // Пропускаем оба типа: отклики (Respond) и приглашения (Invite).
    const allowedType = t => !t
      || t === CONFIRMATION_TYPES.RESPOND_TO_MINOR_TASK
      || t === CONFIRMATION_TYPES.INVITE_TO_TASK;
    received = (Array.isArray(inc) ? inc : []).filter(c => allowedType(c.confirmationType));
    sent = (Array.isArray(out) ? out : []).filter(c => allowedType(c.confirmationType));
    render();
  } catch (e) {
    console.error('Failed to load confirmations:', e);
    toast.error('Не удалось загрузить подтверждения');
  }
}

// ── Render ───────────────────────────────────────────────────────────────────
function render() {
  cntReceived.textContent = received.length;
  cntSent.textContent     = sent.length;

  const items = activeTab === 'received' ? received : sent;
  cntLabel.textContent = items.length
    ? `Всего: ${items.length}`
    : 'Пусто';

  if (!items.length) {
    listEl.innerHTML = '';
    emptyEl.hidden = false;
    emptySub.textContent = activeTab === 'received'
      ? 'Никто пока не откликался на ваши задачи'
      : 'Вы пока не откликались на чужие задачи';
    return;
  }
  emptyEl.hidden = true;

  // Сортируем — новые сверху
  const sorted = [...items].sort((a, b) =>
    new Date(b.createdAt || 0) - new Date(a.createdAt || 0)
  );

  listEl.innerHTML = sorted.map(c => renderCard(c, activeTab)).join('');
  bindActions();
}

function renderCard(c, kind) {
  const status   = (c.status || '').toLowerCase();
  const isInvite = c.confirmationType === CONFIRMATION_TYPES.INVITE_TO_TASK;

  // Имена приходят из Gateway-агрегатора напрямую.
  const taskName = escapeHtml(taskNameOf(c));
  const who      = escapeHtml(counterpartyOf(c, kind));

  // ID «второй стороны» для просмотра профиля:
  // received → собеседник это initiator; sent → reviewer.
  const counterpartyId = kind === 'received' ? c.initiatorId : c.reviewerId;

  // Тексты зависят от типа (отклик/приглашение) и вкладки.
  let subtitle, headline;
  if (isInvite) {
    if (kind === 'received') {
      // Меня пригласили в задачу (я reviewer)
      headline = `Приглашение в задачу «${taskName}»`;
      subtitle = who ? `Пригласил: <strong>${who}</strong>` : 'Приглашение от организатора';
    } else {
      // Я пригласил волонтёра (я initiator)
      headline = `Приглашение в «${taskName}»`;
      subtitle = who ? `Кому: <strong>${who}</strong>` : 'Приглашение волонтёру';
    }
  } else {
    if (kind === 'received') {
      headline = `Заявка на задачу «${taskName}»`;
      subtitle = who ? `От: <strong>${who}</strong>` : 'От пользователя';
    } else {
      headline = `Ваш отклик на «${taskName}»`;
      subtitle = who ? `Организатор: <strong>${who}</strong>` : `Заявка на: <strong>${taskName}</strong>`;
    }
  }

  const reasonRow = (status === 'rejected' && c.rejectionReason)
    ? `<div class="conf-row conf-reason"><span>Причина:</span> ${escapeHtml(c.rejectionReason)}</div>`
    : '';

  let actions = '';
  if (isPending(c.status)) {
    if (kind === 'received') {
      actions = `
        <button class="conf-btn conf-btn-accept" data-action="accept" data-id="${c.id}">
          <span class="btn-label">${isInvite ? 'Принять приглашение' : 'Принять'}</span>
          <span class="btn-spinner" hidden></span>
        </button>
        <button class="conf-btn conf-btn-reject" data-action="reject" data-id="${c.id}">
          <span class="btn-label">Отклонить</span>
          <span class="btn-spinner" hidden></span>
        </button>`;
    } else {
      actions = `
        <button class="conf-btn conf-btn-revoke" data-action="revoke" data-id="${c.id}">
          <span class="btn-label">${isInvite ? 'Отменить приглашение' : 'Отозвать'}</span>
          <span class="btn-spinner" hidden></span>
        </button>`;
    }
  }

  // Кнопка просмотра профиля собеседника
  const profileBtn = counterpartyId
    ? `<button class="conf-btn conf-btn-view" data-action="view-profile" data-user="${counterpartyId}" data-name="${who}">
         <span class="btn-label">Профиль</span>
       </button>`
    : '';

  const allActions = (actions || profileBtn)
    ? `<div class="conf-card-actions">${actions}${profileBtn}</div>`
    : '';

  // data-entity-id — для перехода к задаче по клику
  return `
    <div class="conf-card conf-card-clickable" data-id="${c.id}" data-entity-id="${c.entityId}">
      <div class="conf-card-head">
        <div class="conf-card-title">
          <h3>${headline}</h3>
          <p class="conf-card-sub">${subtitle}</p>
        </div>
        ${statusBadge(c.status)}
      </div>
      <div class="conf-card-body">
        <div class="conf-row"><span>Создана:</span> ${fmtDate(c.createdAt)}</div>
        ${c.respondedAt ? `<div class="conf-row"><span>Ответ:</span> ${fmtDate(c.respondedAt)}</div>` : ''}
        ${c.expiresAt && isPending(c.status) ? `<div class="conf-row"><span>Истекает:</span> ${fmtDate(c.expiresAt)}</div>` : ''}
        ${reasonRow}
      </div>
      ${allActions}
      <div class="conf-card-link-hint">Нажмите чтобы открыть задачу →</div>
    </div>`;
}

function bindActions() {
  listEl.querySelectorAll('.conf-btn').forEach(btn => {
    btn.addEventListener('click', async () => {
      const id     = btn.dataset.id;
      const action = btn.dataset.action;

      // Отклонение — через модалку с необязательной причиной
      if (action === 'reject') {
        openRejectModal(id);
        return;
      }

      // Просмотр профиля собеседника
      if (action === 'view-profile') {
        openUserProfile(btn.dataset.user, { fallbackName: btn.dataset.name || '' });
        return;
      }

      setLoading(btn, true);
      try {
        if (action === 'accept') {
          await confirmationsApi.respond(id, true);
          toast.success('Принято');
        } else if (action === 'revoke') {
          await confirmationsApi.revoke(id, currentUserId);
          toast.info('Отозвано');
        }
        await loadAll();
      } catch (e) {
        toast.error(e.message || 'Ошибка операции');
      } finally {
        setLoading(btn, false);
      }
    });
  });

  // Клик по карточке → открыть детали задачи прямо здесь (read-only модалка)
  listEl.querySelectorAll('.conf-card-clickable').forEach(card => {
    card.addEventListener('click', e => {
      // Не открываем, если кликнули по кнопке действия или кнопке профиля
      if (e.target.closest('.conf-btn')) return;
      const entityId = card.dataset.entityId;
      if (entityId && entityId !== '00000000-0000-0000-0000-000000000000') {
        openTaskDetail(entityId);
      }
    });
  });
}

// ── Reject reason modal ────────────────────────────────────────────────────────
const modalReject    = document.getElementById('modal-reject');
const rejectReasonEl  = document.getElementById('reject-reason');
const btnConfirmReject = document.getElementById('btn-confirm-reject');
const btnCancelReject  = document.getElementById('btn-cancel-reject');
let pendingRejectId = null;

function openRejectModal(id) {
  pendingRejectId = id;
  if (rejectReasonEl) rejectReasonEl.value = '';
  if (modalReject) {
    modalReject.hidden = false;
    document.body.style.overflow = 'hidden';
    setTimeout(() => rejectReasonEl?.focus(), 50);
  }
}
function closeRejectModal() {
  if (modalReject) modalReject.hidden = true;
  document.body.style.overflow = '';
  pendingRejectId = null;
}

btnCancelReject?.addEventListener('click', closeRejectModal);
modalReject?.addEventListener('click', e => { if (e.target === modalReject) closeRejectModal(); });
document.addEventListener('keydown', e => {
  if (e.key === 'Escape' && modalReject && !modalReject.hidden) closeRejectModal();
});

btnConfirmReject?.addEventListener('click', async () => {
  if (!pendingRejectId) return;
  const reason = (rejectReasonEl?.value || '').trim();
  setLoading(btnConfirmReject, true);
  try {
    // Причина необязательна: пустую отправляем как null
    await confirmationsApi.respond(pendingRejectId, false, reason || null);
    toast.info('Заявка отклонена');
    closeRejectModal();
    await loadAll();
  } catch (e) {
    toast.error(e.message || 'Не удалось отклонить заявку');
  } finally {
    setLoading(btnConfirmReject, false);
  }
});

// ── Tabs ─────────────────────────────────────────────────────────────────────
document.querySelectorAll('#conf-tabs .toggle-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('#conf-tabs .toggle-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    activeTab = btn.dataset.tab;
    render();
  });
});

// ── Refresh button ───────────────────────────────────────────────────────────
document.getElementById('btn-refresh-conf').addEventListener('click', loadAll);

// ── Logout ───────────────────────────────────────────────────────────────────
document.getElementById('btn-logout').addEventListener('click', async () => {
  const refresh = tokenStore.getRefresh();
  tokenStore.clear();
  if (refresh) await authApi.logout(refresh).catch(() => {});
  window.location.href = 'index.html#login';
});

// ── SignalR real-time ────────────────────────────────────────────────────────
// Payload приходит напрямую от ConfirmationService (мимо агрегатора), поэтому
// имена/название берём из metaData (она здесь — JSON-строка или объект).
// Точные имена в любом случае подтянет последующий loadAll() из агрегатора.
onNotification('ShowConfirmationCreated', payload => {
  const meta = parseMetaData(payload?.metaData);
  const applicant = meta.applicantUsername || payload?.initiatorUsername || 'Пользователь';
  const taskName  = meta.taskName || payload?.entityName || '';
  toast.info(`${applicant} откликнулся${taskName ? ` на «${taskName}»` : ''}`);
  loadAll();
});

onNotification('ShowConfirmationRespond', payload => {
  const meta = parseMetaData(payload?.metaData);
  const status = (payload?.status || '').toLowerCase();
  const taskName = meta.taskName || payload?.entityName || '';
  if (status === 'accepted')      toast.success(`Заявка принята${taskName ? ` («${taskName}»)` : ''}`);
  else if (status === 'rejected') toast.warning(`Заявка отклонена${taskName ? ` («${taskName}»)` : ''}`);
  else                            toast.info('Ответ на заявку получен');
  loadAll();
});

onNotification('ShowConfirmationRevoked', payload => {
  const meta = parseMetaData(payload?.metaData);
  const applicant = meta.applicantUsername || payload?.initiatorUsername || 'Пользователь';
  const taskName  = meta.taskName || payload?.entityName || '';
  toast.info(`${applicant} отозвал заявку${taskName ? ` на «${taskName}»` : ''}`);
  loadAll();
});

// ── Boot ─────────────────────────────────────────────────────────────────────
loadAll();
startNotifications();
