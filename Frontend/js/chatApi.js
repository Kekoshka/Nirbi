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

  // POST /api/messages/group — отправить в существующий групповой чат
  sendGroupMessage(chatId, content) {
    return api.post('/api/messages/group', { chatId, content });
  },

  // POST /api/messages/private — отправить личное сообщение (создаёт чат при необходимости)
  sendPrivateMessage(recipientId, content) {
    return api.post('/api/messages/private', { recipient: recipientId, content });
  },

  // GET /api/chats/{chatId}/messages — история сообщений чата
  getMessages(chatId) {
    return api.get(`/api/chats/${chatId}/messages`);
  },

  // GET /api/messages/preview — превью последних сообщений для списка чатов
  getPreviewMessages(chatIds) {
    if (!chatIds || !chatIds.length) return Promise.resolve([]);
    const qs = chatIds.map(id => `chatIds=${encodeURIComponent(id)}`).join('&');
    return api.get(`/api/messages/preview?${qs}`);
  },

  // PUT /api/messages
  updateMessage(messageId, content) {
    return api.put('/api/messages', { messageId, content });
  },

  // DELETE /api/messages/{messageId}
  deleteMessage(messageId) {
    return api.delete(`/api/messages/${messageId}`);
  },
};
