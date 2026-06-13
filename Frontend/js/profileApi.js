import { api } from './api.js';

// GET  /api/Users/users/{id}   → UserProfile
// PUT  /api/Users/users/{id}   → bool (UpdateUserRequest body)
// GET  /api/Users/users/fullnames?ids=...  → [{ id, firstName, secondName, lastName }]
// GET  /api/Users/search?username=...     → [{ userId, username }]

export const profileApi = {
  /**
   * Загрузить профиль пользователя по ID
   * @returns {Promise<UserProfile>}
   */
  getById(userId) {
    return api.get(`/api/Users/users/${userId}`);
  },

  /**
   * Обновить профиль пользователя
   * Тело: UpdateUserRequest (extends UserProfile + currentPassword + newPassword)
   * currentPassword обязателен на бэкенде.
   * @returns {Promise<boolean>}
   */
  update(userId, data) {
    return api.put(`/api/Users/users/${userId}`, data);
  },

  /**
   * Получить ФИО нескольких пользователей по списку ID
   * GET /api/Users/users/fullnames?ids=id1&ids=id2&...
   * @param {string[]} ids
   * @returns {Promise<Array<{id, firstName, secondName, lastName}>>}
   */
  getFullNames(ids) {
    if (!ids || !ids.length) return Promise.resolve([]);
    const qs = ids.map(id => `ids=${encodeURIComponent(id)}`).join('&');
    return api.get(`/api/Users/users/fullnames?${qs}`);
  },

  /**
   * Поиск пользователей по username (частичное совпадение)
   * @returns {Promise<Array<{userId, username}>>}
   */
  search(username) {
    return api.get(`/api/Users/search?username=${encodeURIComponent(username)}`);
  },
};
