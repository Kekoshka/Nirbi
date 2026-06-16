import { api } from './api.js';

export const confirmationsApi = {
  create(data) {
    return api.post('/api/Confirmations', data);
  },
  getById(id) {
    return api.get(`/api/Confirmations/${id}`);
  },
  getByReviewer() {
  return api.get('/api/Confirmations/reviewer');
  },
  getByInitiator() {
    return api.get('/api/Confirmations/initiator');
  },
  respond(confirmationId, isAccepted, rejectionReason = null) {
    return api.post(`/api/Confirmations/${confirmationId}/respond`, { isAccepted, rejectionReason });
  },
  revoke(id, initiatorId) {
    return api.post(`/api/Confirmations/${id}/revoke?initiatorId=${initiatorId}`, {});
  },
};
