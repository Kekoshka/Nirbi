using CommunicationService.DataAccess.Postgres.Context;
using CommunicationService.DataAccess.Postgres.Models;
using CommunicationService.WebApi.Common.DTO;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Services
{
    public class MessageService : IMessageService
    {
        AppDbContext _context;
        ICurrentUserService _currentUserService;
        IChatService _chatService;
        public MessageService(
            AppDbContext context,
            ICurrentUserService currentUserService,
            IChatService chatService) 
        { 
            _context = context;
            _currentUserService = currentUserService;
            _chatService = chatService;
        }

        public async Task<Guid> CreateMessageGroupChatAsync(CreateMessageGroupChatRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var existedChat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == request.ChatId &&
                    c.ChatUsers.Any(cu => cu.UserId == currentUserId),
                    cancellationToken);
            if (existedChat is null)
                throw new NotFoundException("Чат не найден");

            Message message = new(
                currentUserId,
                request.ChatId,
                request.Content,
                existedChat.ChatUsers
                    .Where(cu => cu.UserId != currentUserId)
                    .Select(cu => cu.UserId)
                    .ToList());
            _context.Messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            return message.Id;
        }
        
        public async Task<Guid> CreateMessagePrivateChatAsync(CreateMessagePrivateChatRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var existedChat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c =>
                    c.ChatTypeId == Common.Enums.ChatType.Private &&
                    c.ChatUsers.Any(u => u.UserId == request.Recipient && !u.IsDeleted) &&
                    c.ChatUsers.Any(u => u.UserId == currentUserId && !u.IsDeleted),
                    cancellationToken);
            if (existedChat is null)
            {
                var chatId = await _chatService.CreateChatAsync(
                    new List<Guid>()
                    {
                        currentUserId,
                        request.Recipient
                    },
                    cancellationToken);
                existedChat = await _context.Chats
                    .Include(c => c.ChatUsers)
                    .FirstAsync(c => c.Id == chatId);
            }
            Message message = new(currentUserId,
                existedChat!.Id,
                request.Content,
                existedChat.ChatUsers
                    .Where(cu => cu.UserId != currentUserId)
                    .Select(cu => cu.UserId)
                    .ToList());

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            return message.Id;
        }

        public async Task UpdateMessageAsync(UpdateMessageRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var existedMessage = await _context.Messages
                .Include(m => m.Chat)
                .ThenInclude(c => c.ChatUsers)
                .FirstOrDefaultAsync(m => m.Id == request.MessageId && m.Sender == currentUserId, cancellationToken);
            if (existedMessage is null)
                throw new NotFoundException("Сообщение не найдено!");
            
            existedMessage.UpdateMessage(
                request.Content, 
                existedMessage.Chat.ChatUsers
                    .Where(cu => cu.UserId != currentUserId)
                    .Select(cu => cu.UserId)
                    .ToList());
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var existedMessage = await _context.Messages
                .Include(m => m.Chat)
                .ThenInclude(c => c.ChatUsers)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.Sender == currentUserId, cancellationToken);
            if (existedMessage is null)
                throw new NotFoundException("Сообщение не найдено!");

            existedMessage.RemoveMessage(
                existedMessage.Chat.ChatUsers
                    .Where(cu => cu.UserId != currentUserId)
                    .Select(cu => cu.UserId)
                    .ToList());
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<GetMessagesResponse>> GetMessagesByChatIdAsync(Guid chatId, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var messages = await _context.Chats
                .Include(c => c.ChatUsers)
                .Include(c => c.Messages)
                .Where(c => c.Id == chatId && c.ChatUsers.Any(cu => cu.UserId == currentUserId && !cu.IsDeleted))
                .Select(c => c.Messages)
                .FirstOrDefaultAsync(cancellationToken);
            if (messages is null)
                throw new NotFoundException("Сообщения не найдены");
            return messages.ToList().ToGetMessagesResponse();
        }

        public async Task<List<GetPreviewMessagesResponse>> GetPreviewMessagesAsync(List<Guid> chatIds, CancellationToken cancellationToken)
        {
            var previewMessages = await _context.Chats
                .Include(c => c.Messages)
                .Where(c => chatIds.Any(ci => ci == c.Id) && c.Messages.Count > 0)
                .Select(c => c.Messages.First())
                .ToListAsync(cancellationToken);
            return previewMessages!.ToGetPreviewMessagesResponse();
        }
    }
}
