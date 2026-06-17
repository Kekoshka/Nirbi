namespace CommunicationService.DataAccess.Postgres.Models
{
    public class ChatType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Chat> Chats { get; set; }
    }
}
