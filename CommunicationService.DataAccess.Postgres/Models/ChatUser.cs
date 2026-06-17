using CommunicationService.DataAccess.Postgres.DomainEvents;
using CommunicationService.DataAccess.Postgres.DomainEvents.Interfaces;

namespace CommunicationService.DataAccess.Postgres.Models
{
    public class ChatUser : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
        public Chat Chat { get; set; }
        public bool IsDeleted { get; set; }

        public ChatUser() { }
        public ChatUser(Guid chatId, Guid userId, List<Guid> chatUsers) 
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            UserId = userId;
            IsDeleted = false;

            _domainEvents.Add(new UserJoinedEvent(UserId,ChatId, chatUsers));
        }

        public void RemoveUser(List<Guid> chatUsers)
        {
            IsDeleted = true;

            _domainEvents.Add(new UserRemovedEvent(UserId, ChatId, chatUsers));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

}
