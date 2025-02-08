namespace DatabaseApp.AppCommunication.Consumers.Data;

public record NewClassesMessage
{
    public required string GroupName { get; init; }
    public required IEnumerable<Class> Classes { get; init; }
}

public record Class
{
    public required string Name { get; init; }
    public required DateOnly Date { get; init; }
}