using NihaoTyan.Bot.commandsList.userCommands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using static NihaoTyan.Bot.commandsList.userCommands.STF.Keyboards.SteptoFreedomKeyboards;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;
using Telegram.Bot.Types.Enums;

namespace NihaoTyan.Bot.commandsList.userCommands.STF.Main
{
    public class SteptoFreedomMain
    {
        private static long _userId;
        private static long _chatId;
        private static int messToDel;
        private static long _data;
        private static ITelegramBotClient _botclient;

        
        public static async Task StartSteptoFreedomMode(Message message, ITelegramBotClient botClient)
        {
            _chatId = message.Chat.Id;
            _userId = message.From.Id;
            _botclient = botClient;

            var keyboard = GetKeyboardForStartSTF(_userId);
            string imagePath = Path.Combine("DreamGirl", "commandsList", "userCommands", "mediaFiles", "nihaoSTF.jpeg");
            
            await using var stream = File.OpenRead(imagePath);
            var inputFile = InputFile.FromStream(stream, "nihaoJPEG.jpg"); // Новый способ создания файла
            var helpMessage = $"Привет товарищ @{usr[message.From.Id].Username}, ты запустил особый режим\n" +
                              "Режим ПРОГУЛКИ подразумевает концентрацию и усидчевость\n\n" +
                              "Kак будешь готов, дай мне знать";
            var messtoDell = await botClient.SendPhotoAsync(
                _chatId, inputFile, 
                caption: helpMessage, 
                replyMarkup: keyboard);
            usr[_userId].MessageTodDellIdForSTFmodule = messtoDell.MessageId;

        }
        public static async Task UpdateUserOrange()
        {
            using (var dbContext = new StepToFDbContext())
            {
                var userSetting = await dbContext.STFSettings.FindAsync(_userId);
                if (userSetting != null)
                {
                    userSetting.Orange++;
                    await dbContext.SaveChangesAsync();
                }
                usr[_userId].UpdateSTFRecord(userSetting);   
                usr[_userId].Orange = userSetting.Orange;
            }
        }
        public static async Task UpdateUserDataSTF()
        {
            await _botclient.DeleteMessageAsync(_chatId, usr[_userId].MessageTodDellIdForSTFmodule);
            using (var dbContext = new StepToFDbContext())
            {
                var userSetting = await dbContext.STFSettings.FindAsync(_userId);
                if (userSetting != null)
                {
                    userSetting.FirstTimer = usr[_userId].STFList.FirstOrDefault().FirstTimer;
                    userSetting.SecondTimer = usr[_userId].STFList.FirstOrDefault().SecondTimer;
                    await dbContext.SaveChangesAsync();
                }
            }
            var keyboard = GetKeyboardForStartSTF(_userId);
            var messtoDell = await _botclient.SendTextMessageAsync(
                _chatId, 
                $"Изменения применены:\n" + $"*Время работы:* {usr[_userId].STFList.FirstOrDefault().FirstTimer}(в минутах)\n" + $"*Время отдыха:* {usr[_userId].STFList.FirstOrDefault().SecondTimer}(в минутах)", 
                replyMarkup:keyboard,
                parseMode: ParseMode.Markdown);
            usr[_userId].MessageTodDellIdForSTFmodule = messtoDell.MessageId;
        }

