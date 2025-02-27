using System.Composition;
using System.Text;
using FluentResults;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;

// ReSharper disable UnusedType.Global

namespace TelegramBotApp.Application.CallbackQueries;

[Export(typeof(ICallbackQuery))]
[ExportMetadata(nameof(Query), $"{TelegramCommandQueryFactory.CommandQueryPrefix}hop")]
public class EnqueueCallbackQuery : ICallbackQuery
{
    public string Query =>
        $"{TelegramCommandQueryFactory.CommandQueryPrefix}hop";

    public async Task<ExecutionResult> Execute(
        long chatId,
        TelegramCommandQueryFactory factory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        var argumentsList = arguments.ToList();

        if (argumentsList.Count != 1)
            throw new ArgumentException(
                "EnqueueCallbackQuery: Неверное количество аргументов",
                nameof(arguments));

        if (!int.TryParse(argumentsList.First(), out var classId))
            throw new FormatException(
                $"EnqueueCallbackQuery: Неверный формат аргумента." +
                $"Невозможно привести {argumentsList.First()} к {classId.GetType()} {nameof(classId)}");

        var result = await factory.DatabaseCommunicator.EnqueueInClass(classId, chatId, cancellationToken);

        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        var classData = $"{result.Value.Name} {result.Value.Date:dd.MM}";

        var messageHeader = result.Value.WasAlreadyEnqueued ?
            $"Вы уже были записаны на {classData}\n" :
            $"Вы успешно записаны на {classData}\nОчередь:\n";

        StringBuilder message = new(messageHeader);

        for (var i = 0; i < result.Value.StudentsQueue.Count(); i++)
        {
            var labClass = result.Value.StudentsQueue.ElementAt(i);
            message.AppendLine($"{i + 1}. {labClass}");
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ICallbackQuery))]
[ExportMetadata(nameof(Query), $"{TelegramCommandQueryFactory.CommandQueryPrefix}dehop")]
public class DequeueCallbackQuery : ICallbackQuery
{
    public string Query =>
        $"{TelegramCommandQueryFactory.CommandQueryPrefix}dehop";

    public async Task<ExecutionResult> Execute(
        long chatId,
        TelegramCommandQueryFactory factory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        var argumentsList = arguments.ToList();

        if (argumentsList.Count != 1)
            throw new ArgumentException("DequeueCallbackQuery: Неверное количество аргументов");

        if (!int.TryParse(argumentsList.First(), out var classId))
            throw new FormatException(
                $"DequeueCallbackQuery: Неверный формат аргумента." +
                $"Невозможно привести {argumentsList.First()} к {classId.GetType()} {nameof(classId)}");

        var result = await factory.DatabaseCommunicator.DequeueFromClass(classId, chatId, cancellationToken);

        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        var classData = $"{result.Value.Name} {result.Value.Date:dd.MM}";

        var messageHeader = result.Value.WasAlreadyDequeuedFromClass ?
            $"Вы не были записаны в очередь {classData}\n" :
            $"Вы успешно выписаны из очереди {classData}\n";

        if (!result.Value.StudentsQueue.Any())
            return new ExecutionResult(Result.Ok(messageHeader));

        messageHeader += "Очередь:\n";

        StringBuilder message = new(messageHeader);

        for (var i = 0; i < result.Value.StudentsQueue.Count(); i++)
        {
            var labClass = result.Value.StudentsQueue.ElementAt(i);
            message.AppendLine($"{i + 1}. {labClass}");
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}

[Export(typeof(ICallbackQuery))]
[ExportMetadata(nameof(Query), $"{TelegramCommandQueryFactory.CommandQueryPrefix}queue")]
public class ViewClassQueueQuery : ICallbackQuery
{
    public string Query =>
        $"{TelegramCommandQueryFactory.CommandQueryPrefix}queue";

    public async Task<ExecutionResult> Execute(
        long chatId,
        TelegramCommandQueryFactory factory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        var argumentsList = arguments.ToList();

        if (argumentsList.Count != 1)
            throw new ArgumentException("ViewQueueAtClassQuery: Неверное количество аргументов");

        if (!int.TryParse(argumentsList.First(), out var classId))
            throw new FormatException(
                $"ViewClassQueueQuery: Неверный формат аргумента." +
                $"Невозможно привести {argumentsList.First()} к {classId.GetType()} {nameof(classId)}");

        var result = await factory.DatabaseCommunicator.ViewClassQueue(classId, cancellationToken);

        if (result.IsFailed)
            return new ExecutionResult(Result.Fail(result.Errors.First()));

        var classData = $"{result.Value.Name} {result.Value.Date:dd.MM}";

        var messageHeader = $"Очередь на {classData}:\n";

        if (!result.Value.StudentsQueue.Any())
        {
            messageHeader += "Очередь пуста :^(";

            return new ExecutionResult(Result.Ok(messageHeader));
        }

        StringBuilder message = new(messageHeader);

        for (var i = 0; i < result.Value.StudentsQueue.Count(); i++)
        {
            var labClass = result.Value.StudentsQueue.ElementAt(i);
            message.AppendLine($"{i + 1}. {labClass}");
        }

        return new ExecutionResult(Result.Ok(message.ToString()));
    }
}