using System.Composition;
using System.Text;
using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.Commands;

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/help")]
[ExportMetadata(nameof(Description), "- выводит это сообщение справки")]
public class HelpTelegramCommand : ITelegramCommand
{
    public string Command => "/help";
    public string Description => "- выводит это сообщение справки";
    
    public IEnumerable<string> Arguments => [];
    
    public Task<Result<string>> Execute(long chatId, IGroupScheduleCommunicator scheduleCommunicator, CancellationToken cancellationToken)
    {
        StringBuilder message = new();

        foreach (string command in TelegramCommandFactory.GetAllCommandsInfo())
        {
            message.AppendLine(command);
        }

        return Task.FromResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/groups")]
[ExportMetadata(nameof(Description), "- выводит поддерживаемые группы")]
public class GroupsTelegramCommand : ITelegramCommand
{
    public string Command => "/groups";
    public string Description => "- выводит поддерживаемые группы";

    public IEnumerable<string> Arguments => [];
    
    public async Task<Result<string>> Execute(long chatId, IGroupScheduleCommunicator scheduleCommunicator, CancellationToken cancellationToken)
    {
        Result<Dictionary<int,string>> result = await scheduleCommunicator.GetAvailableGroups();
        
        if (result.IsFailed)
        {
            return Result.Fail("Не удалось получить список групп");
        }
        
        StringBuilder message = new();
        foreach (KeyValuePair<int,string> idGroupPair in result.Value)
        {
            message.AppendLine(idGroupPair.Value);
        }

        return Result.Ok(message.ToString());
    }
}

// [Export(typeof(ITelegramCommand))]
// [ExportMetadata(nameof(Command), "/classes")]
// [ExportMetadata(nameof(Description), "- ")]
// scheduleCommunicator.GetAvailableLabClasses();