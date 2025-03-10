using System.Text.RegularExpressions;
using NihaoTyan.Main.Manager;
using Telegram.Bot;
using Telegram.Bot.Polling;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;
using NihaoTyan.Bot.commandsList.userCommands.Models;

namespace NihaoTyan.Bot
{
    class Program
    {
        private static ITelegramBotClient _botClient;
        private static Timer _usernameUpdateTimer;

        /// <summary>
        /// Основной метод приложения.
        /// Инициализирует клиента Telegram, базу данных, логирование и обработку входящих сообщений.
        /// Также обрабатывает команды, введённые через консоль.
        /// </summary>
        static async Task Main(string[] args)
        {
            _botClient = new TelegramBotClient(Config.Token);
            SQLitePCL.Batteries.Init();
            
            using (var db = new StepToFDbContext())
                db.Database.EnsureCreated();
            
            using (var db = new NihaoTelegramUsersDbContext())
                db.Database.EnsureCreated();

            var botInfo = await _botClient.GetMeAsync();
            Console.Title = botInfo.Username;
            // Запускаем таймер для ежедневного обновления username пользователей
            _usernameUpdateTimer = new Timer(
                async _ => await UpdateUsernamesAsync(), 
                null, TimeSpan.Zero, TimeSpan.FromDays(1));
            Console.WriteLine($"Bot @{botInfo.Username} запущен.");

            Directory.CreateDirectory(LogHelper.DirectoryPath);
            using var cts = new CancellationTokenSource();
            // Запускаем получение сообщений от Telegram
            _botClient.StartReceiving(new MyUpdateHandler(), new ReceiverOptions { AllowedUpdates = { } }, cts.Token);
            Console.WriteLine("Бот начал получать сообщения");

            // Цикл для чтения консольных команд
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "exit") break;
                await ProcessCommand(input);
            }
            cts.Cancel();
        }

        /// <summary>
        /// Обрабатывает команды, введённые через консоль.
        /// Поддерживаемые команды:
        /// /get_users - вывод списка текущих пользователей с их данными;
        /// /get_chats - вывод уникальных чатов из лог-файла;
        /// /send_message - отправка сообщения в указанный чат.
        /// </summary>
        private static async Task ProcessCommand(string command)
        {
            var parts = command.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            switch (parts[0])
            {
                case "/get_users":
                    Console.WriteLine("Текущие пользователи:");
                    foreach (var user in usr.GetAllUsers())
                    {
                        Console.WriteLine($"UserId: {user.UserId}, Username: {user.Username}, FSM: {user.FSM}");
                        user.STFList.ForEach(stf =>
                            Console.WriteLine($"STF -> UserId: {stf.UserId}, FirstTimer: {stf.FirstTimer}, SecondTimer: {stf.SecondTimer}, Orange: {stf.Orange}")
                        );
                    }
                    break;

                case "/get_chats":
                    await ProcessGetChats();
                    break;

                case "/send_message":
                    if (parts.Length < 3 || !long.TryParse(parts[1], out long chatId))
                    {
                        Console.WriteLine("Использование: /send_message <chat_id> <message>");
                        return;
                    }
                    try
                    {
                        await _botClient.SendTextMessageAsync(chatId, parts[2]);
                        Console.WriteLine("Сообщение отправлено.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    break;
                
                case "/get_telegram_users":
                    await ProcessGetTelegramUsers();
                    break;
            }
        }

        /// <summary>
        /// Обновляет имена пользователей (username) для всех пользователей,
        /// получая актуальные данные через метод GetChatAsync Telegram Bot API.
        /// </summary>
        private static async Task UpdateUsernamesAsync()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Начало обновления Username для всех пользователей.");
            foreach (var user in usr.GetAllUsers())
            {
                try
                {
                    var chat = await _botClient.GetChatAsync(user.UserId);
                    if (!string.IsNullOrEmpty(chat?.Username))
                    {
                        user.Username = chat.Username;
                        Console.WriteLine($"Обновлён Username для UserId {user.UserId}: {user.Username}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении пользователя {user.UserId}: {ex.Message}");
                }
            }
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Завершено обновление Username.");
        }

        /// <summary>
        /// Обрабатывает лог-файл, извлекая информацию о чатах.
        /// Использует регулярное выражение для поиска записей и выводит уникальные чаты.
        /// </summary>
        private static async Task ProcessGetChats()
        {
            if (!File.Exists(LogHelper.FilePath))
            {
                Console.WriteLine("Файл log.txt не найден.");
                return;
            }
            var regex = new Regex(@"^\[\d{2}:\d{2}:\d{2}\] Пользователь (?<username>.+?)\(\d+\) -- (?<chatName>.*?) ChatId\((?<chatId>\-?\d+)\):");
            var chats = (await File.ReadAllLinesAsync(LogHelper.FilePath))
                .Select(line => regex.Match(line))
                .Where(m => m.Success)
                .Select(m => new
                {
                    ChatId = m.Groups["chatId"].Value.Trim(),
                    ChatName = m.Groups["chatName"].Value.Trim() == "Личка"
                        ? m.Groups["username"].Value.Trim()
                        : m.Groups["chatName"].Value.Trim(),
                    ChatType = m.Groups["chatName"].Value.Trim() == "Личка" ? "Пользователь" : "Группа"
                })
                .GroupBy(c => c.ChatId)
                .Select(g => g.First());
            Console.WriteLine("Уникальные чаты:");
            foreach (var chat in chats)
                Console.WriteLine($"ChatId: {chat.ChatId}, Имя: {chat.ChatName}, Тип: {chat.ChatType}");
        }
        
        private static async Task ProcessGetTelegramUsers()
        {
            if (!File.Exists(LogHelper.FilePath))
            {
                Console.WriteLine("Файл log.txt не найден.");
                return;
            }
            var regex = new Regex(@"^\[\d{2}:\d{2}:\d{2}\] Пользователь (?<username>.+?)\(\d+\) -- (?<chatName>.*?) ChatId\((?<chatId>\-?\d+)\):");
            var chats = (await File.ReadAllLinesAsync(LogHelper.FilePath))
                .Select(line => regex.Match(line))
                .Where(m => m.Success)
                .Select(m => new
                {
                    ChatId = m.Groups["chatId"].Value.Trim(),
                    ChatName = m.Groups["chatName"].Value.Trim() == "Личка"
                        ? m.Groups["username"].Value.Trim()
                        : m.Groups["chatName"].Value.Trim(),
                    ChatType = m.Groups["chatName"].Value.Trim() == "Личка" ? "Пользователь" : "Группа"
                })
                .GroupBy(c => c.ChatId)
                .Select(g => g.First())
                .Where(c => c.ChatType == "Пользователь"); // Фильтрация только пользователей
            Console.WriteLine("Уникальные пользователи:");
            foreach (var chat in chats)
                Console.WriteLine($"ChatId: {chat.ChatId}, Имя: {chat.ChatName}");
        }
    }
}
