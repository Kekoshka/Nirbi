import { api } from './api.js';

export const tasksApi = {
  // Серверная пагинация + поиск + фильтр по статусу + сортировка.
  // Возвращает { total, items }.
  getPage({ offset = 0, limit = 20, search = '', status = '', sort = 'newest' } = {}) {
    const parts = [`offset=${offset}`, `limit=${limit}`];
    if (search) parts.push(`search=${encodeURIComponent(search)}`);
    if (status) parts.push(`status=${encodeURIComponent(status)}`);
    if (sort)   parts.push(`sort=${encodeURIComponent(sort)}`);
    return api.get(`/api/tasks?${parts.join('&')}`);
  },

  getById(id) {
    return api.get(`/api/tasks/${id}`);
  },

  // Батч-превью для ленивой загрузки картинок.
  // taskIds: string[] (пачка из 5-10 id). Возвращает
  // [{ taskId, previewImageData, previewImageContentType }] — только для задач,
  // у которых есть картинка.
  getPreviews(taskIds) {
    return api.post('/api/tasks/previews', { taskIds });
  },

  // Gateway /api/tasks принимает multipart/form-data
  // (см. CreateMinorTaskGatewayRequest: [FromForm], Images = List<IFormFile>).
  // Gateway сам кладёт файлы в DataService и форвардит UUID коллекции в MinorTaskService.
  // data: { name, description, latitude, longitude, numberVolunteers, encouragement }
  // images: File[] (опционально)
  create(data, images = []) {
    const fd = new FormData();
    fd.append('Name',             data.name);
    fd.append('Description',      data.description);
    fd.append('Latitude',         data.latitude);
    fd.append('Longitude',        data.longitude);
    fd.append('NumberVolunteers', data.numberVolunteers);
    fd.append('Encouragement',    data.encouragement);
    (images || []).forEach(file => fd.append('Images', file));
    return api.postForm('/api/tasks', fd);
  },

  update(id, data) {
    return api.patch(`/api/tasks/${id}`, data);
  },

  updateStatus(id, statusId) {
    return api.put(`/api/tasks/${id}`, { statusId });
  },

  delete(id) {
    return api.delete(`/api/tasks/${id}`);
  },

  removeParticipant(taskId, participantId) {
    return api.delete(`/api/tasks/${taskId}/participants/${participantId}`);
  },

  // Список участников задачи (GUID[]). Доступ на сервере: владелец или участник.
  getParticipants(taskId) {
    return api.get(`/api/tasks/${taskId}/participants`);
  },

  getStatuses() {
    return api.get('/api/statuses');
  },
};
