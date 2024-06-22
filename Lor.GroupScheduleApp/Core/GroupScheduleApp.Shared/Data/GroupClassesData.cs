namespace GroupScheduleApp.Shared;

public readonly struct GroupClassesData(string groupName, List<ClassData> classes)
{
    public string GroupName { get; } = groupName;
    public List<ClassData> Classes { get; } = classes;
}