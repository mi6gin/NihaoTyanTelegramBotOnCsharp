using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;

namespace NihaoTyan.Bot.commandsList.userCommands.NihaoGPT
{
    public class NihaoGPT
    {
        private static ITelegramBotClient _botClient;
        private static long _userId;
        private static long _chatId;

        public static async Task ProcessUserInputAsync(string userInput)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = "ответ не должен содержать больше 300 символов "+userInput } } }
                }
            };

            try
            {
                using var httpClient = new HttpClient();
                string jsonRequest = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(Config.geminiKey, content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseContent);

                string candidateText = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Нет ответа от API.";
                
                await _botClient.SendTextMessageAsync(_chatId, candidateText, parseMode:ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                string errorMessage = ex is HttpRequestException
                    ? $"Request error: {ex.Message}"
                    : $"Error parsing response: {ex.Message}";

                await _botClient.SendTextMessageAsync(_chatId, errorMessage);
            }
        }

        public static async Task NihaoGPTstart(ITelegramBotClient botClient, Update update)
        {
            _botClient = botClient;
            _chatId = update.Message.Chat.Id;
            _userId = update.Message.From.Id;

            usr[_userId].FSM = "NihaoGPT";
            await _botClient.SendTextMessageAsync(_chatId, $"Здравствуй @{update.Message.From.Username}, чем могу помочь?");
        }

        public static async Task OnMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null)
                return;

            // Пропуск команд
            if (!string.IsNullOrEmpty(update.Message.Text) &&
                (update.Message.Text.StartsWith("/") ||
                 (update.Message.Entities?.Any(entity => entity.Type == MessageEntityType.BotCommand) ?? false)))
            {
                return;
            }

            if (update.Message.Chat.Id != _chatId || update.Message.From.Id != _userId)
                return;

            if (usr[_userId].FSM == "NihaoGPT")
            {
                await ProcessUserInputAsync(update.Message.Text);
            }
        }
    }
}
