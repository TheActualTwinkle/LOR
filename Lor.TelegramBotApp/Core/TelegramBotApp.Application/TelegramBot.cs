using FluentResults;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotApp.AppCommunication.Interfaces;
using TelegramBotApp.Application.Factories;
using TelegramBotApp.Application.Interfaces;
using TelegramBotApp.Application.Settings;

namespace TelegramBotApp.Application;

public class TelegramBot(ITelegramBotClient telegramBot, ReceiverOptions receiverOptions) : ITelegramBot
{
    private const string HelpCommand = "/help";
    
    private readonly ITelegramBotSettings _settings = TelegramBotSettings.CreateDefault();
    private TelegramCommandFactory _telegramCommandFactory = null!;
    private IGroupScheduleCommunicator _scheduleCommunicator = null!;

    public void StartReceiving(IGroupScheduleCommunicator scheduleCommunicator, CancellationToken cancellationToken)
    {
        _telegramCommandFactory = new TelegramCommandFactory(_settings, scheduleCommunicator);
        _scheduleCommunicator = scheduleCommunicator;
        
        telegramBot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleError), receiverOptions, cancellationToken);
    }

    public Task SendMessageAsync(long telegramId, string message) => telegramBot.SendTextMessageAsync(telegramId, message);

    public Task<User> GetMeAsync() => telegramBot.GetMeAsync();
    
    private Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return Task.CompletedTask;
        if (message.Text is not { } userMessageText) return Task.CompletedTask;
        
        long chatId = message.Chat.Id; // chat id equals to user telegram id
        
        Task.Run(async () =>
        {
            using var cts = new CancellationTokenSource(_settings.Timeout);
            
            try
            {
                Result<string> result = await _telegramCommandFactory.StartCommand(userMessageText, chatId);

                if (result.IsFailed)
                {
                    await SendErrorMessage(bot, chatId, new Exception("Неизвестная команда"), cts.Token);
                    return;
                }

                await bot.SendTextMessageAsync(chatId: chatId, result.Value, cancellationToken: cts.Token);
            }
            catch (TaskCanceledException)
            {
                await SendErrorMessage(bot, chatId, new Exception("Время запроса истекло"), cts.Token);
            }
            catch (Exception)
            {
                await SendErrorMessage(bot, chatId, new Exception("Внутрення ошибка"), cts.Token);
            }
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task SendErrorMessage(ITelegramBotClient bot, long chatIdInner, Exception exception, CancellationToken cancellationToken)
    {
        await bot.SendTextMessageAsync(chatId: chatIdInner, exception.Message, cancellationToken: cancellationToken);
        
        Result<string> text = await _telegramCommandFactory.StartCommand(HelpCommand, chatIdInner);
        await bot.SendTextMessageAsync(chatId: chatIdInner, text.Value, cancellationToken: cancellationToken);
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
}