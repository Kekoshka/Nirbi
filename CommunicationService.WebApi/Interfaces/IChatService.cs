using CommunicationService.WebApi.Common.DTO;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Interfaces
{
    public interface IChatService
    {
        Task<Guid> CreateChatAsync(List<Guid> users, CancellationToken cancellationToken);
        Task<Guid> CreateGroupChatAsync(CreateGroupChatRequest request, CancellationToken cancellationToken);
        Task<List<GetChatsResponse>> GetChatsAsync(CancellationToken cancellationToken);
    }
}
