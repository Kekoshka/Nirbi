namespace CommunicationService.WebApi.Common.DTO
{
    public class ResponseDTO
    {
        public record class GetMessagesResponse(
            Guid Id,
            Guid Sender,
            Guid ChatId,
            DateTime CreatedAt,
            bool IsUpdated,
            bool IsDeleted,
            string Content);
        public record class GetChatsResponse(
            Guid Id,
            string Name,
            Guid ChatTypeId,
            List<Guid> ChatUsers);

        public record class GetChatUsersResponse(
            Guid Id,
            Guid ChatId,
            Guid UserId);

        public record class GetPreviewMessagesResponse(
            Guid Id,
            Guid ChatId,
            Guid Sender,
            DateTime CreatedAt,
            string Content);
    }
}
