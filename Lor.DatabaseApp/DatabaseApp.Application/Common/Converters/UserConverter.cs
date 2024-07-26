using DatabaseApp.Application.User;

namespace DatabaseApp.Application.Common.Converters;

public static class UserConverter
{
    public static Task Handle(this UserDto userDto, string? groupName)
    {
        userDto.GroupName = groupName;

        return Task.CompletedTask;
    }
}