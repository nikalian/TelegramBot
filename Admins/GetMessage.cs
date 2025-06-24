using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;

class GetMessage()
{
    public static string Message;
    public static TelegramBotClient BotClient;
    public async Task HandleAdminCommand(ITelegramBotClient bot, Message message)
    {
        BotClient = new TelegramBotClient(Spravka.BotToken);
        using var cts = new System.Threading.CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } // получать все обновления
        };

        // Обработчик входящих сообщений
        BotClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cts.Token
        );

        Console.ReadLine();
    }
    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, System.Threading.CancellationToken cancellationToken)
    {
        if (update != null)
        {
            Message = update.Message.Text;
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, System.Threading.CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Ошибка API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}