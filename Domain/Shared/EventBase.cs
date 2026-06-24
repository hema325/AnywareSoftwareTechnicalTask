using MediatR;

namespace Domain.Shared
{
    public abstract record EventBase: INotification
    {
    }
}
