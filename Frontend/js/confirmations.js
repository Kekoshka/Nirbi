import { toast }                              from './toast.js';
import { tokenStore }                         from './tokenStore.js';
import { authApi }                            from './api.js';
import { confirmationsApi }                   from './confirmationsApi.js';
import { startNotifications, onNotification } from './notifications.js';
import { CONFIRMATION_TYPES }  from './config.js';

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
}

// ── Load data ────────────────────────────────────────────────────────────────
async function loadAll() {
  if (!currentUserId) return;
  try {
    const [inc, out] = await Promise.all([
      confirmationsApi.getByReviewer(currentUserId).catch(() => []),
      confirmationsApi.getByInitiator(currentUserId).catch(() => []),
    ]);
    received = (Array.isArray(inc) ? inc : []).filter(c =>
      !c.confirmationType || c.confirmationType === CONFIRMATION_TYPES.RESPOND_TO_MINOR_TASK
    );
    sent = (Array.isArray(out) ? out : []).filter(c =>
      !c.confirmationType || c.confirmationType === CONFIRMATION_TYPES.RESPOND_TO_MINOR_TASK
    );
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

  // Gateway теперь возвращает entityName и initiatorUsername/reviewerUsername напрямую.
  // Fallback на metaData — для совместимости с не-обогащёнными ответами.
  const taskName = escapeHtml(c.entityName ?? c.metaData?.taskName ?? '—');
  const who = escapeHtml(
    kind === 'received'
      ? (c.initiatorUsername ?? c.metaData?.applicantUsername ?? '')
      : (c.reviewerUsername  ?? '')
  );

  const subtitle = kind === 'received'
    ? (who ? `От: <strong>${who}</strong>` : 'От пользователя')
    : `Заявка на: <strong>${taskName}</strong>`;

  const headline = kind === 'received'
    ? `Заявка на задачу «${taskName}»`
    : 'Ваш отклик';

  const reasonRow = (status === 'rejected' && c.rejectionReason)
    ? `<div class="conf-row conf-reason"><span>Причина:</span> ${escapeHtml(c.rejectionReason)}</div>`
    : '';

  let actions = '';
  if (isPending(c.status)) {
    if (kind === 'received') {
      actions = `
        <button class="conf-btn conf-btn-accept" data-action="accept" data-id="${c.id}">
          <span class="btn-label">Принять</span>
        </button>
        <button class="conf-btn conf-btn-reject" data-action="reject" data-id="${c.id}">
          <span class="btn-label">Отклонить</span>
        </button>`;
    } else {
      actions = `
        <button class="conf-btn conf-btn-revoke" data-action="revoke" data-id="${c.id}">
          <span class="btn-label">Отозвать</span>
        </button>`;
    }
  }

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
      ${actions ? `<div class="conf-card-actions">${actions}</div>` : ''}
      <div class="conf-card-link-hint">Нажмите чтобы открыть задачу →</div>
    </div>`;
}

function bindActions() {
  listEl.querySelectorAll('.conf-btn').forEach(btn => {
    btn.addEventListener('click', async () => {
      const id     = btn.dataset.id;
      const action = btn.dataset.action;
      setLoading(btn, true);
      try {
        if (action === 'accept') {
          await confirmationsApi.respond(id, true);
          toast.success('Заявка принята');
        } else if (action === 'reject') {
          await confirmationsApi.respond(id, false, 'Отклонено организатором');
          toast.info('Заявка отклонена');
        } else if (action === 'revoke') {
          await confirmationsApi.revoke(id, currentUserId);
          toast.info('Заявка отозвана');
        }
        await loadAll();
      } catch (e) {
        toast.error(e.message || 'Ошибка операции');
      } finally {
        setLoading(btn, false);
      }
    });
  });

  // Клик по карточке → открыть связанную задачу
  // Сейчас поддерживаются только задачи; в будущем можно расширить по confirmationType
  listEl.querySelectorAll('.conf-card-clickable').forEach(card => {
    card.addEventListener('click', e => {
      // Не переходим, если пользователь кликнул на кнопку действия
      if (e.target.closest('.conf-btn')) return;
      const entityId = card.dataset.entityId;
      if (entityId && entityId !== '00000000-0000-0000-0000-000000000000') {
        window.location.href = `tasks.html?openTask=${entityId}`;
      }
    });
  });
}

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
onNotification('ShowConfirmationCreated', payload => {
  const applicant = payload?.metaData?.applicantUsername || 'Пользователь';
  const taskName  = payload?.metaData?.taskName || '';
  toast.info(`${applicant} откликнулся${taskName ? ` на «${taskName}»` : ''}`);
  loadAll();
});

onNotification('ShowConfirmationRespond', payload => {
  const status = (payload?.status || '').toLowerCase();
  const taskName = payload?.metaData?.taskName || '';
  if (status === 'accepted')      toast.success(`Заявка принята${taskName ? ` («${taskName}»)` : ''}`);
  else if (status === 'rejected') toast.warning(`Заявка отклонена${taskName ? ` («${taskName}»)` : ''}`);
  else                            toast.info('Ответ на заявку получен');
  loadAll();
});

onNotification('ShowConfirmationRevoked', payload => {
  const applicant = payload?.metaData?.applicantUsername || 'Пользователь';
  const taskName  = payload?.metaData?.taskName || '';
  toast.info(`${applicant} отозвал заявку${taskName ? ` на «${taskName}»` : ''}`);
  loadAll();
});

// ── Boot ─────────────────────────────────────────────────────────────────────
loadAll();
startNotifications();
