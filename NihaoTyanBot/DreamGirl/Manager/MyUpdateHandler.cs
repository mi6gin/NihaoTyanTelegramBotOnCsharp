using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NihaoTyan.Bot.commandsList.userCommands;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;
using NihaoTyan.Bot.commandsList.userCommands.Models;
using NihaoTyan.Bot.commandsList.userCommands.YtDlp;
using NihaoTyan.Bot.commandsList.userCommands.NihaoGPT;
using NihaoTyanBot.DreamGirl.commandsList.userCommands.STF.SteptoFreedomHandlers;
using NihaoTyan.Bot;

namespace NihaoTyan.Main.Manager
{
    // Класс-обработчик обновлений от Telegram
    public class MyUpdateHandler : IUpdateHandler
    {
        /// <summary>
        /// Обрабатывает входящие обновления от Telegram.
        /// Если получено текстовое сообщение, то обновляет username пользователя, 
        /// управляет настройками в БД, формирует лог-сообщение и распределяет сообщение между обработчиками команд.
        /// Если получен CallbackQuery, то вызывает соответствующий обработчик.
        /// </summary>
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message is Message msg)
            {
                // Обновление данных пользователя
                var user = usr[msg.From.Id];
                user.Username = msg.From.Username;

                // Работа с базой данных для настройки STF
                using (var db = new StepToFDbContext())
                {
                    var setting = await db.STFSettings.FirstOrDefaultAsync(s => s.UserId == msg.From.Id);
                    if (setting == null)
                    {
                        setting = new STF
                        {
                            UserId = msg.From.Id, 
                            FirstTimer = 1, 
                            SecondTimer = 1, 
                            Orange = 0
                        };
                        db.STFSettings.Add(setting);
                        await db.SaveChangesAsync();
                    }
                    else if (!user.STFList.Any())
                    {
                        user.AddSTFRecord(setting);
                    }
                    user.Orange = setting.Orange;
                }
                
                using (var db = new NihaoTelegramUsersDbContext())
                {
                    var setting = await db.NihaoUsers.FirstOrDefaultAsync(s => s.UserId == msg.From.Id);
                    if (setting == null)
                    {
                        setting = new NihaoTelegramUsers
                        {
                            UserId = msg.From.Id, 
                            Username = user.Username,
                        };
                        db.NihaoUsers.Add(setting);
                        await db.SaveChangesAsync();
                    }
                }
                
                // Формирование названия чата
                var chatTitle = string.IsNullOrEmpty(msg.Chat.Title) ? "Личка" : msg.Chat.Title;
                // Формирование лог-сообщения
                var logMsg = $"[{DateTime.Now:HH:mm:ss}] Пользователь {msg.From.Username}({msg.From.Id}) -- {chatTitle} ChatId({msg.Chat.Id}): {msg.Text}";
                Console.WriteLine(logMsg);
                await LogHelper.AppendAsync(logMsg);

                // Обработка команды несколькими обработчиками
                await Task.WhenAll(
                    Commands.HandleCommandsAsync(msg, botClient, update),
                    DedInside.OnMessage(botClient, update, cancellationToken),
                    SteptoFreedomHandlers.OnMessage(botClient, update, cancellationToken),
                    NihaoGPT.OnMessage(botClient, update, cancellationToken),
                    Tikitoki.TTMessage(botClient, update, cancellationToken)
                );
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } callback)
            {
                // Обработка CallbackQuery (например, нажатие на кнопки)
                Console.WriteLine($"[CALLBACK] {callback.Data}");
                await SteptoFreedomHandlers.CallbackQueryHandlerSTF(botClient, callback);
            }
        }

        /// <summary>
        /// Обрабатывает ошибки, возникающие при получении обновлений.
        /// Логирует сообщение об ошибке и стек вызовов.
        /// </summary>
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken cancellationToken)
        {
            var errorMsg = $"[{DateTime.Now:HH:mm:ss}] Ошибка: {exception.Message}\nStackTrace: {exception.StackTrace}";
            Console.WriteLine(errorMsg);
            _ = LogHelper.AppendAsync(errorMsg);
            return Task.CompletedTask;
        }
    }
}
