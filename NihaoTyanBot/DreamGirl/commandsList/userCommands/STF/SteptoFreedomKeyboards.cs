using Telegram.Bot.Types.ReplyMarkups;

namespace NihaoTyan.Bot.commandsList.userCommands.STF.Keyboards
{
    
    public class SteptoFreedomKeyboards
    {
        public static InlineKeyboardMarkup GetKeyboardForStartSTF(long userId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Запуск", $"startSTF{userId}"),
                    InlineKeyboardButton.WithCallbackData("Отмена", $"canelSTF{userId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Изменить время", $"changeSTF{userId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Посмотреть настройки", $"viewSTF{userId}")
                }
            });
        }

        public static InlineKeyboardMarkup GetKeyboardForStartSTFforsettings(long userId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Запуск", $"startSTF{userId}"),
                    InlineKeyboardButton.WithCallbackData("Отмена", $"canelSTF{userId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Изменить время", $"changeSTF{userId}")
                }
            });
        }

        public static InlineKeyboardMarkup GetKeyboardForRestartSTF(long userId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Давай", $"startSTF{userId}"),
                    InlineKeyboardButton.WithCallbackData("Не надо", $"canelSTF{userId}")
                }
            });
        }

        public static InlineKeyboardMarkup GetKeyboardSTFchangeCanel(long userId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Отмена", $"canelSTF{userId}")
                }
            });
        }
    }
}
