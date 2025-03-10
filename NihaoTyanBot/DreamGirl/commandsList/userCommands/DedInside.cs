using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;

namespace NihaoTyan.Bot.commandsList.userCommands
{
    /// <summary>
    /// Команда по "призыву" собутыльника.
    /// </summary>
    public class DedInside
    {
        private static bool _awaitingMention = false;
        private static long _userId;
        private static long _chatId;

        /// <summary>
        /// Инициализирует взаимодействие с пользователем и устанавливает начальное состояние.
        /// </summary>
        public static async Task DedInsideStart(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken = default)
        {
            _chatId = message.Chat.Id;
            _userId = message.From.Id;
            usr[_userId].FSM = "AwaitingMention";
            _awaitingMention = true;

            await botClient.SendTextMessageAsync(_chatId, 
                "Здравия желаю, господа-коммунисты-бояре!\nКто нас интересует? Напишите его ник с @.", 
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Обрабатывает упоминание пользователя.
        /// </summary>
        private static async Task HandleMention(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken = default)
        {
            if (!_awaitingMention || message.Chat.Id != _chatId || message.From.Id != _userId || message.Type != MessageType.Text)
                return;

            if (message.Text.StartsWith("@"))
            {
                _awaitingMention = false;
                for (int i = 0; i < 15; i++)
                {
                    var sentMessage = await botClient.SendTextMessageAsync(_chatId, message.Text, cancellationToken: cancellationToken);
                    await Task.Delay(1000, cancellationToken);
                    await botClient.DeleteMessageAsync(_chatId, sentMessage.MessageId, cancellationToken);
                }
                usr[_userId].FSM = "MainBomb";
            }
            else
            {
                await botClient.SendTextMessageAsync(_chatId,
                    "Нет-нет-нет, товарищ...\nВы делаете всё не так. Нужно указывать перед ником \"@\".\nНапример: [@githab_parasha]",
                    cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Обработчик входящих сообщений.
        /// </summary>
        public static async Task OnMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            // Если сообщение содержит команду (например, начинается с "/" или имеет сущность команды), пропускаем его.
            if (!string.IsNullOrEmpty(message.Text) &&
                (message.Text.StartsWith("/") ||
                 (message.Entities?.Any(e => e.Type == MessageEntityType.BotCommand) ?? false)))
            {
                return;
            }

            if (message.Chat.Id != _chatId || message.From.Id != _userId)
                return;

            if (usr[_userId].FSM == "AwaitingMention")
            {
                _ = Task.Run(() => HandleMention(message, botClient, cancellationToken));
            }

        }

    }
}
