import { api } from './api.js';

const PRIVATE_CHAT_TYPE_ID  = 'c0e3b007-f0bc-4c92-9f32-a87f1582812b';
const GROUP_CHAT_TYPE_ID    = 'b36b1c9c-1647-4785-9573-e4b225cf35ae';

export { PRIVATE_CHAT_TYPE_ID, GROUP_CHAT_TYPE_ID };

export const chatApi = {
  // GET /api/chats — список чатов текущего пользователя
  getChats() {
    return api.get('/api/chats');
  },

  // GET /api/chat/{chatId}/chatUsers
  getChatUsers(chatId) {
    return api.get(`/api/chat/${chatId}/chatUsers`);
  },

  // POST /api/Messages — отправить в существующий групповой чат
  sendGroupMessage(chatId, content) {
    return api.post('/api/Messages', { chatId, content });
  },

  // POST /api/Messages/private — отправить личное сообщение (создаёт чат при необходимости)
  sendPrivateMessage(recipientId, content) {
    return api.post('/api/Messages/private', { recipient: recipientId, content });
  },

  // GET /api/Messages?chatId= — история сообщений чата
  getMessages(chatId) {
    return api.get(`/api/Messages?chatId=${chatId}`);
  },

  // GET /api/Messages/preview — превью последних сообщений для списка чатов
  getPreviewMessages(chatIds) {
    if (!chatIds || !chatIds.length) return Promise.resolve([]);
    const qs = chatIds.map(id => `chatIds=${encodeURIComponent(id)}`).join('&');
    return api.get(`/api/Messages/preview?${qs}`);
  },

  // PUT /api/Messages/{messageId}
  updateMessage(messageId, content) {
    return api.put(`/api/Messages/${messageId}`, { messageId, content });
  },

  // DELETE /api/Messages/{messageId}
  deleteMessage(messageId) {
    return api.delete(`/api/Messages/${messageId}`);
  },
};
