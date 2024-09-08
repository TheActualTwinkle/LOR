namespace TelegramBotApp.Identity.Services.NstuGroupService.NstuGroupContext;

public record NstuGroupReply
{
    public string GroupName { get; }
    public string FormattedFullName { get; }

    private NstuGroupReply(string groupName, string formattedFullName) =>
        (GroupName, FormattedFullName) = (groupName, formattedFullName);

    public static NstuGroupReply Create(string groupName, string formattedFullName) =>
        new(groupName, formattedFullName);
}