using NihaoTyan.Bot.commandsList.userCommands.YtDlp.Main;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NihaoTyan.Bot.commandsList.userCommands.YtDlp
{
    public class Tikitoki
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _url;

        public static async Task TTMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { Type: MessageType.Text } message)
                return;

            if (!Regex.IsMatch(message.Text, "^(vm|https://(www|vm|vr|vt))\\.tiktok\\.com/"))
                return;

            try
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
                var sentMessage = await botClient.SendTextMessageAsync(message.Chat.Id, $"Скачиваю {message.Text}", cancellationToken: cancellationToken);
                var messToDel = sentMessage.MessageId;

                // Приведение ссылки к полному виду
                _url = message.Text.StartsWith("https") ? message.Text : $"https://{message.Text}";
                var response = await _httpClient.GetAsync(_url, cancellationToken);
                var fullUrl = response.RequestMessage?.RequestUri?.ToString();
                
                if (fullUrl is null)
                {
                    await botClient.EditMessageTextAsync(message.Chat.Id, messToDel, "Ошибка при обработке ссылки", cancellationToken: cancellationToken);
                    return;
                }

                // Проверка на тип контента (слайд-шоу не поддерживаются)
                if (fullUrl.Contains("photo"))
                {
                    await botClient.EditMessageTextAsync(message.Chat.Id, messToDel, $"Скачивание слайд-шоу не поддерживается: {_url}", cancellationToken: cancellationToken);
                    return;
                }

                int messageToDelete = sentMessage.MessageId;
                // Инициализация загрузки видео
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadVideo, cancellationToken: cancellationToken);
                using var outputStream = new MemoryStream();
                await VideoDownloaderHelper.DownloadAndSendVideoAsync(message.Text, botClient, message, messageToDelete, outputStream, cancellationToken);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Не получилось скачать\nP.s. ну и лошара \ud83d\ude02\ud83e\udd23\ud83d\ude02", cancellationToken: cancellationToken);
            }
        }
    }
}
