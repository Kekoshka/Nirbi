import { tokenStore } from './tokenStore.js';

const HUB_URL = '/notificationHub';

let connection = null;

// Все поддерживаемые события SignalR (подтверждения + чат)
const listeners = {
  // Подтверждения
  ShowConfirmationCreated: [],
  ShowConfirmationRespond: [],
  ShowConfirmationRevoked: [],

  // Чаты
  ChatCreated:    [],
  MessageCreated: [],
  MessageDeleted: [],
  MessageUpdated: [],
  UserJoined:     [],
  UserRemoved:    [],
};

/**
 * Подписаться на SignalR-событие.
 * Можно вызывать до startNotifications() — listeners накапливаются.
 */
export function onNotification(event, cb) {
  if (!listeners[event]) listeners[event] = [];
  listeners[event].push(cb);
}

/**
 * Отписаться от события (например при размонтировании страницы).
 */
export function offNotification(event, cb) {
  if (!listeners[event]) return;
  listeners[event] = listeners[event].filter(fn => fn !== cb);
}

export async function startNotifications() {
  if (!window.signalR) {
    console.warn('[notifications] @microsoft/signalr не загружен');
    return null;
  }
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => tokenStore.getAccess() || '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10_000, 30_000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  // Привязываем обработчики для всех событий
  Object.keys(listeners).forEach(event => {
    connection.on(event, payload => {
      let data = payload;
      if (typeof payload === 'string') {
        try { data = JSON.parse(payload); } catch { /* оставляем как есть */ }
      }
      listeners[event].forEach(cb => {
        try { cb(data); } catch (e) { console.error(`[notifications] listener error [${event}]`, e); }
      });
    });
  });

  connection.onreconnected(() => {
    console.log('[notifications] reconnected');
    // Уведомляем подписчиков о переподключении (они могут перезагрузить данные)
    window.dispatchEvent(new CustomEvent('signalr:reconnected'));
  });
  connection.onclose(err => console.warn('[notifications] connection closed', err));

  try {
    await connection.start();
    console.log('[notifications] connected to', HUB_URL);
  } catch (err) {
    console.error('[notifications] connect failed:', err);
    setTimeout(() => { connection = null; startNotifications(); }, 5000);
    return null;
  }

  return connection;
}

export function stopNotifications() {
  if (connection) {
    connection.stop().catch(() => {});
    connection = null;
  }
}

/** Возвращает текущее состояние соединения ('Connected' | 'Disconnected' | ...) */
export function getConnectionState() {
  return connection?.state ?? 'Disconnected';
}
