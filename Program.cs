using System.Dynamic;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using Microsoft.VisualBasic;

namespace MilanaBot
{
    internal class Program
    {
        private static Admins admins = new Admins();
        private static DataBase dataBase = new DataBase();
        public static readonly string token = Spravka.BotToken;
        public static Telegram.Bot.Types.Message chatFullInfo;
        public static DateTime todayDateOnly = DateTime.Today;
        public static CallbackQuery Data;
        private static long AdminChatID;
        public static System.Threading.CancellationTokenSource CTS;

        public static string Usluga = "";
        public static string MasterName = "";
        public static string Stoimost = "";
        public static int Day;
        public static int Month;
        public static int Hour;
        public static int Minute;

        public static async Task Main(string[] args)
        {
            await HandleAdminCommand();

            Console.ReadLine();
        }
        public static async Task HandleAdminCommand()
        {
            var botClient = new TelegramBotClient(token);

            var me = await botClient.GetMe();

            using var cts = new System.Threading.CancellationTokenSource();

            CTS = cts;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // получать все обновления
            };

            Console.WriteLine($"Запущен бот: {me.Username} для пользователей");

            // Обработчик входящих сообщений
            botClient.StartReceiving(
                handleUpdateAsync,
                handleErrorAsync,
                receiverOptions,
                cts.Token
            );
            
        }

