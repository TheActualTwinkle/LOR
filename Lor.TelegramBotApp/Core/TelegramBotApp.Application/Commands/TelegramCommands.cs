using System.Composition;
using System.Text;
using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Authorization;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.Commands;

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}start")]
[ExportMetadata(nameof(Description), "- выводит приветственное сообщение")]
public class StartTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}start";
    public string Description => "- выводит приветственное сообщение";
    public string? ButtonDescriptionText => null;

    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        const string message = $"Привет! Данный бот служит для записи на лабораторные работы.\n\n" +
                               $"Для начала следует авторизоваться, для этого напишите {TelegramCommandFactory.CommandPrefix}auth а после введите своё ФИО\n\n" +
                               $"Если все прошло успешно, бот назначит вам вашу группу и лабораторные работы, на которые вы можете записаться введя {TelegramCommandFactory.CommandPrefix}hop\n\n" +
                               $"Для получения справки введите {TelegramCommandFactory.CommandPrefix}help\n\n" +
                               $"Исходники проекта: https://github.com/TheActualTwinkle/LOR\nВопросы и предложения: @ext4zzzy\n\n";
        
        return Task.FromResult(new ExecutionResult(Result.Ok(message)));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}help")]
[ExportMetadata(nameof(Description), "- выводит это сообщение справки")]
public class HelpTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}help";
    public string Description => "- выводит это сообщение справки";
    public string? ButtonDescriptionText => null;
    
    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        StringBuilder message = new();

        foreach (var command in TelegramCommandFactory.GetAllCommandsInfo())
            message.AppendLine(command);

        return Task.FromResult(new ExecutionResult(Result.Ok(message.ToString())));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}groups")]
