namespace CommunicationService.WebApi.Common.DTO
{
    public record class CreateGroupChatRequest(
        List<Guid> Users, string ChatName);
    public record class CreateMessageGroupChatRequest(
        Guid ChatId,
        string Content);
    public record class CreateMessagePrivateChatRequest(
        Guid Recipient,
        string Content);
    public record class UpdateMessageRequest(
        Guid MessageId,
        string Content);
}