        public static async Task STFtoworking(long userId, int newDuration1, int newDuration2)
        {
            await _botclient.DeleteMessageAsync(_chatId, usr[_userId].MessageTodDellIdForSTFmodule);
            var user = usr.GetAllUsers().FirstOrDefault(u => u.UserId == _userId);
            if (user != null)
            {
                // Получаем время из базы (в минутах)
                int fTime = user.STFList.FirstOrDefault().FirstTimer;
                int sTime = user.STFList.FirstOrDefault().SecondTimer;

                // Переводим минуты в секунды
                int fTimeSeconds = fTime * 60;
                int sTimeSeconds = sTime * 60;

                // Отправляем сообщение с обратным отсчётом для рабочего времени.
                var message = await _botclient.SendTextMessageAsync(
                    _chatId, 
                    $"Впахивай как стахановец следующие *{TimeSpan.FromSeconds(fTimeSeconds):mm} мин {TimeSpan.FromSeconds(fTimeSeconds):ss} сек*",
                    parseMode: ParseMode.MarkdownV2
                );

                // Обновляем сообщение каждую секунду
                while (fTimeSeconds > 0)
                {
                    await Task.Delay(1000);
                    fTimeSeconds--;
                    await _botclient.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: $"Впахивай как стахановец следующие *{TimeSpan.FromSeconds(fTimeSeconds):mm}:{TimeSpan.FromSeconds(fTimeSeconds):ss}*",
                        parseMode: ParseMode.MarkdownV2
                    );
                }
                // По окончании рабочего времени удаляем сообщение
                await _botclient.DeleteMessageAsync(_chatId, message.MessageId);

                // Отправляем сообщение с обратным отсчётом для отдыха
                message = await _botclient.SendTextMessageAsync(
                    _chatId,
                    $"Отдыхай пролетарий *{TimeSpan.FromSeconds(sTimeSeconds):mm} мин {TimeSpan.FromSeconds(sTimeSeconds):ss} сек*",
                    parseMode: ParseMode.MarkdownV2
                );

                // Обновляем сообщение каждую секунду
                while (sTimeSeconds > 0)
                {
                    await Task.Delay(1000);
                    sTimeSeconds--;
                    await _botclient.EditMessageTextAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        text: $"Отдыхай пролетарий *{TimeSpan.FromSeconds(sTimeSeconds):mm} мин {TimeSpan.FromSeconds(sTimeSeconds):ss} сек*",
                        parseMode: ParseMode.MarkdownV2
                    );
                }
                // По окончании отдыха удаляем сообщение
                await _botclient.DeleteMessageAsync(_chatId, message.MessageId);
                UpdateUserOrange();
                STFyesorno();
            }
            else
            {
                Console.WriteLine("Пользователь не найден.");
            }

        }
        public static async Task STFyesorno()
        {
            var keyboard = GetKeyboardForRestartSTF(_userId);
            string imagePath = Path.Combine("DreamGirl", "commandsList", "userCommands", "mediaFiles", "nihaoSTF.jpeg");
            
            await using var stream = File.OpenRead(imagePath);
            var inputFile = InputFile.FromStream(stream, "STFnihao.png"); 
            var helpMessage = $"Поздравляю товарищ *@{usr[_userId].Username}*, вы получили  *🍊{usr[_userId].Orange}*\n" +
                              "Хочешь ещё?";

            var messtoDell = await _botclient.SendPhotoAsync(
                _chatId, inputFile, 
                caption: helpMessage, 
                parseMode: ParseMode.MarkdownV2, 
                replyMarkup: keyboard
            );
            usr[_userId].MessageTodDellIdForSTFmodule = messtoDell.MessageId;
            
        }
        public static async Task STFpressInt1()
        {
            await _botclient.DeleteMessageAsync(_chatId, usr[_userId].MessageTodDellIdForSTFmodule);
            var keyboard = GetKeyboardSTFchangeCanel(_userId);
            var message = "Введите новое значение числом\\(минуты\\) для *рабочего времени*:";
            var messtoDell = await _botclient.SendTextMessageAsync(
                _chatId, 
                message, 
                replyMarkup: keyboard,
                parseMode: ParseMode.MarkdownV2
                
                );
            usr[_userId].MessageTodDellIdForSTFmodule = messtoDell.MessageId;
            usr[_userId].FSM = "STFchangeInt1";
        }
        
        public static async Task STFpressInt2()
        {
            await _botclient.DeleteMessageAsync(_chatId, usr[_userId].MessageTodDellIdForSTFmodule);
            var message = "Введите новое значение числом\\(минуты\\) для *времени отдыха*:";
            var messtoDell = await _botclient.SendTextMessageAsync(
                _chatId, 
                message, 
                parseMode: ParseMode.MarkdownV2
            );
            usr[_userId].MessageTodDellIdForSTFmodule = messtoDell.MessageId;
            usr[_userId].FSM = "STFchangeInt2";
        }
        public static async Task<(long, long, ITelegramBotClient)> TrashSTF()
        {
            return (_userId, _chatId, _botclient);
        }
    }
}
