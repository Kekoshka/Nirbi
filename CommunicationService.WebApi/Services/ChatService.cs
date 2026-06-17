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
    public class ChatService : IChatService
    {
        AppDbContext _context;
        ICurrentUserService _currentUserService;
        public ChatService(
            AppDbContext context,
            ICurrentUserService currentUserService) 
        {
            _context = context;
            _currentUserService = currentUserService;
        }
        public async Task<Guid> CreateChatAsync(List<Guid> users, CancellationToken cancellationToken)
        {
            if (users.Count != 2)
                throw new BadRequestException("В частном чате должно быть 2 пользователя!");

            var existedChat = _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefault(c => 
                    c.ChatUsers.Any(u => u.Id == users[0]) &&
                    c.ChatUsers.Any(u => u.Id == users[1]) &&
                    c.ChatTypeId == Common.Enums.ChatType.Private);
            if (existedChat != null)
                throw new NotFoundException("Чат между пользователями уже существует!");

            Chat chat = new(
                string.Empty,
                Common.Enums.ChatType.Private,
                users);
            await _context.Chats.AddAsync(chat, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            chat.ChatUsers.ToList().AddRange(users.Select(u => new ChatUser(chat.Id, u, users)));
            await _context.SaveChangesAsync(cancellationToken);

            return chat.Id;
        }

        public async Task<Guid> CreateGroupChatAsync(CreateGroupChatRequest request, CancellationToken cancellationToken)
        {
            Chat chat = new(
                request.ChatName, 
                Common.Enums.ChatType.Group, 
                request.Users);
            await _context.Chats.AddAsync(chat, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            chat.ChatUsers.ToList().AddRange(request.Users.Select(u => new ChatUser(chat.Id,u, request.Users)));
            await _context.SaveChangesAsync(cancellationToken);
            return chat.Id;
        }

        public async Task<List<GetChatsResponse>> GetChatsAsync(CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == currentUserId))
                .ToListAsync(cancellationToken);

            return chats.ToGetChatsResponse();
        }
    }
}
