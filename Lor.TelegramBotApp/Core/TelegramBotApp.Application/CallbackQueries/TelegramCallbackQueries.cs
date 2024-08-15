using System.Composition;
using System.Text;
using FluentResults;
using TelegramBotApp.AppCommunication;
using TelegramBotApp.AppCommunication.Data;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.CallbackQueries;

[Export(typeof(ICallbackQuery))]
[ExportMetadata(nameof(Query), "!hop")]
public class EnqueueCallbackQuery : ICallbackQuery
{
    public string Query => "!hop";
    
    public async Task<ExecutionResult> Execute(long chatId, TelegramCommandQueryFactory factory, IEnumerable<string> arguments, CancellationToken cancellationToken)
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
        
        Result<EnqueueInClassResult> result = await factory.DatabaseCommunicator.EnqueueInClass(classId, chatId, cancellationToken);
        
        if (result.IsFailed)
        {
            return new ExecutionResult(Result.Fail(result.Errors.First()));
        }
        
        StringBuilder message = new($"Вы успешно записаны на {result.Value.ClassName} {result.Value.ClassesDateTime:dd.MM}\nОчередь:\n");
        for (var i = 0; i < result.Value.StudentsQueue.Count(); i++)
        {
            string labClass = result.Value.StudentsQueue.ElementAt(i);
            message.AppendLine($"{i+1}. {labClass}");
        }
        
        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}