namespace Domain.Events
{
    public record TaskItemDeletedEvent(TaskItem Task) : EventBase;
}
