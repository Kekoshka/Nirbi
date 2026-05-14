import { tokenStore } from './tokenStore.js';

// В Docker nginx проксирует /api/ → gateway, GATEWAY = ''
// Для локальной разработки без Docker укажи: const GATEWAY = 'http://localhost:8084'
const GATEWAY = '';

let refreshPromise = null;

async function refreshTokens() {
  const refresh = tokenStore.getRefresh();
  if (!refresh) throw new Error('No refresh token');

  const res = await fetch(`${GATEWAY}/api/Auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: refresh }),
  });

  if (!res.ok) {
    tokenStore.clear();
    throw new Error('Session expired');
  }

  const data = await res.json();
  tokenStore.save(data.accessToken, data.refreshToken, data.userId, null);
  return data.accessToken;
}

async function request(path, options = {}, retry = true) {
  // Ensure valid access token before request
  if (!tokenStore.isAccessValid() && tokenStore.hasSession()) {
    if (!refreshPromise) {
      refreshPromise = refreshTokens().finally(() => { refreshPromise = null; });
    }
    await refreshPromise;
  }

  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  const access = tokenStore.getAccess();
  if (access) headers['Authorization'] = `Bearer ${access}`;

  const res = await fetch(`${GATEWAY}${path}`, { ...options, headers });

  // Auto-retry once on 401
  if (res.status === 401 && retry && tokenStore.hasSession()) {
    if (!refreshPromise) {
      refreshPromise = refreshTokens().finally(() => { refreshPromise = null; });
    }
    try {
      await refreshPromise;
      return request(path, options, false);
    } catch {
      tokenStore.clear();
      window.location.href = 'index.html#login';
      throw new Error('Сессия истекла. Войдите снова.');
    }
  }

  if (!res.ok) {
    let message = `Ошибка ${res.status}`;
    try {
      const err = await res.json();
      message = err.message || err.error || err.errorMessage || message;
    } catch {}
    throw new Error(message);
  }

  const text = await res.text();
  return text ? JSON.parse(text) : null;
}

export const api = {
  get:    (path, opts)        => request(path, { method: 'GET',    ...opts }),
  post:   (path, body, opts)  => request(path, { method: 'POST',   body: JSON.stringify(body), ...opts }),
  put:    (path, body, opts)  => request(path, { method: 'PUT',    body: JSON.stringify(body), ...opts }),
  delete: (path, opts)        => request(path, { method: 'DELETE', ...opts }),
};

// ── Auth calls — без токена, с явной обработкой ошибок ──────────────────────
async function authRequest(url, body) {
  const res = await fetch(`${GATEWAY}${url}`, {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(body),
  });

  if (!res.ok) {
    let message = `Ошибка ${res.status}`;
    try {
      const e = await res.json();
      // Keycloak возвращает error_description, ASP.NET — message/title
      message = e.error_description || e.message || e.title || e.error || message;
    } catch {}
    throw new Error(message);
  }

  return res.json();
}

export const authApi = {
  login(username, password) {
    return authRequest('/api/Auth/login', { username, password });
  },
  register(username, email, password) {
    return authRequest('/api/Auth/register', { username, email, password });
  },
  forgotPassword(email) {
    return authRequest('/api/Auth/forgot-password', { email });
  },
  async logout(refreshToken) {
    await fetch(`${GATEWAY}/api/Auth/logout`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ refreshToken }),
    }).catch(() => {});
  },
};
