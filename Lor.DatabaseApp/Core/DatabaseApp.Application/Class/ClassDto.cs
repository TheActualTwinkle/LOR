using DatabaseApp.AppCommunication.Class;

namespace DatabaseApp.Application.Class;

public struct ClassDto
{
    public required List<ClassInfoDto> ClassList { get; set; }
}