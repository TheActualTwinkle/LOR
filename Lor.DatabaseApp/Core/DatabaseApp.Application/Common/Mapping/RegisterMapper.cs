using DatabaseApp.Application.Classes;
using DatabaseApp.Application.Groups;
using DatabaseApp.Application.QueueEntries;
using DatabaseApp.Application.Subscribers;
using DatabaseApp.Application.Users;
using DatabaseApp.Domain.Models;
using Lor.Shared.Messaging.Models;
using Mapster;

namespace DatabaseApp.Application.Common.Mapping;

public class RegisterMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Class, ClassDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Date, src => src.Date);
        
        config.NewConfig<Class, ClassModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Date, src => src.Date);

        config.NewConfig<Group, GroupDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.GroupName, src => src.Name);

        config.NewConfig<QueueEntry, QueueEntryDto>()
            .Map(dest => dest.ClassId, src => src.ClassId)
            .Map(dest => dest.FullName, src => src.User.FullName);

        config.NewConfig<Subscriber, SubscriberDto>()
            .Map(dest => dest.TelegramId, src => src.User.TelegramId)
            .Map(dest => dest.GroupName, src => src.User.Group.Name);

        config.NewConfig<User, UserDto>()
            .Map(dest => dest.TelegramId, src => src.TelegramId)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.GroupName, src => src.Group.Name);

        config.NewConfig<User, UserModel>()
            .Map(dest => dest.TelegramId, src => src.TelegramId);
    }
}