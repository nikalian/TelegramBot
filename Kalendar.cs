using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Transactions;

namespace MilanaBot
{
    class Kalendar : Program
    {
        public static int CurrentMonth;
        public static DataBase dataBase = new DataBase();
        public static async Task ShowCalendar(ITelegramBotClient bot, long chatId, DateTime date)
        {
            
            int year = date.Year;
            int month = date.Month;   

            var firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7 + 1;

            var buttons = new List<InlineKeyboardButton[]>();
            
            //Текущий месяц
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($" {date:MMMM yyyy}", "none") });

            // Названия дней недели
            buttons.Add(new[]
            {
            InlineKeyboardButton.WithCallbackData("Пн", "none"),
            InlineKeyboardButton.WithCallbackData("Вт", "none"),
            InlineKeyboardButton.WithCallbackData("Ср", "none"),
            InlineKeyboardButton.WithCallbackData("Чт", "none"),
            InlineKeyboardButton.WithCallbackData("Пт", "none"),
            InlineKeyboardButton.WithCallbackData("Сб", "none"),
            InlineKeyboardButton.WithCallbackData("Вс", "none") });

            // Создаем сетку дней
            int currentDay = 1;
            for (int week = 0; week < 6; week++)
            {
                var weekButtons = new List<InlineKeyboardButton>();
                for (int dayOfWeek = 1; dayOfWeek <= 7; dayOfWeek++)
                {
                    if (week == 0 && dayOfWeek < startDayOfWeek)
                    {
                        weekButtons.Add(InlineKeyboardButton.WithCallbackData(" ", "none"));
                    }
                    else if (currentDay > daysInMonth)
                    {
                        weekButtons.Add(InlineKeyboardButton.WithCallbackData(" ", "none"));
                    }
                    else
                    {
                        weekButtons.Add(InlineKeyboardButton.WithCallbackData($"{currentDay}", $"{currentDay}_day"));
                        currentDay++;
                    }
                }
                buttons.Add(weekButtons.ToArray());
                if (currentDay > daysInMonth)
                    break;
            }

            CurrentMonth = date.Month;

            // Переключение месяцев
            buttons.Add(new[]{InlineKeyboardButton.WithCallbackData($"{date.AddMonths(1):MMMM yyyy >}", "next_month") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"Отмена", "BackIsData") });

            await bot.EditMessageText(chatId, Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))), "Выберите дату:", replyMarkup: new InlineKeyboardMarkup(buttons));
        }
        public static async Task ShowCalendarAddMonth(ITelegramBotClient bot, long chatId, DateTime date)
        {

            int year = date.Year;
            int month = date.Month;

            var firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7 + 1;

            var buttons = new List<InlineKeyboardButton[]>();
            
            //Текущий месяц
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($" {date:MMMM yyyy}", "none") });

            // Названия дней недели
            buttons.Add(new[]
            {
            InlineKeyboardButton.WithCallbackData("Пн", "none"),
            InlineKeyboardButton.WithCallbackData("Вт", "none"),
            InlineKeyboardButton.WithCallbackData("Ср", "none"),
            InlineKeyboardButton.WithCallbackData("Чт", "none"),
            InlineKeyboardButton.WithCallbackData("Пт", "none"),
            InlineKeyboardButton.WithCallbackData("Сб", "none"),
            InlineKeyboardButton.WithCallbackData("Вс", "none") });

            // Создаем сетку дней
            int currentDay = 1;
            for (int week = 0; week < 6; week++)
            {
                var weekButtons = new List<InlineKeyboardButton>();
                for (int dayOfWeek = 1; dayOfWeek <= 7; dayOfWeek++)
                {
                    if (week == 0 && dayOfWeek < startDayOfWeek)
                    {
                        weekButtons.Add(InlineKeyboardButton.WithCallbackData(" ", "none"));
                    }
                    else if (currentDay > daysInMonth)
                    {
                        weekButtons.Add(InlineKeyboardButton.WithCallbackData(" ", "none"));
                    }
                    else
                    {
                        weekButtons.Add(InlineKeyboardButton.WithCallbackData($"{currentDay}", $"{currentDay}_day"));
                        currentDay++;
                    }
                }
                buttons.Add(weekButtons.ToArray());
                if (currentDay > daysInMonth)
                    break;
            }
            CurrentMonth = date.Month;

            // Переключение месяцев
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"⬅ {date.AddMonths(-1):MMMM yyyy}", "back_month") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"Отмена", "BackIsData") });

            await bot.EditMessageText(chatId, Convert.ToInt32(dataBase.GetKlientsMessageID(Convert.ToString(chatId))), "Выберите дату:", replyMarkup: new InlineKeyboardMarkup(buttons));
        
        }
    }
}
