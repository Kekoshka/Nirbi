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

async function request(path, options = {}, retry = true, isForm = false) {
  // Ensure valid access token before request
  if (!tokenStore.isAccessValid() && tokenStore.hasSession()) {
    if (!refreshPromise) {
      refreshPromise = refreshTokens().finally(() => { refreshPromise = null; });
    }
    await refreshPromise;
  }

  // Для FormData не ставим Content-Type — браузер сам добавит boundary
  const headers = isForm
    ? { ...(options.headers || {}) }
    : { 'Content-Type': 'application/json', ...(options.headers || {}) };
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
  patch:  (path, body, opts)  => request(path, { method: 'PATCH',  body: JSON.stringify(body), ...opts }),
  delete: (path, opts)        => request(path, { method: 'DELETE', ...opts }),
  // multipart/form-data — Content-Type выставляет браузер (с boundary)
  postForm: (path, formData, opts) => request(path, { method: 'POST', body: formData, ...opts }, true, true),
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
  // Логин: username (email или телефон) + password
  login(username, password) {
    return authRequest('/api/Auth/login', { username, password });
  },

  // Регистрация: новый контракт бэкенда (FName, SName, LName, phone, email, password)
  register({ fName, sName, lName, phone, email, password }) {
    return authRequest('/api/Auth/register', {
      FName:    fName,
      SName:    sName,
      LName:    lName,
      phone,
      email,
      password,
    });
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

// ── User lookup — для обогащения данных задач именами организаторов ──────────
// Подтверждения обогащает Gateway-агрегатор (entityName/initiatorUsername/reviewerUsername),
// поэтому usersApi здесь нужен ТОЛЬКО для имён владельцев задач на карточках.
//
// Контракт AuthService (swagger):
//   GET /api/Users/users/{id}                  → UserProfile
//   GET /api/Users/users/fullnames?ids=&ids=   → [{ id, firstName, secondName, lastName }]
//   GET /api/Users/search?username=            → [{ userId, username }]
export const usersApi = {
  // Получить пользователя по ID
  getById(userId) {
    return api.get(`/api/Users/users/${userId}`);
  },

  // Батч ФИО по массиву ID: GET /api/Users/users/fullnames?ids=..&ids=..
  // Возвращает [{ id, firstName, secondName, lastName }]
  async getByIds(ids) {
    if (!ids || !ids.length) return [];
    const qs = ids.map(id => `ids=${encodeURIComponent(id)}`).join('&');
    try {
      const result = await api.get(`/api/Users/users/fullnames?${qs}`);
      return Array.isArray(result) ? result : [];
    } catch (e) {
      console.warn('[usersApi] getByIds failed:', e.message);
      return [];
    }
  },

  // Удобный хелпер: возвращает Map<userId, displayName> для быстрого lookup.
  // displayName собираем из ФИО; если его нет — username/userId как фолбэк.
  async getUsernameMap(ids) {
    const unique = [...new Set((ids || []).filter(Boolean).map(String))];
    if (!unique.length) return new Map();
    const users = await this.getByIds(unique);
    const map = new Map();
    users.forEach(u => {
      const id = u?.id ?? u?.userId;
      if (!id) return;
      const display = [u.lastName, u.firstName, u.secondName].filter(Boolean).join(' ')
                   || u.username
                   || String(id);
      map.set(String(id), display);
    });
    return map;
  },
};
