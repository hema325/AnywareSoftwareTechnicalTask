using Application.TaskItems.Dtos;
using AutoMapper;

namespace Application.TaskItems.Mapping
{
    public sealed class TaskItemProfile : Profile
    {
        public TaskItemProfile()
        {
            CreateMap<TaskItem, TaskItemDto>()
                .ForMember(dto => dto.Status, options => options.MapFrom(task => task.Status.ToString()))
                .ForMember(dto => dto.Priority, options => options.MapFrom(task => task.Priority.ToString()));
        }
    }
}
