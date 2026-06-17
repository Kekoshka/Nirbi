// taskNameMap.js
// Кешированный резолвер: taskId → название задачи.
// Структура аналогична usersApi.getUsernameMap.

import { tasksApi } from './tasksApi.js';

// Кеш живёт на протяжении сессии страницы
const _cache = new Map(); // taskId (lowercase) → name

/**
 * Получить Map<taskId, name> для переданных ID.
 * Уже закешированные ID не запрашиваются повторно.
 *
 * @param {string[]} ids
 * @returns {Promise<Map<string, string>>}
 */
export async function getTaskNameMap(ids) {
  const unique = [...new Set((ids || []).filter(Boolean).map(id => String(id).toLowerCase()))];
  if (!unique.length) return new Map(_cache);

  const missing = unique.filter(id => !_cache.has(id));

  if (missing.length) {
    try {
      const raw = await tasksApi.getTaskNames(missing);
      const list = Array.isArray(raw) ? raw : [];
      list.forEach(item => {
        // Бэкенд может вернуть { id, name } или { minorTaskId, name }
        const id = String(item.id ?? item.minorTaskId ?? '').toLowerCase();
        if (id && item.name) _cache.set(id, item.name);
      });
    } catch (e) {
      console.warn('[taskNameMap] getTaskNames failed:', e.message);
    }
  }

  // Возвращаем только запрошенные
  const result = new Map();
  unique.forEach(id => {
    const name = _cache.get(id);
    if (name) result.set(id, name);
  });
  return result;
}

/**
 * Получить название одной задачи (использует кеш).
 * @param {string} id
 * @param {string} [fallback] — что вернуть если не найдено
 */
export async function getTaskName(id, fallback = null) {
  const map = await getTaskNameMap([id]);
  return map.get(String(id).toLowerCase()) ?? fallback ?? String(id);
}

/** Принудительно инвалидировать кеш (например после обновления задачи) */
export function invalidateTaskName(id) {
  _cache.delete(String(id).toLowerCase());
}