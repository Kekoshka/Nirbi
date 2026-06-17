using CommunicationService.WebApi.Common.DTO;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Interfaces
{
    public interface IMessageService
    {
        Task<Guid> CreateMessageGroupChatAsync(CreateMessageGroupChatRequest request, CancellationToken cancellationToken);
        Task<Guid> CreateMessagePrivateChatAsync(CreateMessagePrivateChatRequest request, CancellationToken cancellationToken);
        Task UpdateMessageAsync(UpdateMessageRequest request, CancellationToken cancellationToken);
        Task DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken);
        Task<List<GetMessagesResponse>> GetMessagesByChatIdAsync(Guid chatId, CancellationToken cancellationToken);
        Task<List<GetPreviewMessagesResponse>> GetPreviewMessagesAsync(List<Guid> chatIds, CancellationToken cancellationToken);
    }
}
