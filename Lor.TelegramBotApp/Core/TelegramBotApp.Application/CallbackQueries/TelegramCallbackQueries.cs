using System.Composition;
using System.Text;
using FluentResults;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Interfaces;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.CallbackQueries;

[Export(typeof(ICallbackQuery))]
[ExportMetadata(nameof(Query), "!hop")]
public class EnqueueCallbackQuery : ICallbackQuery
{
    public string Query => "!hop";
    
    public async Task<ExecutionResult> Execute(long chatId, IDatabaseCommunicationClient databaseCommunicator, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {        
        List<string> argumentsList = arguments.ToList();
        if (argumentsList.Count != 1)
        {
            throw new ArgumentException("EnqueueCallbackQuery: Неверное количество аргументов");
        }
        
        if (int.TryParse(argumentsList.First(), out int classId) == false)
        {
            throw new ArgumentException($"EnqueueCallbackQuery: Неверный формат аргумента (должен быть {classId.GetType})");
        }
        
        Result<IEnumerable<string>> result = await databaseCommunicator.EnqueueInClass(classId, chatId);
        
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }
        
        StringBuilder message = new("Вы успешно записаны!\nОчередь:\n");
        foreach (string labClass in result.Value)
        {
            message.AppendLine(labClass);
        }
        
        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}