[ExportMetadata(nameof(Description), "- выводит поддерживаемые группы")]
// [ExportMetadata(nameof(ButtonDescriptionText), "Доступные группы \ud83d\ude4b\u200d\u2642\ufe0f\ud83d\ude4b\u200d\u2640\ufe0f")]
public class GroupsTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}groups";
    public string Description => "- выводит поддерживаемые группы";
    public string ButtonDescriptionText => "Доступные группы \ud83d\ude4b\u200d\u2642\ufe0f\ud83d\ude4b\u200d\u2640\ufe0f";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        StringBuilder message = new("Поддерживаемые группы:\n");
        
        var result = await factory.DatabaseCommunicator.GetAvailableGroups(cancellationToken);
        
        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        foreach (var idGroupPair in result.Value)
            message.AppendLine(idGroupPair.Value);

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}labs")]
[ExportMetadata(nameof(Description), "- выводит доступные лабораторные работы")]
// [ExportMetadata(nameof(ButtonDescriptionText), "Доступные лаб. работы \ud83d\udc68\u200d\ud83d\udd2c\ud83d\udc69\u200d\ud83d\udd2c")]
public class GetAvailableLabClassesTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}labs";
    public string Description => "- выводит доступные лабораторные работы для выбранной группы";
    public string ButtonDescriptionText => "Доступные лаб. работы \ud83d\udc68\u200d\ud83d\udd2c\ud83d\udc69\u200d\ud83d\udd2c";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var getUserGroupResult = await factory.DatabaseCommunicator.GetUserInfo(chatId, cancellationToken);
        if (getUserGroupResult.IsFailed)
            return new ExecutionResult(Result.Fail(getUserGroupResult.Errors.First().Message));

        StringBuilder message = new("Доступные лабораторные работы:\n");

        var getAvailableLabClassesResult = await factory.DatabaseCommunicator.GetAvailableLabClasses(chatId, cancellationToken);
        
        if (getAvailableLabClassesResult.IsFailed)
            return new ExecutionResult(Result.Fail(getAvailableLabClassesResult.Errors.First()));

        foreach (var classInformation in getAvailableLabClassesResult.Value)
        {
            var dateTime = classInformation.Date;
            message.AppendLine($"{classInformation.Name} {dateTime:dd.MM}");
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}{CommandWithoutPrefix}")]
[ExportMetadata(nameof(Description), "- записывает на лабораторную работу")]
[ExportMetadata(nameof(ButtonDescriptionText), "Записаться на лаб. работу \u270d\ufe0f")]
public class EnqueueInClassTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}{CommandWithoutPrefix}";
    public string Description => "- записывает на лабораторную работу";
    public string ButtonDescriptionText => "Записаться на лаб. работу \u270d\ufe0f";
    
    private const string CommandWithoutPrefix = "hop";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var databaseCommunicator = factory.DatabaseCommunicator;
        
        var result = await databaseCommunicator.GetUserInfo(chatId, cancellationToken);
        
        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        var availableLabClassesResult = await databaseCommunicator.GetAvailableLabClasses(chatId, cancellationToken);
        
        if (availableLabClassesResult.IsFailed)
            return new ExecutionResult(Result.Fail(availableLabClassesResult.Errors.First()));

        var replyMarkup = await MarkupCreator.CreateInlineKeyboardMarkupAsync(availableLabClassesResult.Value, CommandWithoutPrefix);
        
        return new ExecutionResult(Result.Fail("Выберите пару для ЗАПИСИ \u270d\ufe0f"), replyMarkup);
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}{CommandWithoutPrefix}")]
[ExportMetadata(nameof(Description), "- выписывает из очереди на лабораторную работу")]
[ExportMetadata(nameof(ButtonDescriptionText), "Отменить запись \ud83d\udeb7")]
public class DequeueFromClassTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}{CommandWithoutPrefix}";
    public string Description => "- выписывает из очереди на лабораторную работу";
    public string ButtonDescriptionText => "Отменить запись \ud83d\udeb7";

    private const string CommandWithoutPrefix = "dehop";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var databaseCommunicator = factory.DatabaseCommunicator;
        
        var result = await databaseCommunicator.GetUserInfo(chatId, cancellationToken);
        
        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        var availableLabClassesResult = await databaseCommunicator.GetAvailableLabClasses(chatId, cancellationToken);
        
        if (availableLabClassesResult.IsFailed)
            return new ExecutionResult(Result.Fail(availableLabClassesResult.Errors.First()));

        var replyMarkup = await MarkupCreator.CreateInlineKeyboardMarkupAsync(availableLabClassesResult.Value, CommandWithoutPrefix);
        
        return new ExecutionResult(Result.Fail("Выберите пару для ОТМЕНЫ ЗАПИСИ \ud83d\udeb7"), replyMarkup);
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}{CommandWithoutPrefix}")]
[ExportMetadata(nameof(Description), "- показывает очередь на лабораторную работу")]
[ExportMetadata(nameof(ButtonDescriptionText), "Посмотреть очередь \ud83e\uddfe")]
public class ViewQueueAtClass : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}{CommandWithoutPrefix}";
    public string Description => "- показывает очередь на лабораторную работу";
    public string ButtonDescriptionText => "Посмотреть очередь \ud83e\uddfe";

    private const string CommandWithoutPrefix = "queue";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var databaseCommunicator = factory.DatabaseCommunicator;
        
        var result = await databaseCommunicator.GetUserInfo(chatId, cancellationToken);
        
        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        var availableLabClassesResult = await databaseCommunicator.GetAvailableLabClasses(chatId, cancellationToken);
        
        if (availableLabClassesResult.IsFailed)
            return new ExecutionResult(Result.Fail(availableLabClassesResult.Errors.First()));

        var replyMarkup = await MarkupCreator.CreateInlineKeyboardMarkupAsync(availableLabClassesResult.Value, CommandWithoutPrefix);
        
        return new ExecutionResult(Result.Fail("Выберите пару для ПРОСМОТРА ОЧЕРЕДИ \ud83e\uddfe"), replyMarkup);
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}sub")]
[ExportMetadata(nameof(Description), "- подписывает на уведомления о появлении новых лабораторных работ")]
[ExportMetadata(nameof(ButtonDescriptionText), "Рассылка новых лаб. работ \u2709\ufe0f\u2705")]
public class AddSubscriberTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}sub";
    public string Description => "- подписывает на уведомления о появлении новых пар";
    public string ButtonDescriptionText => "Рассылка новых лаб. работ \u2709\ufe0f\u2705";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var result = await factory.DatabaseCommunicator.AddSubscriber(chatId, cancellationToken);
        
        return result.IsFailed ? new ExecutionResult(Result.Fail(result.Errors.First())) : new ExecutionResult(Result.Ok("Теперь вы будете получать уведомления о новых лабораторных работах"));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}unsub")]
