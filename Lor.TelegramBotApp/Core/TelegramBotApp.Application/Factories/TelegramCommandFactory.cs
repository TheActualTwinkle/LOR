using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories.Common;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Application.Factories;

public partial class TelegramCommandFactory(IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService)
{
    #region ImportsInfo

    private class ImportInfo
    {
        [ImportMany]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<Lazy<ITelegramCommand, TelegramCommandMetadata>> Commands { get; set; } = [];
    }

    #endregion
    
    public const string CommandPrefix = "/";

    public IDatabaseCommunicationClient DatabaseCommunicator => databaseCommunicator;
    public IAuthorizationService AuthorizationService => authorizationService;
    
    private static readonly ImportInfo Info = new();
    
    static TelegramCommandFactory()
    {
        Assembly[] assemblies = [typeof(ITelegramCommand).Assembly];
        ContainerConfiguration configuration = new();
        try
        {
            configuration = configuration.WithAssemblies(assemblies);
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to load TelegramCommandFactory");
            throw;
        }
        
        using var container = configuration.CreateContainer();
        container.SatisfyImports(Info);   
    }
    
    public async Task<ExecutionResult> StartCommand(string commandString, long chatId, CancellationToken token)
    {
        var match = TelegramCommandRegex().Match(commandString);
        if (match.Success == false)
        {
            return new ExecutionResult(Result.Fail($"Команда не найдена\nДля получения списка команд введите {CommandPrefix}help"));
        }
        
        var command = GetCommand(match.Value.Trim());
        
        if (command == null)
        {
            return new ExecutionResult(Result.Fail($"Команда не найдена\nДля получения списка команд введите {CommandPrefix}help"));
        }

        return await command.Execute(chatId, this, GetArguments(commandString), token);
    }
    
    public static IEnumerable<string> GetAllCommandsInfo()
    {
        return Info.Commands.Select(x => $"{x.Metadata.Command} {x.Metadata.Description}");
    }

    public static ReplyKeyboardMarkup GetCommandButtonsReplyMarkup()
    {
        var commandsWithButtonDescription = Info.Commands.Where(x => x.Metadata.ButtonDescriptionText != null);
        
        var buttons = commandsWithButtonDescription.Select(x => new[] { new KeyboardButton($"{x.Metadata.ButtonDescriptionText}\n\n({x.Metadata.Command})") });
        
        // Take 2 buttons per row
        var buttonsPerRow = buttons.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 2).Select(x => x.Select(v => v.Value).SelectMany(v => v));
        
        ReplyKeyboardMarkup replyKeyboardMarkup = new(buttonsPerRow);

        return replyKeyboardMarkup;
    }

    private static ITelegramCommand? GetCommand(string command)
    {
        return Info.Commands.FirstOrDefault(x => x.Metadata.Command == command)?.Value;
    }

    private string[] GetArguments(string commandString)
    {
        return commandString.Split(' ').Skip(1).ToArray();
    }

    [GeneratedRegex(@$"\{CommandPrefix}\w+")]
    public static partial Regex TelegramCommandRegex();
}