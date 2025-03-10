using System;
using System.IO;
using System.Threading.Tasks;

namespace NihaoTyan.Main.Manager
{
    // Класс для логирования сообщений в файл
    static class LogHelper
    {
        public const string DirectoryPath = "DreamGirl";
        public const string LogFileName = "log.txt";
        public static readonly string FilePath = Path.Combine(DirectoryPath, LogFileName);

        /// <summary>
        /// Асинхронно добавляет сообщение в лог-файл.
        /// Создаёт папку, если она не существует, и записывает строку с переходом на новую строку.
        /// </summary>
        public static async Task AppendAsync(string message)
        {
            Directory.CreateDirectory(DirectoryPath);
            await File.AppendAllTextAsync(FilePath, message + Environment.NewLine);
        }
    }
}