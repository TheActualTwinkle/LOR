using System.Composition;
using System.Text;
using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;
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
    
    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
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
    
    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
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
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableGroups();
        
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
[ExportMetadata(nameof(Command), "/setgroup")]
[ExportMetadata(nameof(Description), "- устанавливает группу")]
public class SetGroupTelegramCommand : ITelegramCommand
{
    public string Command => "/setgroup";
    public string Description => "- устанавливает группу";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            IReplyMarkup replyMarkup = await CreateInlineKeyboardMarkupAsync(telegramCommandFactory);
            return new ExecutionResult(Result.Fail("Выберете свою группу"), replyMarkup);
        }
        
        string groupName = arguments.First();
        
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;

        Result<string> result = await databaseCommunicator.TrySetGroup(chatId, groupName);

        return result.IsFailed ? new ExecutionResult(Result.Fail(result.Errors.First())) : new ExecutionResult(Result.Ok($"Группа {groupName} установлена"));
    }
    
    private async Task<IReplyMarkup> CreateInlineKeyboardMarkupAsync(TelegramCommandFactory telegramCommandFactory)
    {
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableGroups();
        
        if (result.IsFailed)
        {
            return new ReplyKeyboardRemove();
        }
        
        List<InlineKeyboardButton[]> buttons = [];
        foreach (KeyValuePair<int,string> idGroupPair in result.Value)
        {
            InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData(idGroupPair.Value, $"/setgroup {idGroupPair.Value}");
            buttons.Add([button]);
        }

        return new InlineKeyboardMarkup(buttons);
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), "/labs")]
[ExportMetadata(nameof(Description), "- выводит доступные лабораторные работы")]
public class GetAvailableLabClassesTelegramCommand : ITelegramCommand
{
    public string Command => "/labs";
    public string Description => "- выводит доступные лабораторные работы для выбранной группы";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableLabClasses(chatId);
        
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
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory telegramCommandFactory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            if (telegramCommandFactory.DatabaseCommunicator.IsUserInGroup(chatId).Result.IsFailed)
            {
                return new ExecutionResult(Result.Fail("Вы не состоите ни в одной группе. Установите группу командой /setgroup"));
            }

            IReplyMarkup replyMarkup = await CreateInlineKeyboardMarkupAsync(telegramCommandFactory, chatId);
            return new ExecutionResult(Result.Fail("Выберите пару"), replyMarkup);
        }
        
        if (int.TryParse(arguments.First(), out int classId) == false)
        {
            return new ExecutionResult(Result.Fail("Номер пары должен быть числом"));
        }
        
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<IEnumerable<string>> result = await databaseCommunicator.EnqueueInClass(classId, chatId);
        
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }
        
        StringBuilder message = new("Вы успешно записаны!\nОчередь:");
        foreach (string labClass in result.Value)
        {
            message.AppendLine(labClass);
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
    
    private async Task<IReplyMarkup> CreateInlineKeyboardMarkupAsync(TelegramCommandFactory telegramCommandFactory, long userId)
    {
        IDatabaseCommunicationClient databaseCommunicator = telegramCommandFactory.DatabaseCommunicator;
        Result<Dictionary<int,string>> result = await databaseCommunicator.GetAvailableLabClasses(userId);
        
        if (result.IsFailed)
        {
            return new ReplyKeyboardRemove();
        }
        
        List<InlineKeyboardButton[]> buttons = [];
        foreach (KeyValuePair<int,string> idClassPair in result.Value)
        {
            InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData(idClassPair.Value, $"/hop {idClassPair.Key}");
            buttons.Add([button]);
        }

        return new InlineKeyboardMarkup(buttons);
    }
}