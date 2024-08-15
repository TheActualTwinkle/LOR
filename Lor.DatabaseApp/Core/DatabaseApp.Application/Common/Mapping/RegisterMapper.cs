using DatabaseApp.Application.Class;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.Queue;
using DatabaseApp.Application.User;
using Mapster;

namespace DatabaseApp.Application.Common.Mapping;

public class RegisterMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Models.Class, ClassDto>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.GroupId, src => src.GroupId);
        config.NewConfig<Domain.Models.Group, GroupDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.GroupName, src => src.Name);
        config.NewConfig<Domain.Models.Queue, QueueDto>()
            .Map(dest => dest.ClassId, src => src.ClassId)
            .Map(dest => dest.FullName, src => src.User.FullName)
            .Map(dest => dest.QueueNum, src => src.QueueNum);
        config.NewConfig<Domain.Models.User, UserDto>()
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.GroupName, src => src.Group.Name);
    }
}