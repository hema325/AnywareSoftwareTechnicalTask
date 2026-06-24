namespace Domain.Events
{
    public record TaskItemUpdatedEvent(TaskItem Task) : EventBase;
}
