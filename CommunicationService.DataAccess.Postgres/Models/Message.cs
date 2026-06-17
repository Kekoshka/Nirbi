using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;
using System.Linq.Expressions;

namespace CommunicationService.DataAccess.Postgres.Models
{
    public class Message : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public Guid Id { get; set; }
        public Guid Sender { get; set; }
        public Guid ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public string Content { get; set; }
        public Chat Chat { get; set; }

        public Message() { }
        public Message(Guid sender, Guid chatId, string content, List<Guid> chatUsers)
        {
            Id = Guid.NewGuid();
            Sender = sender;
            ChatId = chatId;
            CreatedAt = DateTime.UtcNow;
            IsUpdated = false;
            IsDeleted = false;
            Content = content;

            _domainEvents.Add(new MessageCreatedEvent(Id, Sender, ChatId, Content, CreatedAt, chatUsers));
        }

        public void UpdateMessage(string content, List<Guid> chatUsers)
        {
            Content = content;
            IsUpdated = true;

            _domainEvents.Add(new MessageUpdatedEvent(Id,ChatId,Content,chatUsers));
        }

        public void RemoveMessage(List<Guid> ChatUsers)
        {
            IsDeleted = true;

            _domainEvents.Add(new MessageDeletedEvent(Id, ChatId, ChatUsers));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
