namespace DatabaseApp.Application.Class;

public struct ClassDto
{
    public required List<AppCommunication.Grpc.ClassInformation> ClassList { get; set; }
}