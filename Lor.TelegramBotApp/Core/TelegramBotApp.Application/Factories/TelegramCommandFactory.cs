﻿using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentResults;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories.Common;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Identity.Services.Interfaces;

namespace TelegramBotApp.Application.Factories;

public class TelegramCommandFactory(
    IDatabaseCommunicationClient databaseCommunicator,
    IRegistrationService registrationService,
    IAuthService authService)
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
    public IRegistrationService RegistrationService => registrationService;
    public IAuthService AuthService => authService;
    
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
        
        using CompositionHost container = configuration.CreateContainer();
        container.SatisfyImports(Info);   
    }
    
    public async Task<ExecutionResult> StartCommand(string commandString, long chatId, CancellationToken token)
    {
        Match match = TelegramCommand().Match(commandString);
        if (match.Success == false)
        {
            return new ExecutionResult(Result.Fail($"Команда не найдена\nДля получения списка команд введите {CommandPrefix}help"));
        }
        
        ITelegramCommand? command = GetCommand(match.Value.Trim());
        
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
        IEnumerable<Lazy<ITelegramCommand, TelegramCommandMetadata>> commandsWithButtonDescription = Info.Commands.Where(x => x.Metadata.ButtonDescriptionText != null);
        
        IEnumerable<KeyboardButton[]> buttons = commandsWithButtonDescription.Select(x => new[] { new KeyboardButton($"{x.Metadata.ButtonDescriptionText}\n\n({x.Metadata.Command})") });
        
        // Take 2 buttons per row
        IEnumerable<IEnumerable<KeyboardButton>> buttonsPerRow = buttons.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 2).Select(x => x.Select(v => v.Value).SelectMany(v => v));
        
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
    public static partial Regex TelegramCommand();
}