[ExportMetadata(nameof(Description), "- отписывает от уведомлений о появлении новых лабораторных работ")]
[ExportMetadata(nameof(ButtonDescriptionText), "Отписаться от рассылки \u2709\ufe0f\u274c")]
public class DeleteSubscriberTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}unsub";
    public string Description => "- отписывает от уведомлений о появлении новых пар";
    public string ButtonDescriptionText => "Отписаться от рассылки \u2709\ufe0f\u274c";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var result = await factory.DatabaseCommunicator.DeleteSubscriber(chatId, cancellationToken);
        
        return result.IsFailed ? new ExecutionResult(Result.Fail(result.Errors.First())) : new ExecutionResult(Result.Ok("Вы отписаны от уведомлений о новых лабораторных работах"));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}auth")]
[ExportMetadata(nameof(Description), "- авторизует пользователя")]
[ExportMetadata(nameof(ButtonDescriptionText), "Авторизация \ud83d\udd11")]
public class AuthorizationTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}auth";
    public string Description => "- авторизует пользователя";
    public string ButtonDescriptionText => "Авторизация \ud83d\udd11";

    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var result = await factory.DatabaseCommunicator.GetUserInfo(chatId, cancellationToken);
        if (result.IsSuccess == true)
            return new ExecutionResult(Result.Ok($"Вы уже авторизованы в группе {result.Value.GroupName} как {result.Value.FullName}"));

        IEnumerable<string> argumentsList = arguments.ToList();
        
        if (!argumentsList.Any())
            return new ExecutionResult(Result.Fail($"Обработка {Command}\nОтветом на это сообщение введите, пожалуйста, ФИО для авторизации"), new ForceReplyMarkup());

        if (argumentsList.Count() != 2 && argumentsList.Count() != 3)
            return new ExecutionResult(Result.Fail($"Обработка {Command}\nОшибка при вводе ФИО. Ответом на это сообщение введите, пожалуйста, ФИО в формате: Фамилия Имя Отчество"), 
                new ForceReplyMarkup());

        var fullName = argumentsList.Aggregate((x, y) => $"{x} {y}");
        var authorizeResult = await factory.AuthorizationService.TryAuthorize(new AuthorizationRequest(fullName));
        
        if (authorizeResult.IsFailed)
            return new ExecutionResult(Result.Fail(authorizeResult.Errors.First()));

        var setGroupResult = await factory.DatabaseCommunicator.SetGroup(chatId, authorizeResult.Value.Group, authorizeResult.Value.FullName, cancellationToken);

        return setGroupResult.IsFailed ? new ExecutionResult(Result.Fail(setGroupResult.Errors.First())) : new ExecutionResult(Result.Ok(setGroupResult.Value));
    }
}

public static class MarkupCreator
{
    public static Task<IReplyMarkup> CreateInlineKeyboardMarkupAsync(IEnumerable<ClassDto> classes, string callbackQueryWithoutPrefix)
    {
        List<InlineKeyboardButton[]> buttons = [];
        
        foreach (var classInformation in classes)
        {
            var dateTime = classInformation.Date;
            
            var button = InlineKeyboardButton.WithCallbackData($"{classInformation.Name} {dateTime:dd.MM}",
                $"{TelegramCommandQueryFactory.CommandQueryPrefix}{callbackQueryWithoutPrefix} {classInformation.Name} {classInformation.Date}");
            
            buttons.Add([button]);
        }

        return Task.FromResult<IReplyMarkup>(new InlineKeyboardMarkup(buttons));
    }
}