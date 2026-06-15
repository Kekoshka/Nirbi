import { toast }       from './toast.js';
import { tokenStore }  from './tokenStore.js';
import { authApi, usersApi } from './api.js';
import { tasksApi }    from './tasksApi.js';
import { confirmationsApi } from './confirmationsApi.js';
import { openUserProfile } from './userProfileModal.js';
import { startNotifications } from './notifications.js';
import { CONFIRMATION_TYPES, CONFIRMATION_DEFAULT_EXPIRATION_HOURS } from './config.js';

// ── Guard ────────────────────────────────────────────────────────────────────
if (!tokenStore.hasSession()) {
  window.location.href = 'index.html#login';
}
const currentUserId = tokenStore.getUserId();

// Поля, которые тянем для карточек списка
const LIST_FIELDS = ['firstName', 'secondName', 'lastName', 'username', 'city', 'tg', 'vk', 'max'];
const PAGE_SIZE = 20;

// ── State ────────────────────────────────────────────────────────────────────
let offset = 0;
let total = null;          // null = неизвестно (бэкенд не вернул total)
let searchQuery = '';
let loading = false;
let items = [];
let myTasks = [];          // задачи текущего пользователя — для приглашения

// ── DOM ──────────────────────────────────────────────────────────────────────
const grid       = document.getElementById('users-grid');
const emptyEl     = document.getElementById('users-empty');
const countLabel  = document.getElementById('users-count-label');
const searchInput = document.getElementById('users-search');
const pager       = document.getElementById('users-pager');
const btnPrev     = document.getElementById('btn-prev');
const btnNext     = document.getElementById('btn-next');
const pageLabel   = document.getElementById('page-label');

// ── Helpers ───────────────────────────────────────────────────────────────────
function escHtml(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => ({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;',
  }[c]));
}
function fullName(u) {
  return [u.lastName, u.firstName, u.secondName].filter(Boolean).join(' ')
      || u.username || 'Пользователь';
}
function initials(u) {
  const parts = [u.firstName, u.lastName].filter(Boolean).map(s => s[0].toUpperCase());
  return parts.join('') || (u.username ? u.username[0].toUpperCase() : '?');
}
function idOf(u) { return u.id ?? u.userId; }

// ── Load list ──────────────────────────────────────────────────────────────────
async function loadUsers() {
  if (loading) return;
  loading = true;
  grid.innerHTML = '<div class="user-card skeleton"></div>'.repeat(8);
  emptyEl.hidden = true;

  try {
    const res = await usersApi.list({
      offset, limit: PAGE_SIZE, search: searchQuery, fields: LIST_FIELDS,
    });
    total = res.total;
    // Не показываем самого себя в списке для приглашений
    items = (res.items || []).filter(u => String(idOf(u)) !== String(currentUserId));
    render();
  } catch (e) {
    console.error('[users] load failed:', e);
    toast.error('Не удалось загрузить пользователей');
    grid.innerHTML = '';
    emptyEl.hidden = false;
  } finally {
    loading = false;
  }
}

function render() {
  // Счётчик
  if (total != null) {
    countLabel.textContent = `Всего: ${total}`;
  } else {
    countLabel.textContent = items.length ? `Показано: ${items.length}` : 'Пусто';
  }

  if (!items.length) {
    grid.innerHTML = '';
    emptyEl.hidden = false;
    updatePager();
    return;
  }
  emptyEl.hidden = true;

  grid.innerHTML = items.map((u, i) => {
    const id = idOf(u);
    const hasMessenger = u.tg || u.vk || u.max;
    return `
      <div class="user-card" data-id="${id}" style="animation-delay:${i * 25}ms">
        <div class="user-avatar">${escHtml(initials(u))}</div>
        <div class="user-info">
          <div class="user-name">${escHtml(fullName(u))}</div>
          <div class="user-sub">${escHtml(u.city || u.username || '')}</div>
          ${hasMessenger ? `<div class="user-badges">
            ${u.tg ? '<span class="user-badge">TG</span>' : ''}
            ${u.vk ? '<span class="user-badge">VK</span>' : ''}
            ${u.max ? '<span class="user-badge">MAX</span>' : ''}
          </div>` : ''}
        </div>
        <div class="user-actions">
          <button class="btn-secondary user-btn-view" data-id="${id}">Профиль</button>
          <button class="btn-primary user-btn-invite" data-id="${id}" style="width:auto;padding:0 1rem;height:36px;">
            <span class="btn-label">Пригласить</span>
          </button>
        </div>
      </div>`;
  }).join('');

  // bind
  grid.querySelectorAll('.user-btn-view').forEach(b => {
    b.addEventListener('click', () => {
      const u = items.find(x => String(idOf(x)) === String(b.dataset.id));
      openProfileWithInvite(b.dataset.id, u);
    });
  });
  grid.querySelectorAll('.user-btn-invite').forEach(b => {
    b.addEventListener('click', () => {
      const u = items.find(x => String(idOf(x)) === String(b.dataset.id));
      openInvitePicker(b.dataset.id, u ? fullName(u) : '');
    });
  });
  // клик по карточке (не по кнопке) — открыть профиль
  grid.querySelectorAll('.user-card').forEach(card => {
    card.addEventListener('click', e => {
      if (e.target.closest('button')) return;
      const u = items.find(x => String(idOf(x)) === String(card.dataset.id));
      openProfileWithInvite(card.dataset.id, u);
    });
  });

  updatePager();
}

