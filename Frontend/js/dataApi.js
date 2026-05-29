import { api } from './api.js';

export const dataApi = {
  // Создать новую (пустую) коллекцию — возвращает её UUID
  createCollection() {
    return api.post('/api/collections', {});
  },

  // Загрузить файл в коллекцию — возвращает UUID файла
  uploadToCollection(collectionId, file) {
    const fd = new FormData();
    fd.append('file', file);
    return api.postForm(`/api/collections/${collectionId}/files`, fd);
  },

  listCollection(collectionId) {
    return api.get(`/api/collections/${collectionId}/files`);
  },

  deleteCollection(id) {
    return api.delete(`/api/collections/${id}`);
  },

  uploadStandalone(file) {
    const fd = new FormData();
    fd.append('file', file);
    return api.postForm('/api/files', fd);
  },

  getFileMetadata(id) {
    return api.get(`/api/files/${id}/metadata`);
  },

  // URL для прямой загрузки файла (можно подставить в <img src>)
  fileUrl(id) {
    return `/api/files/${id}`;
  },

  deleteFile(id) {
    return api.delete(`/api/files/${id}`);
  },

  // Хелпер: создать коллекцию и залить туда все файлы. Возвращает UUID коллекции.
  async uploadImagesAsCollection(files) {
    if (!files || !files.length) return null;
    const collectionId = await this.createCollection();
    // последовательно, чтобы сохранить порядок (sortOrder ставит сам бэкенд)
    for (const file of files) {
      await this.uploadToCollection(collectionId, file);
    }
    return collectionId;
  },
};
