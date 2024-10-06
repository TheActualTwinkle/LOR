using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Settings;
using TelegramBotApp.Authorization.Interfaces;
using TelegramBotApp.Domain.Interfaces;

namespace TelegramBotApp.Application;

public class TelegramBot(ITelegramBotClient telegramBot, ReceiverOptions receiverOptions, IDatabaseCommunicationClient databaseCommunicator, IAuthorizationService authorizationService) : ITelegramBot
{
    private readonly ITelegramBotSettings _settings = TelegramBotSettings.CreateDefault();
    private TelegramCommandFactory _telegramCommandFactory = null!;
    private TelegramCommandQueryFactory _telegramCommandQueryFactory = null!;

    public void StartReceiving(CancellationToken cancellationToken)
    {
        _telegramCommandFactory = new TelegramCommandFactory(databaseCommunicator, authorizationService);
        _telegramCommandQueryFactory = new TelegramCommandQueryFactory(databaseCommunicator);

        telegramBot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleError), receiverOptions, cancellationToken);
    }

    public async Task SendMessageAsync(long telegramId, string message, IReplyMarkup? replyMarkup, CancellationToken cancellationToken = default)
    {
        replyMarkup ??= TelegramCommandFactory.GetCommandButtonsReplyMarkup(); 
            
        await telegramBot.SendTextMessageAsync(telegramId, message, replyMarkup: replyMarkup, cancellationToken: cancellationToken); 
    }
    
    public Task<User> GetMeAsync() => telegramBot.GetMeAsync();
    
    private Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not null)
        {
            Task.Run(async () =>
            {
                using CancellationTokenSource cts = new(_settings.Timeout);
                await HandleCallbackQuery(bot, update.CallbackQuery, cts.Token);
            }, cancellationToken);
        }
        
        if (update.Message is not { } message) return Task.CompletedTask;
        if (message.Text is not { } userMessageText) return Task.CompletedTask;
        
        long chatId = message.Chat.Id; // chat id equals to user telegram id
        
        Task.Run(async () =>
        {
            using CancellationTokenSource cts = new(_settings.Timeout);
            
            if (message.ReplyToMessage is not null && message.ReplyToMessage.From?.Id == bot.BotId)
            {
                await HandleMessageReply(message, chatId, cts.Token);
                return;
            }
            
            try
            {
                ExecutionResult executionResult = await _telegramCommandFactory.StartCommand(userMessageText, chatId, cts.Token);

                if (executionResult.Result.IsFailed)
                {
                    await SendErrorMessage(chatId, new Exception(executionResult.Result.Errors.First()?.Message ?? "Неизвестная ошибка"), executionResult.ReplyMarkup, cts.Token);
                    return;
                }
                
                await SendMessageAsync(chatId, executionResult.Result.Value, executionResult.ReplyMarkup, cts.Token);
            }
            catch (TaskCanceledException)
            {
                await SendErrorMessage(chatId, new Exception("Время запроса истекло"), new ReplyKeyboardRemove(), cts.Token);
            }
            catch (Exception e)
            {
                await SendErrorMessage(chatId, new Exception($"Внутрення ошибка. {e}"), new ReplyKeyboardRemove(), cts.Token);
            }
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task SendErrorMessage(long chatIdInner, Exception exception, IReplyMarkup? replyMarkup, CancellationToken cancellationToken)
    {
        await SendMessageAsync(chatIdInner, exception.Message, replyMarkup, cancellationToken);
    }
    
    private Task HandleError(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);

        return Task.CompletedTask;
    }

    private async Task HandleCallbackQuery(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        try
        {
            ExecutionResult result = await _telegramCommandQueryFactory.Handle(callbackQuery, cancellationToken);

            long chatId = callbackQuery.Message!.Chat.Id;
            
            if (result.Result.IsFailed)
            {
                await SendErrorMessage(chatId, new Exception(result.Result.Errors.FirstOrDefault()?.Message ?? "Неизвестная ошибка"), result.ReplyMarkup, cancellationToken);
            }
            else
            {
                await SendMessageAsync(chatId, result.Result.Value, result.ReplyMarkup, cancellationToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
        
        await bot.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
    }
    
    private async Task HandleMessageReply(Message message, long chatId, CancellationToken cancellationToken)
    {
        if (message.ReplyToMessage is not {} reply) return;
        if (reply.Text is not {} replyText) return;
        if (message.Text is not {} text) return;

        Match commandMatch = TelegramCommandFactory.TelegramCommandRegex().Match(replyText);
        if (commandMatch.Success == false)
        {
            Console.WriteLine("Can`t handle message reply: ReplyToMessage has no command");
            return;
        }
        
        string commandString = commandMatch.Value.Trim();
        
        try
        {
            var wholeCommandString = $"{commandString} {text}";
            ExecutionResult result = await _telegramCommandFactory.StartCommand(wholeCommandString, chatId, cancellationToken);
            
            if (result.Result.IsFailed)
            {
                await SendErrorMessage(chatId, new Exception(result.Result.Errors.FirstOrDefault()?.Message ?? "Неизвестная ошибка"), result.ReplyMarkup, cancellationToken);
            }
            else
            {
                await SendMessageAsync(chatId, result.Result.Value, result.ReplyMarkup, cancellationToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"HandleMessageReply Error: {e.Message}");
        }
    }
}