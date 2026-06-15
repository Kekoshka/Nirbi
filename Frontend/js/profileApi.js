import { api } from './api.js';

// Маршруты AuthService (через Gateway):
//   GET /api/Users/{id}?fields=...   → { id, ...поля } либо полный UserProfile
//   PUT /api/Users/{id}              → bool (UpdateUserRequest body)
//   GET /api/Users/fullnames?ids=... → [{ id, firstName, secondName, lastName }]
//   GET /api/Users/search?username=  → [{ userId, username }]
//   GET /api/Users/fields            → string[] доступных полей

// Полный набор полей профиля (включая мессенджеры vk/tg/max).
export const PROFILE_FIELDS = [
  'firstName', 'secondName', 'lastName', 'phone', 'email', 'username',
  'birthDate', 'city', 'about',
  'educationPlace', 'educationStartYear', 'educationEndYear', 'educationField',
  'vk', 'tg', 'max',
];

// Поля для публичного просмотра чужого профиля (без приватных данных).
export const PUBLIC_PROFILE_FIELDS = [
  'firstName', 'secondName', 'lastName', 'username', 'email', 'phone',
  'city', 'about',
  'educationPlace', 'educationField', 'educationStartYear', 'educationEndYear',
  'vk', 'tg', 'max',
];

function buildFieldsQuery(fields) {
  if (!fields || !fields.length) return '';
  return fields.map(f => `fields=${encodeURIComponent(f)}`).join('&');
}

export const profileApi = {
  /**
   * Загрузить профиль пользователя по ID.
   * fields — список желаемых полей. Бэкенд при наличии fields вернёт
   * Dictionary<field,value>; без fields — полный UserProfile.
   */
  getById(userId, fields = null) {
    const qs = buildFieldsQuery(fields);
    return api.get(`/api/Users/${userId}${qs ? `?${qs}` : ''}`);
  },

  /**
   * Обновить профиль. Тело: UpdateUserRequest (поля профиля + currentPassword/newPassword).
   */
  update(userId, data) {
    return api.put(`/api/Users/${userId}`, data);
  },

  getFullNames(ids) {
    if (!ids || !ids.length) return Promise.resolve([]);
    const qs = ids.map(id => `ids=${encodeURIComponent(id)}`).join('&');
    return api.get(`/api/Users/fullnames?${qs}`);
  },

  search(username) {
    return api.get(`/api/Users/search?username=${encodeURIComponent(username)}`);
  },

  // Список доступных полей профиля (из схемы Keycloak)
  getFields() {
    return api.get('/api/Users/fields');
  },
  // Примечание: список пользователей с пагинацией живёт в usersApi.list (api.js),
  // чтобы не дублировать логику. Используй usersApi.list({ offset, limit, search, fields }).
};
