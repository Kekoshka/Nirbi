using MinorTaskService.WebApi.Common.DTO;
using Refit;

namespace MinorTaskService.WebApi.Common.ExternalApi
{
    public interface ICommunicationServiceApi
    {
        [Post("/api/groupChats")]
        Task<Guid> CreateGroupChatAsync(CreateGroupChatRequest request);

    }
}
