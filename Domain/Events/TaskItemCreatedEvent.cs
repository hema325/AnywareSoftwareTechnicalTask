namespace Domain.Events
{
    public record TaskItemCreatedEvent(TaskItem Task) : EventBase;
}
