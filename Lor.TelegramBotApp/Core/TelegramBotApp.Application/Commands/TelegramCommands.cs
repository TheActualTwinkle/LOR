using System.Composition;
using System.Text;
using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Authorization;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.Commands;

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/start")]
[ExportMetadata(nameof(Description), "- выводит приветственное сообщение")]
public class StartTelegramCommand : ITelegramCommand
{
    public string Command => "/start";
    public string Description => "- выводит приветственное сообщение";
    
    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        const string message = "Привет! Данный бот служит для записи на лабораторные работы. Для получения справки введите /help";
        return Task.FromResult(new ExecutionResult(Result.Ok(message)));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/help")]
[ExportMetadata(nameof(Description), "- выводит это сообщение справки")]
public class HelpTelegramCommand : ITelegramCommand
{
    public string Command => "/help";
    public string Description => "- выводит это сообщение справки";
    
    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        StringBuilder message = new();

        foreach (string command in TelegramCommandFactory.GetAllCommandsInfo())
        {
            message.AppendLine(command);
        }
        
        return Task.FromResult(new ExecutionResult(Result.Ok(message.ToString())));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/groups")]
[ExportMetadata(nameof(Description), "- выводит поддерживаемые группы")]
public class GroupsTelegramCommand : ITelegramCommand
{
    public string Command => "/groups";
    public string Description => "- выводит поддерживаемые группы";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        Result<Dictionary<int,string>> result = await factory.DatabaseCommunicator.GetAvailableGroups();
        
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }
        
        StringBuilder message = new("Доступные группы:\n");
        foreach (KeyValuePair<int,string> idGroupPair in result.Value)
        {
            message.AppendLine(idGroupPair.Value);
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/auth")]
[ExportMetadata(nameof(Description), "<ФИО> - авторизует пользователя")]
public class SetGroupTelegramCommand : ITelegramCommand
{
    public string Command => "/auth";
    public string Description => "<ФИО> - авторизует пользователя";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        IEnumerable<string> argumentsList = arguments.ToList();
        if (argumentsList.Count() != 3)
        {
            return new ExecutionResult(Result.Fail("Неверное количество аргументов!\nПример использования: /auth Высоцкий Владимир Семёнович"));
        }
        
        string fullName = argumentsList.Aggregate((x, y) => $"{x} {y}");
        Result<AuthorizationReply> authorizeResult = await factory.AuthorizationService.TryAuthorize(new AuthorizationRequest(fullName));
        
        if (authorizeResult.IsFailed)
        {
            return new ExecutionResult(Result.Fail(authorizeResult.Errors.First()));
        }

        Result<string> setGroupResult = await factory.DatabaseCommunicator.TrySetGroup(chatId, authorizeResult.Value.Group);

        return setGroupResult.IsFailed ? new ExecutionResult(Result.Fail(setGroupResult.Errors.First())) : new ExecutionResult(Result.Ok(setGroupResult.Value));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/labs")]
[ExportMetadata(nameof(Description), "- выводит доступные лабораторные работы")]
public class GetAvailableLabClassesTelegramCommand : ITelegramCommand
{
    public string Command => "/labs";
    public string Description => "- выводит доступные лабораторные работы для выбранной группы";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        Result<Dictionary<int,string>> result = await factory.DatabaseCommunicator.GetAvailableLabClasses(chatId);
        
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }
        
        StringBuilder message = new("Доступные лабораторные работы:\n");
        foreach (KeyValuePair<int,string> idClassPair in result.Value)
        {
            message.AppendLine(idClassPair.Value);
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/hop")]
[ExportMetadata(nameof(Description), "записывает на лабораторную работу")]
public class EnqueueInClassTelegramCommand : ITelegramCommand
{
    public string Command => "/hop";
    public string Description => "- записывает на лабораторную работу";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        IDatabaseCommunicationClient databaseCommunicator = factory.DatabaseCommunicator;
        if (databaseCommunicator.IsUserInGroup(chatId).Result.IsFailed)
        {
            return new ExecutionResult(Result.Fail("Вы не авторизованы. Для авторизации введите /auth <ФИО>"));
        }

        IReplyMarkup replyMarkup = await CreateInlineKeyboardMarkupAsync(databaseCommunicator, chatId);
        return new ExecutionResult(Result.Fail("Выберите пару"), replyMarkup);
    }
    
    private async Task<IReplyMarkup> CreateInlineKeyboardMarkupAsync(IDatabaseCommunicationClient databaseCommunicator, long userId)
    {
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableLabClasses(userId);
        
        if (result.IsFailed)
        {
            return new ReplyKeyboardRemove();
        }
        
        List<InlineKeyboardButton[]> buttons = [];
        foreach (KeyValuePair<int,string> idClassPair in result.Value)
        {
            InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData(idClassPair.Value, $"!hop {idClassPair.Key}");
            buttons.Add([button]);
        }

        return new InlineKeyboardMarkup(buttons);
    }
}