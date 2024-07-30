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

    public IDatabaseCommunicationClient DatabaseCommunicator => databaseCommunicator;
    
    private static readonly ImportInfo s_info = new();
    
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
        
        using CompositionHost container = configuration.CreateContainer();
        container.SatisfyImports(s_info);   
    }
    
    public async Task<ExecutionResult> Handle(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
    {
        if (callbackQuery.Data?.First() != '!') throw new ArgumentException("CallbackQuery: Неверный формат запроса");
        
        if (callbackQuery.Message is null) return new ExecutionResult(Result.Fail("CallbackQuery: Не найдено сообщение"));
        
        long chatId = callbackQuery.Message.Chat.Id;
        string queryString = callbackQuery.Data;
            
        ICallbackQuery? query = GetQuery(queryString.Split(' ').FirstOrDefault()!);
        
        if (query == null)
        {
            throw new ArgumentException($"CallbackQuery: {queryString} - не найден");
        }

        return await query.Execute(chatId, this, GetArguments(queryString), cancellationToken);
    }
    
    private ICallbackQuery? GetQuery(string queryString)
    {
        return s_info.Queries.FirstOrDefault(x => x.Metadata.Query == queryString)?.Value;
    }
    
    private string[] GetArguments(string commandString)
    {
        return commandString.Split(' ').Skip(1).ToArray();
    }
}