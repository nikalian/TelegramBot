using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using MilanaBot;
using System.Xml;

class AddData(string tablename) : Program
{
    private static Admins admins = new Admins();
    public static DataBase dataBase = new DataBase();
    public static TelegramBotClient BotClient = new TelegramBotClient(Spravka.BotToken);
    public static System.Threading.CancellationTokenSource CTSgetMessage;
    public static int CountElement;
    public static int SchetchikSMS = 0;
    public string TableName = tablename;
    public static string TB;
    public static string[] MassElement = new string[CountElement];
    public async Task HandleAdminCommand(ITelegramBotClient bot, Message message)
    {
        using var cts = new System.Threading.CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } // получать все обновления
        };

        CTSgetMessage = cts;
        TB = TableName;
        //Проверяем какая таблица и в соответсвтии с этим формируем массив для элементов который потом запишем в БД
        switch (TableName)
        {
            case Spravka.UslugiTableName:
                CountElement = 3;
                break;
            case Spravka.ZapisiTableName:
                CountElement = 5;
                break;
            case Spravka.MasteraTableName:
                CountElement = 3;
                break;
            case Spravka.KlientiTableName:
                CountElement = 5;
                break;
            case Spravka.GraphikMasterovTableName:
                CountElement = 5;
                break;
            default:
                break;
        }

        // Обработчик входящих сообщений
        BotClient.StartReceiving(
        Meesage_HandleUpdateAsync,
        Message_HandleErrorAsync,
        receiverOptions,
        cts.Token
    );

        Console.ReadLine();
    }
    public static void SetInDataBase(string table_name)
    {
        switch (table_name)
        {
            case Spravka.UslugiTableName:
                dataBase.AddKServicesForMass(MassElement);
                break;
            case Spravka.ZapisiTableName:
                break;
            case Spravka.MasteraTableName:
                break;
            case Spravka.KlientiTableName:
                break;
            case Spravka.GraphikMasterovTableName:
                break;
            default:
                break;
        }
    }
    static async Task Meesage_HandleUpdateAsync(ITelegramBotClient bot, Update update, System.Threading.CancellationToken cancellationToken)
    {
        if (update != null && SchetchikSMS <= CountElement)
        {
            MassElement[SchetchikSMS] = update.Message.Text;
            SchetchikSMS++;
        }
        else
        {
            SetInDataBase(TB);
            await admins.HandleAdminCommand(bot, update.Message);
            CTSgetMessage.Cancel();
        }
    }
    static Task Message_HandleErrorAsync(ITelegramBotClient bot, Exception exception, System.Threading.CancellationToken cancellationToken)
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