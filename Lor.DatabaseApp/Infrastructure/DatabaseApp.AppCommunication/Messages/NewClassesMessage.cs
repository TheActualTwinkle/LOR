using DatabaseApp.Application.Class;

namespace DatabaseApp.AppCommunication.Messages;

public record NewClassesMessage
{
    public required string GroupName { get; init; }
    public required IEnumerable<ClassDto> Classes { get; init; }
}