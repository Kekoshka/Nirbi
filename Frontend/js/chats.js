/**
 * chats.js — Страница чатов
 *
 * Структура:
 *  - Левая панель: список чатов с превью последнего сообщения
 *  - Правая панель: переписка выбранного чата
 *  - Поддержка личных (Private) и групповых (Group) чатов
 *  - SignalR: ChatCreated, MessageCreated, MessageDeleted, MessageUpdated,
 *             UserJoined, UserRemoved — с дедупликацией
 *  - Уведомления (toast) о новых сообщениях в фоновых чатах
 */

import { toast }         from './toast.js';
import { tokenStore }    from './tokenStore.js';
import { authApi, usersApi } from './api.js';
import { chatApi, PRIVATE_CHAT_TYPE_ID, GROUP_CHAT_TYPE_ID } from './chatApi.js';
import { startNotifications, onNotification } from './notifications.js';
import { getTaskNameMap } from './taskNameMap.js';

// ── Guard ─────────────────────────────────────────────────────────────────────
if (!tokenStore.hasSession()) {
  window.location.href = 'index.html#login';
}
const currentUserId = tokenStore.getUserId();

// ── Constants ─────────────────────────────────────────────────────────────────
const DELETED_PLACEHOLDER = '(сообщение удалено)';
const MAX_PREVIEW_LENGTH  = 60;

// ── State ─────────────────────────────────────────────────────────────────────
let chats        = [];          // GetChatsResponse[]
let nameMap      = new Map();   // userId → displayName
let previews     = new Map();   // chatId → GetPreviewMessagesResponse
let activeChatId = null;        // ID открытого чата
let messages     = [];          // GetMessagesResponse[] текущего чата
let editingId    = null;        // ID редактируемого сообщения

// ── DOM ───────────────────────────────────────────────────────────────────────
const chatList      = document.getElementById('chat-list');
const chatEmpty     = document.getElementById('chat-list-empty');
const threadPanel   = document.getElementById('chat-thread');
const threadWelcome = document.getElementById('chat-welcome');
const threadHeader  = document.getElementById('thread-header');
const threadTitle   = document.getElementById('thread-title');
const threadSub     = document.getElementById('thread-sub');
const msgContainer  = document.getElementById('msg-container');
const msgInput      = document.getElementById('msg-input');
const btnSend       = document.getElementById('btn-send');
const editBanner    = document.getElementById('edit-banner');
const editBannerText = document.getElementById('edit-banner-text');
const btnCancelEdit = document.getElementById('btn-cancel-edit');
const btnSearchChats = document.getElementById('btn-search-chats');
const chatSearchWrap = document.getElementById('chat-search-wrap');
const chatSearchInput = document.getElementById('chat-search-input');

