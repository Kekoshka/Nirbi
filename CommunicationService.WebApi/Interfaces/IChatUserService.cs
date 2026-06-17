using CommunicationService.DataAccess.Postgres.Models;
using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Interfaces
{
    public interface IChatUserService
    {
        Task JoinChatUserAsync(Guid chatId, Guid userId, CancellationToken cancellationToken);
        Task RemoveChatUserAsync(Guid chatId, Guid userId, CancellationToken cancellationToken);
        Task<List<GetChatUsersResponse>> GetChatUsersAsync(Guid chatId, CancellationToken cancellationToken);
    }
}