        static async Task handleUpdateAsync(ITelegramBotClient bot, Update update, System.Threading.CancellationToken cancellationToken)
        {
            if (update != null)
            {

                if (update.Message != null && update.Message.Text == "/start")
                {
                    if (update.Message.Text != null && update.Message.Chat.Username == "ediknikalian")
                    {
                        AdminChatID = update.Message.Chat.Id;
                        await admins.HandleAdminCommand(bot, update.Message);
                        CTS.Cancel();
                    }
                    else
                    {
                        // Отправляем стартовое сообщение с кнопками
                            var replyKeyboard = new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Записаться"), new KeyboardButton("Мои записи") } })
                        {
                            ResizeKeyboard = true
                        };
                        await bot.SendMessage(update.Message.Chat.Id, "Выберите действие:", replyMarkup: replyKeyboard);
                    }
                }
                else if (update.Message != null && update.Type == UpdateType.Message)
                {
                    var msg = update.Message;
                    var chatId = msg.Chat.Id;
                    if (msg.Text == "Записаться")
                    {
                    var botClient = new TelegramBotClient(token);

                        // Получение последних сообщений (например, 100 сообщений)
                        //var messages = await botClient.DeleteMessages(chatId);

                        /*foreach (var message in messages)
                        {
                            try
                            {
                                await botClient.DeleteMessageAsync(chatId, message.MessageId);
                            }
                            catch
                            {

                            }
                        }*/
                        string text = "Выберите мастера к которому вы хотите записаться:";
                        var Masters = dataBase.GetMasters();
                        var button = new List<InlineKeyboardButton[]>();
                        foreach (var Row in Masters.Rows)
                        {
                            string name = ((System.Data.DataRow)Row).ItemArray[1].ToString();
                            button.Add(new[] { InlineKeyboardButton.WithCallbackData(name, name + "_мастер") });
                        }
                        button.ToArray();

                        var markup = new InlineKeyboardMarkup(button);

                        var mes = await bot.SendMessage(chatId, "Обновление данных...", replyMarkup: new ReplyKeyboardRemove());
                        bot.DeleteMessage(chatId, mes.Id);
                        // Новое начало диалога сообщение
                        var message = await bot.SendMessage(
                            update.Message.Chat.Id,
                            text: text,
                            replyMarkup: markup
                        );
                        dataBase.AddKlients(update.Message.Chat.FirstName, update.Message.Chat.Username, Convert.ToString(chatId), Convert.ToString(message.Id));
                    }
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    var callback = update.CallbackQuery;
                    if (callback.Message is not null)
                    {
                        var chatId = callback.Message.Chat.Id;
                        Data = update.CallbackQuery;
                        await HandleCallbackAsync(bot, callback, chatId);
                    }
                }
            }
        }
        static async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, long chatId)
        {

            // В зависимости от данных кнопки — меняем содержимое сообщения
            if (callback.Data == null || callback.Data == "none")
            {
                return;
            }
            else if (callback.Data.ToLower().Contains("_мастер"))
            {
                //Записываем мастера к клиенту в базу
                dataBase.SetMastersForKlient(callback.Data.Substring(0, callback.Data.LastIndexOf("_")), Convert.ToString(chatId));
                //переключаем страницу
                await ShowPage2(bot, chatId);
            }
            else if (callback.Data == "_услугаНазад")
            {
                await ShowPage1(bot, chatId);
            }
            else if (callback.Data.ToLower().Contains("_услуга"))
            {
                //Записываем услугу к клиенту в базу
                dataBase.SetServicesForKlient(callback.Data.Substring(0, callback.Data.LastIndexOf("_")), Convert.ToString(chatId));
                await Kalendar.ShowCalendar(bot, chatId, todayDateOnly);
            }
            else if (callback.Data == "back_month")
            {
                await Kalendar.ShowCalendar(bot, chatId, todayDateOnly);
            }
            else if (callback.Data.ToLower().Contains("next_month"))
            {
                await Kalendar.ShowCalendarAddMonth(bot, chatId, todayDateOnly.AddMonths(1));
            }
            else if (callback.Data.ToLower().Contains("_day"))
            {
                Day = Convert.ToInt32(callback.Data.Substring(0, callback.Data.LastIndexOf("_")));
                Month = Kalendar.CurrentMonth;
                await ShowPage4(bot, chatId);
            }
            else if (callback.Data == "BackIsData")
            {
                await ShowPage2(bot, chatId);
            }
            else if (callback.Data == "page_4_next")
            {
                await ShowPage4(bot, chatId);
            }
            else if (callback.Data.ToLower().Contains("_timenext"))
            {
                Hour = Convert.ToInt32(callback.Data.Substring(0, 2));
                Minute = Convert.ToInt32(callback.Data.Substring(2, 2));
                await ShowPage5(bot, chatId);
            }
            else if (callback.Data == "page_3_next")
            {
                await Kalendar.ShowCalendar(bot, chatId, todayDateOnly);
            }
            else if (callback.Data == "All_Back" && callback.Message is not null)
            {
                await bot.DeleteMessage(chatId, callback.Message.Id);
                var replyKeyboard = new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Записаться"), new KeyboardButton("Мои записи") } })
                {
                    ResizeKeyboard = true
                };
                await bot.SendMessage(chatId, "Выберите действие:", replyMarkup: replyKeyboard);
            }
            else if (callback.Data == "Finish")
            {
                await SendFinish(bot, chatId, callback);
            }
        }

        // Первая страница (МАСТЕРА)
        static async Task ShowPage1(ITelegramBotClient bot, long chatId)
        {
            string text = "Выберите мастера к которому вы хотите записаться:";
            var Masters = dataBase.GetMasters();
            var button = new List<InlineKeyboardButton[]>();
            foreach (var Row in Masters.Rows)
            {
                string name = ((System.Data.DataRow)Row).ItemArray[1].ToString();
                button.Add(new[] { InlineKeyboardButton.WithCallbackData(name, name + "_мастер") });
            }
            button.ToArray();
            
            var markup = new InlineKeyboardMarkup(button);
            // Редактируем сообщение
            var message = await bot.EditMessageText(
                chatId,
                Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))),
                text: text,
                replyMarkup: markup
            );            
        }
        // Вторая страница (УСЛУГИ)
        static async Task ShowPage2(ITelegramBotClient bot, long chatId)
        {
            string text = "Выберите услугу:";
            var Services = dataBase.GetServices();
            var button = new List<InlineKeyboardButton[]>();
            foreach (var Row in Services.Rows)
            {
                string ServicesName = ((System.Data.DataRow)Row).ItemArray[1].ToString();
                string ServicesPrice = ((System.Data.DataRow)Row).ItemArray[2].ToString();
                string ServicesDescription = ((System.Data.DataRow)Row).ItemArray[3].ToString();
                button.Add(new[] { InlineKeyboardButton.WithCallbackData(ServicesName + ": " + ServicesPrice + " руб", ServicesName + "_услуга") });
            }
            button.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅ Назад", "_услугаНазад") });

            var markup = new InlineKeyboardMarkup(button);

            await bot.EditMessageText(
                chatId,
                Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))),
                text: text,
                replyMarkup: markup
            );
        }

        // Четвертая страница
        static async Task ShowPage4(ITelegramBotClient bot, long chatId)
        {
            string text = "Выберите время: ";
            var buttons = new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("14 : 00", "1400_timeNext"),
                        InlineKeyboardButton.WithCallbackData("15 : 00", "1500_timeNext"),
                        InlineKeyboardButton.WithCallbackData("16 : 00", "1600_timeNext")},
                new[] { InlineKeyboardButton.WithCallbackData("17 : 00", "1700_timeNext"),
                        InlineKeyboardButton.WithCallbackData("18 : 00", "1800_timeNext"),
                        InlineKeyboardButton.WithCallbackData("19 : 00", "1900_timeNext")},
                new[] { InlineKeyboardButton.WithCallbackData("20 : 00", "2000_timeNext"),
                        InlineKeyboardButton.WithCallbackData("21 : 00", "2100_timeNext"),
                        InlineKeyboardButton.WithCallbackData("22 : 00", "2200_timeNext")},
                new[] { InlineKeyboardButton.WithCallbackData("⬅ Назад", "page_3_next") },
            };
            var markup = new InlineKeyboardMarkup(buttons);

            // Редактируем сообщение
            await bot.EditMessageText(
                chatId,
                Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))), // здесь нужен messageId оригинального сообщения
                text: text,
                replyMarkup: markup
            );
        }

        // Пятая страница (Подтверждение)
        static async Task ShowPage5(ITelegramBotClient bot, long chatId)
        {
            string Name = null;
            string UserName = null;

            DateTime dateTime = new DateTime(todayDateOnly.Year, Month, Day, Hour, Minute, 0);
            var fullinfo = dataBase.GetKlientsFullInfo(Convert.ToString(chatId));

            if (fullinfo != null)
            foreach (var Row in ((System.Data.DataTable)fullinfo).Rows)
            {
                Name = ((System.Data.DataRow)Row).ItemArray[3].ToString();
                UserName = ((System.Data.DataRow)Row).ItemArray[4].ToString();
            }
            else
            {
                Name = "Неизвестно";
                UserName = "Неизвестно";
            }

            CultureInfo russianCulture = new CultureInfo("ru-RU");

            string text = "ПОДТВЕРЖДЕНИЕ !!!\n" +
                $"Ваше имя: {Name} \n" +
                $"Ваш мастер: {dataBase.GetMasterNameForKlient(Convert.ToString(chatId))}\n" +
                $"Услга: {dataBase.GetServicesNameForKlient(Convert.ToString(chatId))}\n" +
                $"Стоимость: {dataBase.GetServicesStoimostForKlient(Convert.ToString(chatId))}\n" +
                $"Дата записи: {dateTime.ToString("dddd", russianCulture)}, {dateTime.Day} {dateTime.ToString("MMM", russianCulture)} в {dateTime.Hour} : {dateTime.Minute}0" + "\n";
            
            var buttons = new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Подтвердить", "Finish"),
                        InlineKeyboardButton.WithCallbackData("Отмена", "All_Back")},
            };
            var markup = new InlineKeyboardMarkup(buttons);

                // Редактируем сообщение
                await bot.EditMessageText(
                    chatId,
                    Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))), // здесь нужен messageId оригинального сообщения
                    text: text,
                    replyMarkup: markup
                );
        }

        // Шестая страница (Последняя)
        static async Task SendFinish(ITelegramBotClient bot, long chatId, CallbackQuery callback)
        {

            string text = "Списибо что вы с нами";
            // Редактируем сообщение
            await bot.EditMessageText(
                chatId,
                Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))), // здесь нужен messageId оригинального сообщения
                text: text
            );
            await bot.AnswerCallbackQuery(callback.Id, "Запись подтверждена \U0001fae1 \n\n\n\n" +
                "Ваш  мастер: " + dataBase.GetMasterNameForKlient(Convert.ToString(chatId)) + "\n" +
                "Ваша услуга: " + dataBase.GetServicesNameForKlient(Convert.ToString(chatId)) + "\n" +
                "\n\n\nСтоимость: " + dataBase.GetServicesStoimostForKlient(Convert.ToString(chatId)), true);
            var replyKeyboard = new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Записаться"), new KeyboardButton("Мои записи") } })
            {
                ResizeKeyboard = true
            };
            await bot.SendMessage(chatId, "", replyMarkup: replyKeyboard);

            string MessageText =
                "НОВАЯ ЗАПИСЬ !!!\n\n" +
                "Ваш  мастер: " + dataBase.GetMasterNameForKlient(Convert.ToString(chatId)) + "\n" +
                "Ваша услуга: " + dataBase.GetServicesNameForKlient(Convert.ToString(chatId)) + "\n" +
                "\nСтоимость: " + dataBase.GetServicesStoimostForKlient(Convert.ToString(chatId));
            await bot.SendMessage(AdminChatID, MessageText);
        }

        //Обработка ошибок
        static Task handleErrorAsync(ITelegramBotClient bot, Exception exception, System.Threading.CancellationToken cancellationToken)
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
}
