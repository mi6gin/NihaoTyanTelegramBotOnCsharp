using NihaoTyan.Bot.commandsList.userCommands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using static NihaoTyan.Bot.commandsList.userCommands.STF.Main.SteptoFreedomMain;
using static NihaoTyan.Bot.commandsList.userCommands.STF.Keyboards.SteptoFreedomKeyboards;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;
using Telegram.Bot.Types.Enums;

namespace NihaoTyanBot.DreamGirl.commandsList.userCommands.STF.SteptoFreedomHandlers
{
    
    public class SteptoFreedomHandlers
    {
        private static long _userId;
        private static long _chatId;
        private static int messToDel;
        private static ITelegramBotClient _botclient;
        
        public static async Task OnMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            (_userId, _chatId, _botclient) = await TrashSTF();
            if (update.Message is not { } message)
                return;

            // Проверка, является ли введённый текст числом
            if (!int.TryParse(message.Text, out int userValue))
                return;

            if (message.Chat.Id != message.Chat.Id || message.From.Id != message.From.Id)
                return;

            if (usr[_userId].FSM == "STFchangeInt1")
            {
                usr[_userId].STFList.FirstOrDefault().FirstTimer = userValue;
                _ = Task.Run(() => STFpressInt2());
            }
            else if (usr[_userId].FSM == "STFchangeInt2")
            {
                usr[_userId].STFList.FirstOrDefault().SecondTimer = userValue;
                _ = Task.Run(() => UpdateUserDataSTF());
            }
        }

        public static async Task CallbackQueryHandlerSTF(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            (_userId, _chatId, _botclient) = await TrashSTF();
            var userId = callbackQuery.From.Id;
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;
            switch (data)
            {
                case var d when d == $"startSTF{userId}":
                    Console.WriteLine("Запуск режима ПРОГУЛКИ");
                    _ = Task.Run(() => STFtoworking(userId, 0, 0));
                    break;
                
                case var d when d == $"canelSTF{userId}":
                    await _botclient.DeleteMessageAsync(_chatId, usr[_userId].MessageTodDellIdForSTFmodule);
                    break;
                
                case var d when d == $"changeSTF{userId}":
                    usr[_userId].FSM = "Абвгдейка";
                    _ = Task.Run(() => STFpressInt1());
                    break;
                
                case var d when d == $"viewSTF{userId}":
                    await _botclient.DeleteMessageAsync(_chatId, usr[_userId].MessageTodDellIdForSTFmodule);
                    
                    var keyboard = GetKeyboardForStartSTFforsettings(userId);
                    var messtoDell = await _botclient.SendTextMessageAsync(
                        _chatId,
                        $"Ваши настройки:\n" +
                        $"*Время работы:* {usr[_userId].STFList.FirstOrDefault().FirstTimer} (в минутах)\n" +
                        $"*Время отдыха:* {usr[_userId].STFList.FirstOrDefault().SecondTimer} (в минутах)\n" +
                        $"*Полученные 🍊:* {usr[_userId].STFList.FirstOrDefault().Orange}",
                        replyMarkup: keyboard,
                        parseMode: ParseMode.Markdown);
                    
                    usr[_userId].MessageTodDellIdForSTFmodule = messtoDell.MessageId;
                    break;
            }
        }
    }
}