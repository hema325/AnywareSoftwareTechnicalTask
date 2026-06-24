using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Tracing;

namespace Domain.Shared
{
    public abstract class EntityBase
    {
        public int Id { get; set; }

        #region domainEvents
        private List<EventBase> _domainEvents = new();

        [NotMapped]
        public IReadOnlyList<EventBase> DomainEvents => _domainEvents;

        public void AddDomainEvent(EventBase domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
        #endregion

    }
}
