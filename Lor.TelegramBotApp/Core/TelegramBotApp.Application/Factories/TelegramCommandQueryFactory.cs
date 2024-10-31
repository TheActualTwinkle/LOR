using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using FluentResults;
using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories.Common;
using TelegramBotApp.Application.Interfaces;

namespace TelegramBotApp.Application.Factories;

public class TelegramCommandQueryFactory(IDatabaseCommunicationClient databaseCommunicator)
{
    #region ImportsInfo

    private class ImportInfo
    {
        [ImportMany]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<Lazy<ICallbackQuery, TelegramCallbackQueryMetadata>> Queries { get; set; } = [];
    }

    #endregion
    
    public const string CommandQueryPrefix = "!"; 
    
    public IDatabaseCommunicationClient DatabaseCommunicator => databaseCommunicator;
    
    private static readonly ImportInfo Info = new();
    
    static TelegramCommandQueryFactory()
    {
        Assembly[] assemblies = [typeof(ICallbackQuery).Assembly];
        ContainerConfiguration configuration = new();
        try
        {
            configuration = configuration.WithAssemblies(assemblies);
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to load TelegramCommandQueryFactory");
            throw;
        }
        
        using var container = configuration.CreateContainer();
        container.SatisfyImports(Info);   
    }
    
    public async Task<ExecutionResult> Handle(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
    {
        if (callbackQuery.Data is null) return new ExecutionResult(Result.Fail("CallbackQuery: Не найдены данные"));
        
        if (callbackQuery.Data.StartsWith(CommandQueryPrefix) == false) return new ExecutionResult(Result.Fail("CallbackQuery: Неверный формат запроса"));
        
        if (callbackQuery.Message is null) return new ExecutionResult(Result.Fail("CallbackQuery: Не найдено сообщение"));
        
        var chatId = callbackQuery.Message.Chat.Id;
        var queryString = callbackQuery.Data;
            
        var query = GetQuery(queryString.Split(' ').FirstOrDefault()!);
        
        if (query == null)
            return new ExecutionResult(Result.Fail("CallbackQuery: {queryString} - не найден"));

        return await query.Execute(chatId, this, GetArguments(queryString), cancellationToken);
    }
    
    private ICallbackQuery? GetQuery(string queryString)
    {
        return Info.Queries.FirstOrDefault(x => x.Metadata.Query == queryString)?.Value;
    }
    
    private string[] GetArguments(string commandString)
    {
        return commandString.Split(' ').Skip(1).ToArray();
    }
}