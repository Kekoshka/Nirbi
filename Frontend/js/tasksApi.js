import { api } from './api.js';

export const tasksApi = {
  getAll(limit = 50, from = 0) {
    return api.get(`/api/tasks?limit=${limit}&from=${from}`);
  },

  getById(id) {
    return api.get(`/api/tasks/${id}`);
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

  getStatuses() {
    return api.get('/api/statuses');
  },
};
