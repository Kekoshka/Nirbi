using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace CommunicationService.DataAccess.Postgres.Models
{
    public class Chat : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ChatTypeId { get; set; }
        public ChatType ChatType { get; set; }
        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<Message> Messages { get; set; }

        public Chat() { }
        public Chat(string name, Guid chatTypeId, List<Guid> chatUsers) 
        {
            Id = Guid.NewGuid();
            Name = name;
            ChatTypeId = chatTypeId;

            _domainEvents.Add(new ChatCreatedEvent(Id, Name, ChatTypeId, chatUsers));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
