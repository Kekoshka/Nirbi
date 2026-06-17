using CommunicationService.DataAccess.Postgres.Context;
using CommunicationService.DataAccess.Postgres.Models;
using CommunicationService.WebApi.Common.Mappers;
using CommunicationService.WebApi.Interfaces;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Services
{
    public class ChatUserService  : IChatUserService
    {
        AppDbContext _context;
        ICurrentUserService _currentUserService;
        public ChatUserService(
            AppDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task JoinChatUserAsync(Guid chatId, Guid userId, CancellationToken cancellationToken)
        {
            var existedChatUser = await _context.ChatUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ChatId == chatId && !cu.IsDeleted, cancellationToken);
            if (existedChatUser is not null)
                throw new ConflictException("Пользователь уже состоит в чате!");

            var existedChat = await _context.Chats
                .Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
            if (existedChat is null)
                throw new NotFoundException($"Чат с id {chatId} не найден");

            ChatUser chatUser = new(
                chatId, 
                userId, 
                existedChat.ChatUsers.Select(cu => cu.UserId).ToList());

            _context.ChatUsers.Add(chatUser);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveChatUserAsync(Guid chatId, Guid userId, CancellationToken cancellationToken)
        {
            var existedChatUser = await _context.ChatUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ChatId == chatId && !cu.IsDeleted, cancellationToken);
            if (existedChatUser is null)
                throw new ConflictException("Пользователь не состоит в чате");
            var chatUsers = await _context.ChatUsers.Where(cu => cu.ChatId == chatId && cu.IsDeleted == false).Select(c => c.UserId).ToListAsync(cancellationToken);

            existedChatUser.RemoveUser(chatUsers);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<GetChatUsersResponse>> GetChatUsersAsync(Guid chatId, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();
            var chatUsers = await _context.ChatUsers.Where(cu => cu.ChatId == chatId && !cu.IsDeleted).ToListAsync(cancellationToken);
            if (!chatUsers.Any(cu => cu.UserId == currentUserId))
                throw new ForbiddenException("У вас нет доступа к этому чату!");
            return chatUsers.ToGetChatUsersResponse();
        }
    }
}
