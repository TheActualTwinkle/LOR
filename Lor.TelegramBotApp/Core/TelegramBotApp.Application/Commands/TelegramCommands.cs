using System.Composition;
using System.Text;
using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Authorization;
using TelegramBotApp.Caching;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.Commands;

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}start")]
[ExportMetadata(nameof(Description), "- выводит приветственное сообщение")]
public class StartTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}start";
    public string Description => "- выводит приветственное сообщение";
    
    public Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        const string message = $"Привет! Данный бот служит для записи на лабораторные работы. Для получения справки введите {TelegramCommandFactory.CommandPrefix}help";
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
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}groups")]
[ExportMetadata(nameof(Description), "- выводит поддерживаемые группы")]
public class GroupsTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}groups";
    public string Description => "- выводит поддерживаемые группы";
    
    // TODO: DI? Config?
    private TimeSpan CacheExpirationTime => TimeSpan.FromMinutes(1);
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        StringBuilder message = new("Поддерживаемые группы:\n");

        Dictionary<int, string>? supportedGroups = await factory.CacheService.GetAsync<Dictionary<int, string>>(Constants.SupportedGroupsKey, cancellationToken);
        if (supportedGroups != null)
        {
            foreach (KeyValuePair<int, string> idGroupPair in supportedGroups)
            {
                message.AppendLine(idGroupPair.Value);
            }

            return new ExecutionResult(Result.Ok(message.ToString() + "\n (кэш)"));
        }
        
        Result<Dictionary<int, string>> result = await factory.DatabaseCommunicator.GetAvailableGroups(cancellationToken);
        
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }
        
        foreach (KeyValuePair<int, string> idGroupPair in result.Value)
        {
            message.AppendLine(idGroupPair.Value);
        }

        await factory.CacheService.SetAsync(Constants.SupportedGroupsKey, result.Value, CacheExpirationTime, cancellationToken);
        return new ExecutionResult(Result.Ok(message.ToString() + "\n (дб)"));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}auth")]
[ExportMetadata(nameof(Description), "- авторизует пользователя")]
public class AuthorizationTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}auth";
    public string Description => "- авторизует пользователя";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        Result<UserInfo> result = await factory.DatabaseCommunicator.GetUserInfo(chatId, cancellationToken);
        if (result.IsSuccess == true)
        {
            return new ExecutionResult(Result.Ok($"Вы уже авторизованы в группе {result.Value.GroupName} как {result.Value.FullName}"));
        }

        IEnumerable<string> argumentsList = arguments.ToList();
        
        if (argumentsList.Any() == false)
        {
            return new ExecutionResult(Result.Fail($"Обработка {Command}\nОтветом на это сообщение введите, пожалуйста, ФИО для авторизации"), new ForceReplyMarkup());
        }
        
        if (argumentsList.Count() != 2 && argumentsList.Count() != 3)
        {
            return new ExecutionResult(Result.Fail($"Обработка {Command}\nОшибка при вводе ФИО. Ответом на это сообщение введите, пожалуйста, ФИО в формате: Фамилия Имя Отчество"));
        }
        
        string fullName = argumentsList.Aggregate((x, y) => $"{x} {y}");
        Result<AuthorizationReply> authorizeResult = await factory.AuthorizationService.TryAuthorize(new AuthorizationRequest(fullName));
        
        if (authorizeResult.IsFailed)
        {
            return new ExecutionResult(Result.Fail(authorizeResult.Errors.First()));
        }

        Result<string> setGroupResult = await factory.DatabaseCommunicator.TrySetGroup(chatId, authorizeResult.Value.Group, authorizeResult.Value.FullName, cancellationToken);

        return setGroupResult.IsFailed ? new ExecutionResult(Result.Fail(setGroupResult.Errors.First())) : new ExecutionResult(Result.Ok(setGroupResult.Value));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}labs")]
[ExportMetadata(nameof(Description), "- выводит доступные лабораторные работы")]
public class GetAvailableLabClassesTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}labs";
    public string Description => "- выводит доступные лабораторные работы для выбранной группы";
    
    private TimeSpan CacheExpirationTime => TimeSpan.FromSeconds(15);
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        Result<UserInfo> getUserGroupResult = await factory.DatabaseCommunicator.GetUserInfo(chatId, cancellationToken);
        if (getUserGroupResult.IsFailed)
        {
            return new ExecutionResult(Result.Fail(getUserGroupResult.Errors.First()));
        }
        
        StringBuilder message = new("Доступные лабораторные работы:\n");
        
        Dictionary<int,string>? classes = await factory.CacheService.GetAsync<Dictionary<int, string>>($"{Constants.AvailableClassesHeader}{getUserGroupResult.Value}", cancellationToken);
        if (classes != null)
        {
            foreach (KeyValuePair<int,string> idClassPair in classes)
            {
                message.AppendLine(idClassPair.Value);
            }

            return new ExecutionResult(Result.Ok(message.ToString() + "\n (кэш)"));
        }

        Result<IEnumerable<ClassInformation>> getAvailableLabClassesResult = await factory.DatabaseCommunicator.GetAvailableLabClasses(chatId, cancellationToken);
        
        if (getAvailableLabClassesResult.IsFailed)
        {
            return new ExecutionResult(Result.Fail(getAvailableLabClassesResult.Errors.First()));
        }
        
        foreach (ClassInformation classInformation in getAvailableLabClassesResult.Value)
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(classInformation.ClassDateUnixTimestamp).DateTime;
            message.AppendLine($"{classInformation.ClassName} {dateTime:dd.MM}");
        }

        await factory.CacheService.SetAsync($"{Constants.AvailableClassesHeader}{getUserGroupResult.Value}", getAvailableLabClassesResult.Value, CacheExpirationTime, cancellationToken);
        return new ExecutionResult(Result.Ok(message.ToString() + "\n (дб)"));
    }
}

[Export(typeof(ITelegramCommand))]
[ExportMetadata(nameof(Command), $"{TelegramCommandFactory.CommandPrefix}hop")]
[ExportMetadata(nameof(Description), "- записывает на лабораторную работу")]
public class EnqueueInClassTelegramCommand : ITelegramCommand
{
    public string Command => $"{TelegramCommandFactory.CommandPrefix}hop";
    public string Description => "- записывает на лабораторную работу";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        IDatabaseCommunicationClient databaseCommunicator = factory.DatabaseCommunicator;
        Result<UserInfo> result = await databaseCommunicator.GetUserInfo(chatId, cancellationToken);
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }

        Result<IEnumerable<ClassInformation>> availableLabClassesResult = await databaseCommunicator.GetAvailableLabClasses(chatId, cancellationToken);
        if (availableLabClassesResult.IsFailed)
        {
            return new ExecutionResult(Result.Fail(availableLabClassesResult.Errors.First()));
        }
        
        
        IReplyMarkup replyMarkup = await CreateInlineKeyboardMarkupAsync(availableLabClassesResult.Value);
        return new ExecutionResult(Result.Fail("Выберите пару"), replyMarkup);
    }
    
    private Task<IReplyMarkup> CreateInlineKeyboardMarkupAsync(IEnumerable<ClassInformation> classes)
    {
        List<InlineKeyboardButton[]> buttons = [];
        foreach (ClassInformation classInformation in classes)
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(classInformation.ClassDateUnixTimestamp).DateTime;
            InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData($"{classInformation.ClassName} {dateTime:dd.MM}", $"{TelegramCommandQueryFactory.CommandQueryPrefix}hop {classInformation.ClassId}");
            buttons.Add([button]);
        }

        return Task.FromResult<IReplyMarkup>(new InlineKeyboardMarkup(buttons));
    }
}