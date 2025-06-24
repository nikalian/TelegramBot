using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

namespace MilanaBot
{
    class Admins : Program
    {
        public static DataBase dataBase = new DataBase();
        public static System.Threading.CancellationTokenSource CTSadmin;
        public static string MasterID;
        public static string РедактируемаяУслуга;
        public static Message msg;
        public static TelegramBotClient botClient;
        public async Task HandleAdminCommand(ITelegramBotClient bot, Message message)
        {
            botClient = new TelegramBotClient(Spravka.BotToken);
            var me = await botClient.GetMe();
            var chatId = message.Chat.Id;
            var text = message.Text;

            using var cts = new System.Threading.CancellationTokenSource();

            CTSadmin = cts;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // получать все обновления
            };

            Console.WriteLine($"Запущен бот: {me.Username} для администратора");

            // Обработчик входящих сообщений
            botClient.StartReceiving(
                Admin_handleUpdateAsync,
                Admin_handleErrorAsync,
                receiverOptions,
                cts.Token
            );

            Console.ReadLine();
        }

        static async Task Admin_handleUpdateAsync(ITelegramBotClient bot, Update update, System.Threading.CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            if (update.Type == UpdateType.Message && update.Message != null)
            {
                var msg = update.Message;
                var chatId = msg.Chat.Id;
                if (msg.Text != null && update.Message.Chat.Username != Spravka.AdminUserName)
                {
                    await Program.HandleAdminCommand(); // Переключаем на обычного пользователя
                    CTSadmin.Cancel();
                }
                else if (msg.Text != null && msg.Text == "/startAdmins")
                {
                    // Отправляем стартовое сообщение с кнопкой
                    await Admin_SendMainMenu(bot, chatId);
                }
                else
                {
                    await bot.SendMessage(chatId, "Используйте /startAdmins");
                }
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callback = update.CallbackQuery;
                var chatId = callback.Message.Chat.Id;
                Data = update.CallbackQuery;
                // Обрабатываем нажатия на вспомогательные кнопки
                await Admin_HandleCallbackAsync(bot, callback, chatId);
            }
        }

        private static async Task Admin_SendMainMenu(ITelegramBotClient bot, long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Просмотр записей", "GetServices") },
                new[] { InlineKeyboardButton.WithCallbackData("Редактировать услуги", "EditUslug") },
                new[] { InlineKeyboardButton.WithCallbackData("Редактировать мастеров", "EditMasters") },
                new[] { InlineKeyboardButton.WithCallbackData("Изменить рабочий график мастеров", "EditWorkDate") }

            });
            var Message = await bot.SendMessage(chatId, "Добро пожаловать! Выберите функцию", replyMarkup: keyboard);
            chatFullInfo = Message;
        }

        static async Task Admin_HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, long chatId)
        {
            // УСЛУГИ
            if (callback.Data == "EditUslug")
            {
                await ShowPageSelectOtionsServices(bot, chatId);
            }
            //Удаление (Где то ошибка !!!!!!!!!!!!!!!)
            else if (callback.Data == "УдалитьУслугу")
            {
                await ShowPageDeleteService(bot, chatId);
            }
            else if (callback.Data.ToLower().Contains("_НаименованиеУслугиУдаление"))
            {
                РедактируемаяУслуга = callback.Data.Substring(0, callback.Data.LastIndexOf("_"));
                await ShowPageDeleteSelectService(bot, chatId, РедактируемаяУслуга);
            }
            else if (callback.Data == "УдалитьУслугПодтверждить" || callback.Data == "УдалитьУслугОтмена")
            {
                if (callback.Data == "УдалитьУслугПодтверждить")
                {
                    dataBase.DropServices(chatId, РедактируемаяУслуга);
                }
                else
                {
                    bot.DeleteMessage(chatId, msg.Id);
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("Просмотр записей", "GetServices") },
                        new[] { InlineKeyboardButton.WithCallbackData("Редактировать услуги", "EditUslug") },
                        new[] { InlineKeyboardButton.WithCallbackData("Редактировать мастеров", "EditMasters") },
                        new[] { InlineKeyboardButton.WithCallbackData("Изменить рабочий график мастеров", "EditWorkDate") }
                    });
                    var Message = await bot.SendMessage(chatId, "Выберите функцию", replyMarkup: keyboard);
                    chatFullInfo = Message;
                }
            }
            //Добавление
            else if (callback.Data == "ДобавитьУслугу")
            {
                //await bot.SendMessage(chatId, "Напишите название услуги");
                //botClient.Close();
                //GetMessage.Message;
                //GetMessage.BotClient.Close();
            }
            //МАСТЕРА
            else if (callback.Data == "EditWorkDate")
            {
                ShowPageSelectMaster(bot, chatId);
                MasterID = callback.Data.Substring(0, callback.Data.LastIndexOf("_"));
            }
            else if (callback.Data.ToLower().Contains("_мастер"))
            {
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
            else if (callback.Data == "BackIsData")
            {
                ShowPageSelectMaster(bot, chatId);
                MasterID = callback.Data.Substring(0, callback.Data.LastIndexOf("_"));
                await Kalendar.ShowCalendar(bot, chatId, todayDateOnly);
            }
        }

        static async Task ShowPageSelectOtionsServices(ITelegramBotClient bot, long chatId)
        {
            string text = "Что именно вы хотите сделать";
            var buttons = new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Добавить", "ДобавитьУслугу"),
                    InlineKeyboardButton.WithCallbackData("Удалить", "УдалитьУслугу")
                }
            };

            await bot.EditMessageText(
                chatId,
                chatFullInfo.Id,
                text: text,
                replyMarkup: buttons
            );
        }

        static async Task ShowPageDeleteService(ITelegramBotClient bot, long chatId)
        {
            string text = "Выберите услугу которую хотите удалить";
            var Services = dataBase.GetServices();
            var button = new List<InlineKeyboardButton[]>();
            foreach (var Row in Services.Rows)
            {
                string ServicesName = ((System.Data.DataRow)Row).ItemArray[1].ToString();
                button.Add(new[] { InlineKeyboardButton.WithCallbackData(ServicesName, $"{ServicesName}_НаименованиеУслугиУдаление") });
            }
            var markup = new InlineKeyboardMarkup(button);

            await bot.EditMessageText(
                chatId,
                chatFullInfo.Id,
                text: text,
                replyMarkup: markup
            );
        }

        static async Task ShowPageDeleteSelectService(ITelegramBotClient bot, long chatId, string НаименованиеУслуги)
        {
            string text = $"Вы уверенны что хотите удалить услугу '{НаименованиеУслуги}'";
            var buttons = new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Да", "УдалитьУслугПодтверждить"),
                    InlineKeyboardButton.WithCallbackData("Отмена", "УдалитьУслугОтмена")
                }
            };

            var MessageDeleteServices = await bot.EditMessageText(
                chatId,
                chatFullInfo.Id,
                text: text,
                replyMarkup: buttons
            );
            msg = MessageDeleteServices;
        }
        static async Task ShowPageSelectMaster(ITelegramBotClient bot, long chatId)
        {
            string text = "Выберите мастера:";
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
                chatFullInfo.MessageId,
                text: text,
                replyMarkup: markup
            );
        }
        static async Task SelectMastersWorkDate(ITelegramBotClient bot, long chatId)
        {
            string text = "Выберите дату начала работ:";
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
                chatFullInfo.MessageId,
                text: text,
                replyMarkup: markup
            );
        }
        static Task Admin_handleErrorAsync(ITelegramBotClient bot, Exception exception, System.Threading.CancellationToken cancellationToken)
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
