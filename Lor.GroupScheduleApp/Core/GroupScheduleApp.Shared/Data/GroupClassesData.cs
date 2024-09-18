namespace GroupScheduleApp.Shared;

public readonly struct GroupClassesData(string groupName, IEnumerable<ClassData> classes)
{
    public string GroupName { get; } = groupName;
    public IEnumerable<ClassData> Classes { get; } = classes;
}