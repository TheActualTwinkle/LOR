using DatabaseApp.Application.Class;
using DatabaseApp.Application.Group;
using DatabaseApp.Application.QueueEntries;
using DatabaseApp.Application.Subscriber;
using DatabaseApp.Application.User;
using Mapster;

namespace DatabaseApp.Application.Common.Mapping;

public class RegisterMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Models.Class, ClassDto>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Date, src => src.Date);
        config.NewConfig<Domain.Models.Group, GroupDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.GroupName, src => src.Name);
        config.NewConfig<Domain.Models.QueueEntry, QueueEntryDto>()
            .Map(dest => dest.ClassId, src => src.ClassId)
            .Map(dest => dest.FullName, src => src.User.FullName);
        config.NewConfig<Domain.Models.Subscriber, SubscriberDto>()
            .Map(dest => dest.TelegramId, src => src.User.TelegramId)
            .Map(dest => dest.GroupName, src => src.User.Group.Name);
        config.NewConfig<Domain.Models.User, UserDto>()
            .Map(dest => dest.TelegramId, src => src.Id)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.GroupName, src => src.Group.Name);
    }
}