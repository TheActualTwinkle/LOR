namespace GroupScheduleApp.Shared;

public readonly struct ClassData(string name, DateTime date)
{
    public string Name { get; } = name;
    public DateTime Date { get; } = date;

    public override string ToString()
    {
        return $"{Date:M}: {Name}";
    }
}