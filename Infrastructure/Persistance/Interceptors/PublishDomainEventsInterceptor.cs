using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistance.Interceptors
{
    internal class PublishDomainEventsInterceptor: SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;

        public PublishDomainEventsInterceptor(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            if (context is null)
            {
                return await base.SavedChangesAsync(eventData, result, cancellationToken);
            }

            var entries = context.ChangeTracker
                .Entries<EntityBase>()
                .Where(e => e.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = entries
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            foreach (var entry in entries)
            {
                entry.Entity.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
