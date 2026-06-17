using CommunicationService.WebApi.Common.DataSeed;

namespace CommunicationService.WebApi.Common.Enums
{
    public static class ChatType
    {
        public static readonly Guid Group = ChatTypesSeed.ChatTypes[0].Id;
        public static readonly Guid Private = ChatTypesSeed.ChatTypes[1].Id;
    }
}
