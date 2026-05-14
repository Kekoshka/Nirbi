import { api } from './api.js';

export const tasksApi = {
  getAll(limit = 50, from = 0) {
    return api.get(`/api/tasks?limit=${limit}&from=${from}`);
  },
  getById(id) {
    return api.get(`/api/tasks/${id}`);
  },
  create(data) {
    return api.post('/api/tasks', data);
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
