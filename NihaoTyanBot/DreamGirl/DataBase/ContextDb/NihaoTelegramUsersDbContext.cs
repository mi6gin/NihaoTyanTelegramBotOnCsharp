using Microsoft.EntityFrameworkCore;

namespace NihaoTyan.Bot.commandsList.userCommands.Models
{
    public class NihaoTelegramUsersDbContext : DbContext
    {
        public DbSet<NihaoTelegramUsers> NihaoUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine("DreamGirl", "DataBase", "NihaoTelegramUsers.db");
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"База данных не найдена по ожидаемому пути: {dbPath}");
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}