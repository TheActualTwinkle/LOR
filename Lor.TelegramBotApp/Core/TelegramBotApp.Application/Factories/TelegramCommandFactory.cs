using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories.Common;
using TelegramBotApp.Application.Interfaces;

namespace TelegramBotApp.Application.Factories;

public class TelegramCommandFactory(ITelegramBotSettings settings, IDatabaseCommunicationClient databaseCommunicator)
{
    #region ImportsInfo

    private class ImportInfo
    {
        [ImportMany]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<Lazy<ITelegramCommand, TelegramCommandMetadata>> Commands { get; set; } = [];
    }

    #endregion

    public IDatabaseCommunicationClient DatabaseCommunicator => databaseCommunicator;
    
    private static readonly ImportInfo s_info = new();
    
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
            Console.WriteLine("Failed to load AddressablesLoaderFactory");
            throw;
        }
        
        using CompositionHost container = configuration.CreateContainer();
        container.SatisfyImports(s_info);   
    }
    
    public async Task<ExecutionResult> StartCommand(string commandString, long chatId)
    {
        ITelegramCommand? command = GetCommand(commandString.Split(' ').FirstOrDefault()!);
        
        if (command == null)
        {
            return new ExecutionResult(Result.Fail("Команда не найдена\nДля получения списка команд введите /help"));
        }

        return await command.Execute(chatId, this, GetArguments(commandString), settings.Token);
    }
    
    public static IEnumerable<string> GetAllCommandsInfo()
    {
        return s_info.Commands.Select(x => $"{x.Metadata.Command} {x.Metadata.Description}");
    }

    private ITelegramCommand? GetCommand(string command)
    {
        return s_info.Commands.FirstOrDefault(x => x.Metadata.Command == command)?.Value;
    }
    
    private string[] GetArguments(string commandString)
    {
        return commandString.Split(' ').Skip(1).ToArray();
    }
}