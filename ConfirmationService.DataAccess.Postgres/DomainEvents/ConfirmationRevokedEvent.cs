using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfirmationService.DataAccess.Postgres.DomainEvents
{
    public record class ConfirmationRevokedEvent(Guid ConfirmationId, Guid ReviewerId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
