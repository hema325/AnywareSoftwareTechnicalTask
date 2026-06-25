using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistance.Interceptors
{
    internal class PublishDomainEventsInterceptor: SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;
        private readonly List<EventBase> _domainEvents;

        public PublishDomainEventsInterceptor(IPublisher publisher)
        {
            _publisher = publisher;
            _domainEvents = new();
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            // capture domain events
            var context = eventData.Context;

            if (context is null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var entries = context.ChangeTracker
                .Entries<EntityBase>()
                .Where(e => e.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = entries
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            entries.ForEach(entry => entry.Entity.ClearDomainEvents());

            _domainEvents.Clear();
            _domainEvents.AddRange(domainEvents);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            // publish domain events
            var publishTasks = _domainEvents.Select(e => _publisher.Publish(e, cancellationToken));
            await Task.WhenAll(publishTasks);
            _domainEvents.Clear();

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
