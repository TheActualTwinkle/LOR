using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Commands;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Application.Settings;

namespace TelegramBotApp.Application;

public class TelegramBot(ITelegramBotClient telegramBot, ReceiverOptions receiverOptions) : ITelegramBot
{
    private readonly ITelegramBotSettings _settings = TelegramBotSettings.CreateDefault();
    private TelegramCommandFactory _telegramCommandFactory = null!;

    public void StartReceiving(IDatabaseCommunicationClient databaseCommunicator, CancellationToken cancellationToken)
    {
        _telegramCommandFactory = new TelegramCommandFactory(_settings, databaseCommunicator);

        telegramBot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleError), receiverOptions, cancellationToken);
    }
    
    public Task<User> GetMeAsync() => telegramBot.GetMeAsync();
    
    private Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not null)
        {
            Task.Run(async () =>
            {
                await HandleCallbackQuery(bot, update.CallbackQuery);
                return Task.CompletedTask;
            }, cancellationToken);
        }
        
        if (update.Message is not { } message) return Task.CompletedTask;
        if (message.Text is not { } userMessageText) return Task.CompletedTask;
        
        long chatId = message.Chat.Id; // chat id equals to user telegram id
        
        Task.Run(async () =>
        {
            using CancellationTokenSource cts = new(_settings.Timeout);
            
            try
            {
                ExecutionResult executionResult = await _telegramCommandFactory.StartCommand(userMessageText, chatId);

                if (executionResult.Result.IsFailed)
                {
                    await SendErrorMessage(bot, chatId, new Exception(executionResult.Result.Errors.FirstOrDefault()?.Message ?? "Неизвестная ошибка"), executionResult.ReplyMarkup, cts.Token);
                    return;
                }

                await bot.SendTextMessageAsync(chatId: chatId, executionResult.Result.Value, replyMarkup: executionResult.ReplyMarkup, cancellationToken: cts.Token);
            }
            catch (TaskCanceledException)
            {
                await SendErrorMessage(bot, chatId, new Exception("Время запроса истекло"), new ReplyKeyboardRemove(), cts.Token);
            }
            catch (Exception)
            {
                await SendErrorMessage(bot, chatId, new Exception("Внутрення ошибка"), new ReplyKeyboardRemove(), cts.Token);
            }
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task SendErrorMessage(ITelegramBotClient bot, long chatIdInner, Exception exception, IReplyMarkup replyMarkup, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(chatId: chatIdInner, exception.Message, replyMarkup: replyMarkup, cancellationToken: cancellationToken);
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
    
    private async Task HandleCallbackQuery(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data?.First() != '/') return;
        
        if (callbackQuery.Message is null) return;
        
        await HandleUpdateAsync(bot, new Update
        {
            Message = new Message
            {
                Text = callbackQuery.Data,
                Chat = callbackQuery.Message.Chat
            },
        }, CancellationToken.None);
        
        await bot.AnswerCallbackQueryAsync(callbackQuery.Id);
    }
}