using CommunicationService.DataAccess.Postgres.Models;
using static CommunicationService.WebApi.Common.DTO.ResponseDTO;

namespace CommunicationService.WebApi.Common.Mappers
{
    public static class MessageMappers
    {
        public static GetMessagesResponse ToGetMessagesResponse(this Message value) =>
            new GetMessagesResponse(
                value.Id,
                value.Sender,
                value.ChatId,
                value.CreatedAt,
                value.IsUpdated,
                value.IsDeleted,
                value.Content);

        public static List<GetMessagesResponse> ToGetMessagesResponse(this List<Message> value) =>
            value.Select(v => v.ToGetMessagesResponse()).ToList();

        public static GetPreviewMessagesResponse ToGetPreviewMessagesResponse(this Message value) =>
            new GetPreviewMessagesResponse(
                value.Id,
                value.Sender,
                value.ChatId,
                value.CreatedAt,
                value.Content);

        public static List<GetPreviewMessagesResponse> ToGetPreviewMessagesResponse(this List<Message> value) =>
            value.Select(v => v.ToGetPreviewMessagesResponse()).ToList();
    }
}
