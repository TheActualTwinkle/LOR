using System.Composition;
using System.Text;
using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.Commands;

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/start")]
[ExportMetadata(nameof(Description), "- выводит приветственное сообщение")]
public class StartTelegramCommand : ITelegramCommand
{
    public string Command => "/start";
    public string Description => "- выводит приветственное сообщение";
    
    public Task<Result<string>> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Ok("Привет! Данный бот служит для записи на лабораторные работы. Для получения справки введите /help"));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/help")]
[ExportMetadata(nameof(Description), "- выводит это сообщение справки")]
public class HelpTelegramCommand : ITelegramCommand
{
    public string Command => "/help";
    public string Description => "- выводит это сообщение справки";
    
    public Task<Result<string>> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
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
    
    public async Task<Result<string>> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableGroups();
        
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors.First());
        }
        
        StringBuilder message = new();
        foreach (KeyValuePair<int,string> idGroupPair in result.Value)
        {
            message.AppendLine(idGroupPair.Value);
        }

        return Result.Ok(message.ToString());
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/setgroup")]
[ExportMetadata(nameof(Description), "<группа> - устанавливает группу")]
public class SetGroupTelegramCommand : ITelegramCommand
{
    public string Command => "/setgroup";
    public string Description => "<группа> - устанавливает группу";
    
    public async Task<Result<string>> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            return Result.Fail("Не указана группа");
        }
        
        string groupName = arguments.First();
        
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;

        Result<string> result = await databaseCommunicator.TrySetGroup(chatId, groupName);

        return result.IsFailed ? result : Result.Ok($"Группа {groupName} установлена");
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/labs")]
[ExportMetadata(nameof(Description), "- выводит доступные лабораторные работы")]
public class GetAvailableLabClassesTelegramCommand : ITelegramCommand
{
    public string Command => "/labs";
    public string Description => "- выводит доступные лабораторные работы для выбранной группы";
    
    public async Task<Result<string>> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableLabClasses(chatId);
        
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors.First());
        }
        
        StringBuilder message = new();
        foreach (KeyValuePair<int,string> idClassPair in result.Value)
        {
            message.AppendLine(idClassPair.Value);
        }

        return Result.Ok(message.ToString());
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/hop")]
[ExportMetadata(nameof(Description), "<номер пары> - записывает на лабораторную работу")]
public class EnqueueInClassTelegramCommand : ITelegramCommand
{
    public string Command => "/hop";
    public string Description => "<номер пары> - записывает на лабораторную работу";
    
    public async Task<Result<string>> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            return Result.Fail("Не указан номер пары");
        }
        
        if (int.TryParse(arguments.First(), out int classId) == false)
        {
            return Result.Fail("Номер пары должен быть числом");
        }
        
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<IEnumerable<string>> result = await databaseCommunicator.EnqueueInClass(classId, chatId);
        
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors.First());
        }
        
        StringBuilder message = new();
        foreach (string labClass in result.Value)
        {
            message.AppendLine(labClass);
        }

        return Result.Ok(message.ToString());
    }
}