// Открыть профиль с кнопкой «Пригласить в задачу» в футере
function openProfileWithInvite(userId, user) {
  openUserProfile(userId, {
    fallbackName: user ? fullName(user) : '',
    action: {
      label: 'Пригласить в задачу',
      onClick: async (closeProfile) => {
        closeProfile();
        openInvitePicker(userId, user ? fullName(user) : '');
      },
    },
  });
}

// ── Pagination ──────────────────────────────────────────────────────────────
function updatePager() {
  const page = Math.floor(offset / PAGE_SIZE) + 1;
  if (total != null) {
    const pages = Math.max(1, Math.ceil(total / PAGE_SIZE));
    pageLabel.textContent = `Стр. ${page} из ${pages}`;
    btnNext.disabled = page >= pages;
  } else {
    // total неизвестен: «next» доступен, если пришла полная страница
    pageLabel.textContent = `Стр. ${page}`;
    btnNext.disabled = items.length < PAGE_SIZE;
  }
  btnPrev.disabled = offset === 0;
  pager.hidden = false;
}

btnPrev.addEventListener('click', () => {
  if (offset === 0) return;
  offset = Math.max(0, offset - PAGE_SIZE);
  loadUsers();
});
btnNext.addEventListener('click', () => {
  offset += PAGE_SIZE;
  loadUsers();
});

// ── Search (debounced) ────────────────────────────────────────────────────────
let searchTimer = null;
searchInput.addEventListener('input', () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    searchQuery = searchInput.value.trim();
    offset = 0;
    loadUsers();
  }, 350);
});

// ── Invite flow ────────────────────────────────────────────────────────────────
// Приглашение = confirmation типа INVITE_TO_TASK.
// initiator = текущий пользователь (организатор), reviewer = приглашаемый волонтёр.

const inviteModal   = document.getElementById('modal-invite');
const inviteTaskList = document.getElementById('invite-task-list');
const inviteUserName = document.getElementById('invite-user-name');
const btnCancelInvite = document.getElementById('btn-cancel-invite');
let inviteUserId = null;

async function ensureMyTasks() {
  if (myTasks.length) return myTasks;
  try {
    // Берём первую большую страницу и отбираем свои задачи.
    // (Для приглашения обычно достаточно; при большом числе своих задач
    //  имеет смысл серверный фильтр по владельцу — см. примечание.)
    const resp = await tasksApi.getPage({ offset: 0, limit: 100, sort: 'newest' });
    const items = Array.isArray(resp) ? resp : (resp?.items ?? []);
    myTasks = (Array.isArray(items) ? items : []).filter(t => {
      const owner = t.consumerId ?? t.ownerId ?? t.creatorId ?? t.userId ?? t.organizerId;
      return owner && String(owner) === String(currentUserId);
    });
  } catch (e) {
    console.warn('[users] ensureMyTasks failed:', e.message);
    myTasks = [];
  }
  return myTasks;
}

async function openInvitePicker(userId, userName) {
  inviteUserId = userId;
  inviteUserName.textContent = userName || 'пользователя';
  inviteTaskList.innerHTML = '<div class="invite-loading">Загрузка ваших задач…</div>';
  inviteModal.hidden = false;
  document.body.style.overflow = 'hidden';

  const tasks = await ensureMyTasks();
  if (!tasks.length) {
    inviteTaskList.innerHTML = `<div class="invite-empty">У вас пока нет своих задач. Создайте задачу, чтобы приглашать волонтёров.</div>`;
    return;
  }

  inviteTaskList.innerHTML = tasks.map(t => `
    <button class="invite-task-item" data-task-id="${t.id}">
      <span class="invite-task-name">${escHtml(t.name || 'Без названия')}</span>
      <span class="invite-task-arrow">→</span>
    </button>`).join('');

  inviteTaskList.querySelectorAll('.invite-task-item').forEach(btn => {
    btn.addEventListener('click', () => sendInvite(btn.dataset.taskId, btn));
  });
}

function closeInvite() {
  inviteModal.hidden = true;
  document.body.style.overflow = '';
  inviteUserId = null;
}
btnCancelInvite.addEventListener('click', closeInvite);
inviteModal.addEventListener('click', e => { if (e.target === inviteModal) closeInvite(); });
document.addEventListener('keydown', e => {
  if (e.key === 'Escape' && !inviteModal.hidden) closeInvite();
});

async function sendInvite(taskId, btn) {
  if (!inviteUserId || !taskId) return;
  const task = myTasks.find(t => String(t.id) === String(taskId));
  btn.disabled = true;
  try {
    await confirmationsApi.create({
      confirmationType: CONFIRMATION_TYPES.INVITE_TO_TASK,
      entityId:         taskId,
      reviewerId:       inviteUserId,        // приглашаемый волонтёр принимает решение
      expirationHours:  CONFIRMATION_DEFAULT_EXPIRATION_HOURS,
      metaData: {
        taskName:          task?.name || '',
        inviterUsername:   tokenStore.getUsername() || '',
      },
    });
    toast.success('Приглашение отправлено');
    closeInvite();
  } catch (e) {
    toast.error(e.message || 'Не удалось отправить приглашение');
    btn.disabled = false;
  }
}

// ── Logout ────────────────────────────────────────────────────────────────────
document.getElementById('btn-logout')?.addEventListener('click', async () => {
  const refresh = tokenStore.getRefresh();
  tokenStore.clear();
  if (refresh) await authApi.logout(refresh).catch(() => {});
  window.location.href = 'index.html#login';
});

// ── Boot ──────────────────────────────────────────────────────────────────────
loadUsers();
startNotifications();
