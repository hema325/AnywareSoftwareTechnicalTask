namespace Application.Common.Contracts
{
    public interface ITaskQueue
    {
        void Enqueue(int taskId);

        bool TryDequeue(out int taskId);
    }
}
