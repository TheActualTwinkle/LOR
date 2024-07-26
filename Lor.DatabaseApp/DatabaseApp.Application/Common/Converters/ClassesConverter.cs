using DatabaseApp.Application.Class;

namespace DatabaseApp.Application.Common.Converters;

public static class ClassesConverter
{
    public static Task Handle(this ClassDto classDto, Dictionary<int, string> dictionary)
    {
        classDto.ClassList = dictionary;

        return Task.CompletedTask;
    }
}