namespace TelegramBotApp.Identity.Services.NstuGroupService.NstuGroupContext;

public class NstuGroupRequest
{
    public string FullName { get; }
    public DateTime? DateOfBirth { get; }

    private NstuGroupRequest(string fullName, DateTime? dateOfBirth = null) =>
        (FullName, DateOfBirth) = (fullName, dateOfBirth);

    public static NstuGroupRequest Create(string fullName, DateTime? dateOfBirth = null) => new(fullName, dateOfBirth);
}