// ── Helpers ───────────────────────────────────────────────────────────────────
function escHtml(s) {
  return String(s ?? '').replace(/[&<>"']/g, c =>
    ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' }[c]));
}

function fmtTime(iso) {
  if (!iso) return '';
  const d = new Date(iso);
  const now = new Date();
  const isToday = d.toDateString() === now.toDateString();
  if (isToday) {
    return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  }
  return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
}

function fmtFullTime(iso) {
  if (!iso) return '';
  return new Date(iso).toLocaleString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function truncate(s, n = MAX_PREVIEW_LENGTH) {
  if (!s) return '';
  return s.length > n ? s.slice(0, n) + '…' : s;
}

function getDisplayName(userId) {
  return nameMap.get(String(userId)) || 'Пользователь';
}

function initials(name) {
  const parts = String(name || '?').split(' ').filter(Boolean);
  if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  return (parts[0]?.[0] ?? '?').toUpperCase();
}

/**
 * Возвращает отображаемое имя чата:
 * - Личный → ФИО собеседника
 * - Групповой → название из модели
 */
function getChatTitle(chat) {
  if (chat.chatTypeId === PRIVATE_CHAT_TYPE_ID) {
    const otherId = chat.chatUsers?.find(id => String(id) !== String(currentUserId));
    return otherId ? getDisplayName(otherId) : chat.name || 'Личный чат';
  }
  // Групповой чат задачи: Name содержит GUID MinorTaskId
  if (isGuid(chat.name)) {
    return taskNameCache.get(String(chat.id)) || chat.name; // покажем GUID пока не загрузили
  }
  return chat.name || 'Групповой чат';
}

/** Собрать все userId из списка чатов для батч-запроса имён */
function collectAllUserIds() {
  const ids = new Set();
  chats.forEach(c => (c.chatUsers || []).forEach(id => ids.add(String(id))));
  return [...ids];
}

function isGuid(s) {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(String(s ?? ''));
}
const taskNameCache = new Map();


// ── Name map (батч обогащение) ────────────────────────────────────────────────
async function enrichNames(ids) {
  if (!ids.length) return;
  const fresh = await usersApi.getUsernameMap(ids).catch(() => new Map());
  fresh.forEach((v, k) => nameMap.set(k, v));
}

async function enrichTaskNames() {
  const taskChats = chats.filter(
    c => c.chatTypeId === GROUP_CHAT_TYPE_ID && isGuid(c.name)
  );
  if (!taskChats.length) return;

  const taskIds = taskChats.map(c => c.name); // name === MinorTaskId
  try {
    const nameMap = await getTaskNameMap(taskIds);
    taskChats.forEach(c => {
      const name = nameMap.get(String(c.name).toLowerCase());
      if (name) taskNameCache.set(String(c.id), name);
    });
  } catch (e) {
    console.warn('[chats] enrichTaskNames failed:', e.message);
  }
}


// ── Load ──────────────────────────────────────────────────────────────────────
async function loadChats() {
  try {
    const raw = await chatApi.getChats();
    chats = Array.isArray(raw) ? raw : [];

    const allIds = collectAllUserIds();
    // Параллельно обогащаем имена пользователей И названия задач
    await Promise.all([
      enrichNames(allIds),
      enrichTaskNames(),
    ]);

    await loadPreviews();
    renderChatList();
  } catch (e) {
    console.error('[chats] load failed:', e);
    toast.error('Не удалось загрузить чаты');
  }
}

async function loadPreviews() {
  const ids = chats.map(c => c.id);
  if (!ids.length) return;
  try {
    const raw = await chatApi.getPreviewMessages(ids);
    const list = Array.isArray(raw) ? raw : [];
    previews.clear();
    list.forEach(p => previews.set(String(p.chatId), p));
  } catch (e) {
    console.warn('[chats] previews failed:', e.message);
  }
}

// ── Render chat list ──────────────────────────────────────────────────────────
let chatSearchQuery = '';

function renderChatList(query = chatSearchQuery) {
  let filtered = chats;
  if (query) {
    const q = query.toLowerCase();
    filtered = chats.filter(c => getChatTitle(c).toLowerCase().includes(q));
  }

  if (!filtered.length) {
    chatList.innerHTML = '';
    chatEmpty.hidden = false;
    return;
  }
  chatEmpty.hidden = true;

  // Сортируем: у кого есть превью с более поздней датой — выше
  filtered = [...filtered].sort((a, b) => {
    const pa = previews.get(String(a.id));
    const pb = previews.get(String(b.id));
    const ta = pa ? new Date(pa.createdAt).getTime() : 0;
    const tb = pb ? new Date(pb.createdAt).getTime() : 0;
    return tb - ta;
  });

  chatList.innerHTML = filtered.map(chat => {
    const title   = getChatTitle(chat);
    const preview = previews.get(String(chat.id));
    const isGroup = chat.chatTypeId === GROUP_CHAT_TYPE_ID;
    const isActive = String(chat.id) === String(activeChatId);
    const previewText = preview
      ? (preview.isDeleted ? DELETED_PLACEHOLDER : truncate(preview.content))
      : 'Нет сообщений';
    const previewSender = preview && isGroup && String(preview.sender) !== String(currentUserId)
      ? getDisplayName(preview.sender) + ': '
      : (preview && String(preview.sender) === String(currentUserId) ? 'Вы: ' : '');

    return `
      <div class="chat-item ${isActive ? 'active' : ''}" data-chat-id="${chat.id}" role="button" tabindex="0">
        <div class="chat-item-avatar ${isGroup ? 'group-avatar' : ''}">
          ${isGroup
            ? `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
               </svg>`
            : escHtml(initials(title))
          }
        </div>
        <div class="chat-item-body">
          <div class="chat-item-header">
            <span class="chat-item-name">${escHtml(title)}</span>
            ${preview ? `<span class="chat-item-time">${fmtTime(preview.createdAt)}</span>` : ''}
          </div>
          <div class="chat-item-preview">${escHtml(previewSender)}${escHtml(previewText)}</div>
        </div>
      </div>`;
  }).join('');

  chatList.querySelectorAll('.chat-item').forEach(el => {
    el.addEventListener('click', () => openChat(el.dataset.chatId));
    el.addEventListener('keydown', e => { if (e.key === 'Enter') openChat(el.dataset.chatId); });
  });
}

// ── Open chat ─────────────────────────────────────────────────────────────────
async function openChat(chatId) {
  activeChatId = String(chatId);
  editingId = null;
  hideEditBanner();
  msgInput.value = '';

  // Подсветить активный
  chatList.querySelectorAll('.chat-item').forEach(el => {
    el.classList.toggle('active', String(el.dataset.chatId) === activeChatId);
  });

  const chat = chats.find(c => String(c.id) === activeChatId);
  if (!chat) return;

  // Показываем шапку треда
  threadWelcome.hidden = true;
  threadPanel.hidden = false;

  const title = getChatTitle(chat);
  threadTitle.textContent = title;
  const isGroup = chat.chatTypeId === GROUP_CHAT_TYPE_ID;
  const members = chat.chatUsers?.length ?? 0;
  threadSub.textContent = isGroup ? `${members} участник${declMembers(members)}` : 'Личный чат';

  // Загружаем сообщения
  msgContainer.innerHTML = '<div class="msg-loading">Загрузка сообщений…</div>';
  try {
    const raw = await chatApi.getMessages(chatId);
    messages = Array.isArray(raw) ? raw : [];

    // Обогащаем имена отправителей
    const senderIds = [...new Set(messages.map(m => String(m.sender)))];
    await enrichNames(senderIds.filter(id => !nameMap.has(id)));

    renderMessages();
    scrollToBottom();
  } catch (e) {
    msgContainer.innerHTML = '<div class="msg-error">Не удалось загрузить сообщения</div>';
    console.error('[chats] getMessages failed:', e);
  }
}

function declMembers(n) {
  if (n % 10 === 1 && n % 100 !== 11) return '';
  if ([2,3,4].includes(n % 10) && ![12,13,14].includes(n % 100)) return 'а';
  return 'ов';
}

// ── Render messages ───────────────────────────────────────────────────────────
function renderMessages() {
  if (!messages.length) {
    msgContainer.innerHTML = '<div class="msg-empty">Пока нет сообщений. Напишите первым!</div>';
    return;
  }

  // Группируем по дате
  let lastDate = '';
  const items = messages.map(msg => {
    const isMine  = String(msg.sender) === String(currentUserId);
    const date    = msg.createdAt ? new Date(msg.createdAt).toLocaleDateString('ru-RU') : '';
    let dateSep   = '';
    if (date !== lastDate) {
      lastDate = date;
      dateSep = `<div class="msg-date-sep"><span>${escHtml(date)}</span></div>`;
    }
    return dateSep + renderMessage(msg, isMine);
  });

  msgContainer.innerHTML = items.join('');
  bindMessageActions();
}

function renderMessage(msg, isMine) {
  if (msg.isDeleted) {
    return `
      <div class="msg-row ${isMine ? 'mine' : 'theirs'}" data-msg-id="${msg.id}">
        <div class="msg-bubble deleted">${DELETED_PLACEHOLDER}</div>
      </div>`;
  }

  const senderName = isMine ? 'Вы' : escHtml(getDisplayName(msg.sender));
  const chat = chats.find(c => String(c.id) === activeChatId);
  const isGroup = chat?.chatTypeId === GROUP_CHAT_TYPE_ID;

  return `
    <div class="msg-row ${isMine ? 'mine' : 'theirs'}" data-msg-id="${msg.id}">
      ${(!isMine && isGroup)
        ? `<div class="msg-avatar" title="${senderName}">${escHtml(initials(senderName))}</div>`
        : ''}
      <div class="msg-bubble">
        ${(!isMine && isGroup) ? `<div class="msg-sender">${senderName}</div>` : ''}
        <div class="msg-text">${escHtml(msg.content)}</div>
        <div class="msg-meta">
          <span class="msg-time" title="${escHtml(fmtFullTime(msg.createdAt))}">${fmtTime(msg.createdAt)}</span>
          ${msg.isUpdated ? '<span class="msg-edited">ред.</span>' : ''}
        </div>
        ${isMine && !msg.isDeleted ? `
          <div class="msg-actions">
            <button class="msg-btn msg-btn-edit" data-msg-id="${msg.id}" title="Редактировать">
              <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5">
                <path d="M11 2l3 3-9 9H2v-3l9-9z"/>
              </svg>
            </button>
            <button class="msg-btn msg-btn-delete" data-msg-id="${msg.id}" title="Удалить">
              <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5">
                <polyline points="1 4 15 4"/><path d="M6 4V2h4v2"/><path d="M2 4l1 10h10L14 4"/>
              </svg>
            </button>
          </div>` : ''}
      </div>
    </div>`;
}

function bindMessageActions() {
  msgContainer.querySelectorAll('.msg-btn-edit').forEach(btn => {
    btn.addEventListener('click', () => startEdit(btn.dataset.msgId));
  });
  msgContainer.querySelectorAll('.msg-btn-delete').forEach(btn => {
    btn.addEventListener('click', () => confirmDelete(btn.dataset.msgId));
  });
}

function scrollToBottom(smooth = false) {
  msgContainer.scrollTo({
    top: msgContainer.scrollHeight,
    behavior: smooth ? 'smooth' : 'auto',
  });
}

// ── Edit mode ─────────────────────────────────────────────────────────────────
function startEdit(msgId) {
  const msg = messages.find(m => String(m.id) === String(msgId));
  if (!msg || msg.isDeleted) return;
  editingId = String(msgId);
  msgInput.value = msg.content;
  msgInput.focus();
  editBannerText.textContent = truncate(msg.content, 40);
  showEditBanner();
}

function showEditBanner() {
  if (editBanner) editBanner.hidden = false;
}
function hideEditBanner() {
  if (editBanner) editBanner.hidden = true;
  editingId = null;
}

btnCancelEdit?.addEventListener('click', () => {
  hideEditBanner();
  msgInput.value = '';
  msgInput.focus();
});

// ── Delete ────────────────────────────────────────────────────────────────────
async function confirmDelete(msgId) {
  // Используем простой confirm для удаления (можно заменить модалкой)
  if (!confirm('Удалить сообщение?')) return;
  try {
    await chatApi.deleteMessage(msgId);
    // Оптимистично помечаем (SignalR дообновит)
    const idx = messages.findIndex(m => String(m.id) === String(msgId));
    if (idx !== -1) {
      messages[idx] = { ...messages[idx], isDeleted: true };
      updateMessageInDom(messages[idx]);
    }
  } catch (e) {
    toast.error(e.message || 'Не удалось удалить сообщение');
  }
}

// ── Send ──────────────────────────────────────────────────────────────────────
async function sendMessage() {
  const text = msgInput.value.trim();
  if (!text || !activeChatId) return;

  const chat = chats.find(c => String(c.id) === activeChatId);
  if (!chat) return;

  btnSend.disabled = true;
  try {
    if (editingId) {
      // Редактирование
      await chatApi.updateMessage(editingId, text);
      const idx = messages.findIndex(m => String(m.id) === String(editingId));
      if (idx !== -1) {
        messages[idx] = { ...messages[idx], content: text, isUpdated: true };
        updateMessageInDom(messages[idx]);
      }
      hideEditBanner();
    } else {
      // Новое сообщение
      let msgId;
      if (chat.chatTypeId === PRIVATE_CHAT_TYPE_ID) {
        const otherId = chat.chatUsers?.find(id => String(id) !== String(currentUserId));
        msgId = await chatApi.sendPrivateMessage(otherId, text);
      } else {
        msgId = await chatApi.sendGroupMessage(activeChatId, text);
      }
      // Оптимистичный рендер (SignalR подтвердит)
      const optimistic = {
        id: msgId || crypto.randomUUID(),
        sender: currentUserId,
        chatId: activeChatId,
        content: text,
        createdAt: new Date().toISOString(),
        isUpdated: false,
        isDeleted: false,
        _optimistic: true,
      };
      messages.push(optimistic);
      appendMessageToDOM(optimistic, true);
      scrollToBottom(true);

      // Обновляем превью
      previews.set(activeChatId, {
        chatId: activeChatId,
        sender: currentUserId,
        content: text,
        createdAt: optimistic.createdAt,
      });
      renderChatList();
    }
    msgInput.value = '';
  } catch (e) {
    toast.error(e.message || 'Не удалось отправить сообщение');
  } finally {
    btnSend.disabled = false;
    msgInput.focus();
  }
}

btnSend?.addEventListener('click', sendMessage);
msgInput?.addEventListener('keydown', e => {
  if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
});

// ── DOM helpers ───────────────────────────────────────────────────────────────
function appendMessageToDOM(msg, isMine) {
  const wrapper = document.createElement('div');
  wrapper.innerHTML = renderMessage(msg, isMine);
  while (wrapper.firstChild) msgContainer.appendChild(wrapper.firstChild);
  bindMessageActions();
}

function updateMessageInDom(msg) {
  const isMine = String(msg.sender) === String(currentUserId);
  const row = msgContainer.querySelector(`[data-msg-id="${msg.id}"]`);
  if (!row) return;
  const wrapper = document.createElement('div');
  wrapper.innerHTML = renderMessage(msg, isMine);
  row.replaceWith(wrapper.firstElementChild);
  bindMessageActions();
}

// ── SignalR handlers ──────────────────────────────────────────────────────────

onNotification('ChatCreated', async payload => {
  if (!payload) return;
  if (chats.some(c => String(c.id) === String(payload.Id))) return;

  const newChat = {
    id: payload.Id,
    name: payload.Name,
    chatTypeId: payload.ChatTypeId,
    chatUsers: payload.ChatUsers || [],
  };
  chats.unshift(newChat);

  await Promise.all([
    enrichNames((payload.ChatUsers || []).filter(id => !nameMap.has(String(id)))),
    enrichTaskNames(), // <-- добавить
  ]);
  renderChatList();
});

onNotification('MessageCreated', payload => {
  if (!payload) return;
  const chatId  = String(payload.ChatId);
  const msgId   = String(payload.Id);
  const isMine  = String(payload.Sender) === String(currentUserId);

  // Дедупликация: такое сообщение уже есть?
  if (String(activeChatId) === chatId) {
    const existing = messages.findIndex(m => String(m.id) === msgId);
    if (existing !== -1) {
      // Уже добавлено оптимистично — обновляем данные, если нужно
      if (messages[existing]._optimistic) {
        messages[existing] = { ...messages[existing], ...normalizeMsg(payload), _optimistic: false };
        updateMessageInDom(messages[existing]);
      }
      return;
    }

    const msg = normalizeMsg(payload);
    messages.push(msg);

    enrichNames([String(msg.sender)].filter(id => !nameMap.has(id))).then(() => {
      appendMessageToDOM(msg, isMine);
      scrollToBottom(true);
    });
  } else {
    // Чат не открыт — показываем уведомление
    if (!isMine) {
      const chat = chats.find(c => String(c.id) === chatId);
      const chatTitle = chat ? getChatTitle(chat) : 'Чат';
      const senderName = getDisplayName(payload.Sender);
      toast.action({
        type: 'info',
        title: `${senderName} → ${chatTitle}`,
        message: truncate(payload.Content, 80),
        duration: 6000,
        actions: [{
          label: 'Открыть',
          variant: 'primary',
          onClick: (dismiss) => { dismiss(); openChat(chatId); },
        }],
      });
    }
  }

  // Обновляем превью в списке чатов
  previews.set(chatId, {
    chatId,
    sender: payload.Sender,
    content: payload.Content,
    createdAt: payload.CreatedAt,
  });
  renderChatList();
});

onNotification('MessageUpdated', payload => {
  if (!payload) return;
  const msgId = String(payload.Id);
  const chatId = String(payload.ChatId);

  if (String(activeChatId) === chatId) {
    const idx = messages.findIndex(m => String(m.id) === msgId);
    if (idx === -1) return;
    // Проверяем: уже обновлено оптимистично с тем же контентом?
    if (messages[idx].content === payload.Content && messages[idx].isUpdated) return;
    messages[idx] = { ...messages[idx], content: payload.Content, isUpdated: true };
    updateMessageInDom(messages[idx]);
  }

  // Обновляем превью если это последнее сообщение
  const p = previews.get(chatId);
  if (p && String(p.id) === msgId) {
    previews.set(chatId, { ...p, content: payload.Content });
    renderChatList();
  }
});

onNotification('MessageDeleted', payload => {
  if (!payload) return;
  const msgId  = String(payload.Id);
  const chatId = String(payload.ChatId);

  if (String(activeChatId) === chatId) {
    const idx = messages.findIndex(m => String(m.id) === msgId);
    if (idx === -1) return;
    if (messages[idx].isDeleted) return; // уже помечено
    messages[idx] = { ...messages[idx], isDeleted: true };
    updateMessageInDom(messages[idx]);
  }
});

onNotification('UserJoined', async payload => {
  if (!payload) return;
  const chatId = String(payload.ChatId);
  const userId = String(payload.UserId);

  const chat = chats.find(c => String(c.id) === chatId);
  if (!chat) return;

  if (!chat.chatUsers.map(String).includes(userId)) {
    chat.chatUsers.push(userId);
  }

  // Если это текущий пользователь — перезагружаем список чатов
  if (userId === String(currentUserId)) {
    await loadChats();
    return;
  }

  await enrichNames([userId].filter(id => !nameMap.has(id)));

  if (String(activeChatId) === chatId) {
    const name = getDisplayName(userId);
    threadSub.textContent = `${chat.chatUsers.length} участник${declMembers(chat.chatUsers.length)}`;
    toast.info(`${name} присоединился к чату`);
  }
  renderChatList();
});

onNotification('UserRemoved', payload => {
  if (!payload) return;
  const chatId = String(payload.ChatId);
  const userId = String(payload.UserId);

  const chat = chats.find(c => String(c.id) === chatId);
  if (!chat) return;

  chat.chatUsers = chat.chatUsers.filter(id => String(id) !== userId);

  if (userId === String(currentUserId)) {
    // Нас удалили из чата
    if (String(activeChatId) === chatId) {
      activeChatId = null;
      threadPanel.hidden = true;
      threadWelcome.hidden = false;
      toast.warning('Вас удалили из чата');
    }
    chats = chats.filter(c => String(c.id) !== chatId);
    renderChatList();
    return;
  }

  const name = getDisplayName(userId);
  if (String(activeChatId) === chatId) {
    threadSub.textContent = `${chat.chatUsers.length} участник${declMembers(chat.chatUsers.length)}`;
    toast.info(`${name} покинул чат`);
  }
  renderChatList();
});

// ── Search ────────────────────────────────────────────────────────────────────
btnSearchChats?.addEventListener('click', () => {
  chatSearchWrap.hidden = !chatSearchWrap.hidden;
  if (!chatSearchWrap.hidden) chatSearchInput.focus();
  else {
    chatSearchInput.value = '';
    chatSearchQuery = '';
    renderChatList();
  }
});

chatSearchInput?.addEventListener('input', () => {
  chatSearchQuery = chatSearchInput.value.trim();
  renderChatList(chatSearchQuery);
});

// ── Textarea auto-grow ────────────────────────────────────────────────────────
msgInput?.addEventListener('input', () => {
  msgInput.style.height = 'auto';
  msgInput.style.height = Math.min(msgInput.scrollHeight, 160) + 'px';
});

// ── Logout ────────────────────────────────────────────────────────────────────
document.getElementById('btn-logout')?.addEventListener('click', async () => {
  const refresh = tokenStore.getRefresh();
  tokenStore.clear();
  if (refresh) await authApi.logout(refresh).catch(() => {});
  window.location.href = 'index.html#login';
});

// ── Reconnect: перезагрузить данные ──────────────────────────────────────────
window.addEventListener('signalr:reconnected', () => {
  loadChats();
  if (activeChatId) openChat(activeChatId);
});

// ── Normalise SignalR payload → GetMessagesResponse shape ─────────────────────
function normalizeMsg(p) {
  return {
    id:        p.Id        ?? p.id,
    sender:    p.Sender    ?? p.sender,
    chatId:    p.ChatId    ?? p.chatId,
    content:   p.Content   ?? p.content,
    createdAt: p.CreatedAt ?? p.createdAt,
    isUpdated: p.IsUpdated ?? p.isUpdated ?? false,
    isDeleted: p.IsDeleted ?? p.isDeleted ?? false,
  };
}

// ── Boot ──────────────────────────────────────────────────────────────────────
(async () => {
  await loadChats();
  startNotifications();

  // Если пришли с users.html по кнопке «Написать» — открываем нужный чат
  const openWithUserId = sessionStorage.getItem('nirbi_open_chat_with');
  if (openWithUserId) {
    sessionStorage.removeItem('nirbi_open_chat_with');
    // Ищем существующий личный чат с этим пользователем
    const existing = chats.find(c =>
      c.chatTypeId === PRIVATE_CHAT_TYPE_ID &&
      (c.chatUsers || []).map(String).includes(String(openWithUserId))
    );
    if (existing) {
      openChat(existing.id);
    } else {
      // Чата ещё нет — показываем «черновик»: пользователь вводит первое сообщение
      // и при отправке чат создастся автоматически через sendPrivateMessage
      await enrichNames([openWithUserId].filter(id => !nameMap.has(String(id))));
      const targetName = nameMap.get(String(openWithUserId)) || 'Пользователь';

      // Создаём временный объект чата для отображения UI
      const draft = {
        id: `draft_${openWithUserId}`,
        name: targetName,
        chatTypeId: PRIVATE_CHAT_TYPE_ID,
        chatUsers: [currentUserId, openWithUserId],
        _draft: true,
        _recipientId: openWithUserId,
      };
      chats.unshift(draft);
      renderChatList();
      activateDraftChat(draft);
    }
  }
})();

// ── Draft chat (личный чат до первого сообщения) ──────────────────────────
function activateDraftChat(draft) {
  activeChatId = String(draft.id);
  chatList.querySelectorAll('.chat-item').forEach(el => {
    el.classList.toggle('active', String(el.dataset.chatId) === activeChatId);
  });

  threadWelcome.hidden = true;
  threadPanel.hidden = false;
  threadTitle.textContent = draft.name;
  threadSub.textContent = 'Личный чат';
  messages = [];
  msgContainer.innerHTML = '<div class="msg-empty">Напишите первое сообщение, чтобы начать чат!</div>';
  msgInput.focus();

  // При отправке первого сообщения через draft — используем recipientId
  // Monkey-patch sendMessage для этого специального случая
  const origSend = sendMessage;
  const draftId = draft.id;
  const recipientId = draft._recipientId;

  // Перехватываем первую отправку
  async function draftSend() {
    const text = msgInput.value.trim();
    if (!text) return;

    btnSend.disabled = true;
    try {
      const msgId = await chatApi.sendPrivateMessage(recipientId, text);
      // После создания чата SignalR пришлёт ChatCreated — обновляем данные
      msgInput.value = '';
      toast.info('Сообщение отправлено');
      // Перезагружаем чаты — ChatCreated придёт через SignalR, но подстрахуемся
      await loadChats();
      // Открываем реальный чат
      const real = chats.find(c =>
        c.chatTypeId === PRIVATE_CHAT_TYPE_ID &&
        (c.chatUsers || []).map(String).includes(String(recipientId))
      );
      if (real) openChat(real.id);
    } catch (e) {
      toast.error(e.message || 'Не удалось отправить сообщение');
    } finally {
      btnSend.disabled = false;
    }
  }

  // Перекрываем обработчики для draft
  const sendBtn = document.getElementById('btn-send');
  const oldClick = sendBtn.onclick;

  sendBtn.onclick = () => draftSend();
  msgInput.onkeydown = e => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); draftSend(); }
  };

  // Восстановим стандартные обработчики после перехода на реальный чат
  // (это произойдёт в openChat через переприсвоение элементов)
}
