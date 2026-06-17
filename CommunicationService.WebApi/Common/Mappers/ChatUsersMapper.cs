using CommunicationService.DataAccess.Postgres.Models;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Common.Mappers
{
    public static class ChatUsersMapper
    {
        public static GetChatUsersResponse ToGetChatUsersResponse(this ChatUser value) =>
            new GetChatUsersResponse(
                value.Id, 
                value.ChatId,
                value.UserId);

        public static List<GetChatUsersResponse> ToGetChatUsersResponse(this List<ChatUser> value) =>
             value.Select(v => v.ToGetChatUsersResponse()).ToList();
    }
}
