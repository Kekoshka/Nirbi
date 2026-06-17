using CommunicationService.DataAccess.Postgres.Models;
using System.Globalization;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Common.Mappers
{
    public static class ChatMappers
    {
        public static GetChatsResponse ToGetChatResponse(this Chat value) =>
            new GetChatsResponse(
                value.Id,
                value.Name,
                value.ChatTypeId,
                value.ChatUsers.Select(cu => cu.UserId).ToList());

        public static List<GetChatsResponse> ToGetChatsResponse(this List<Chat> value) =>
            value.Select(c => c.ToGetChatResponse()).ToList();
    }
}
