using DatabaseApp.Application.Group;

namespace DatabaseApp.Application.Common.Converters;

public static class GroupConverter
{
    public static Task Handle(this GroupDto groupDto, Dictionary<int, string> dictionary)
    {
        groupDto.GroupList = dictionary;

        return Task.CompletedTask;
    }
}