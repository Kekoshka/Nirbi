using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces
{
    public interface IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
    }
}
