import { tokenStore } from './tokenStore.js';

const HUB_URL = '/notificationHub';

let connection = null;
const listeners = {
  ShowConfirmationCreated: [],
  ShowConfirmationRespond:  [],
  ShowConfirmationRevoked:  [],
};

export function onNotification(event, cb) {
  if (!listeners[event]) listeners[event] = [];
  listeners[event].push(cb);
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

  // Привязываем обработчики событий
  Object.keys(listeners).forEach(event => {
    connection.on(event, payload => {
      listeners[event].forEach(cb => {
        try { cb(payload); } catch (e) { console.error('[notifications] listener error', e); }
      });
    });
  });

  connection.onreconnected(() => console.log('[notifications] reconnected'));
  connection.onclose(err => console.warn('[notifications] connection closed', err));

  try {
    await connection.start();
    console.log('[notifications] connected to', HUB_URL);
  } catch (err) {
    console.error('[notifications] connect failed:', err);
    // Попробуем ещё раз через 5 секунд
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
