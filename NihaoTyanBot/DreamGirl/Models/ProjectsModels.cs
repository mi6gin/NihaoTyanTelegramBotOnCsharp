using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NihaoTyan.Bot.commandsList.userCommands.Models
{
    public class NihaoTelegramUsers
    {
        [Key]
        public long UserId { get; set; }
        public string Username { get; set; } 
    }
    
    /// <summary>
    /// Таблица StepToFreedom
    /// </summary>
    public class STF
    {
        [Key]
        public long UserId { get; set; }
        public int FirstTimer { get; set; } 
        public int SecondTimer { get; set; } 
        public int Orange { get; set; } 
    }
    

    public static class UserManager
    {
        public static UserManagerIndex usr { get; } = new UserManagerIndex();

        public class UserManagerIndex
        {
            private static readonly ConcurrentDictionary<long, Usr> _data = new();

            // Метод для получения всех пользователей
            public IEnumerable<Usr> GetAllUsers() => _data.Values;

            private static Usr GetOrCreate(long userId)
            {
                return _data.GetOrAdd(userId, id => new Usr(id));
            }

            public Usr this[long userId] => GetOrCreate(userId);
        }
    }

    public class Usr
    {
        public long UserId { get; }
        public string Username { get; set; }
        public string FSM { get; set; }
        public int Orange { get; set; }
        public int MessageTodDellIdForSTFmodule { get; set; }
        public List<STF> STFList { get; }

        public Usr(long userId)
        {
            UserId = userId;
            FSM = "Абвгдейка";
            Orange = 0;
            MessageTodDellIdForSTFmodule = 0;
            STFList = new List<STF>();
        }

        // Метод для добавления новой записи STF
        public void AddSTFRecord(STF stf)
        {
            if (stf.UserId == UserId)
            {
                using (var context = new StepToFDbContext())
                {
                    var existingSTF = context.STFSettings.Find(stf.UserId);
                    if (existingSTF == null)
                    {
                        context.STFSettings.Add(stf);
                        context.SaveChanges();
                    }
                }
                STFList.Add(stf);
            }
        }
        public void UpdateSTFRecord(STF stf)
        {
            // Пытаемся найти существующую запись
            var localRecord = STFList.FirstOrDefault(s => s.UserId == stf.UserId);
            if (localRecord != null)
            {
                // Обновляем свойства
                localRecord.Orange = stf.Orange;
                localRecord.FirstTimer = stf.FirstTimer;
                localRecord.SecondTimer = stf.SecondTimer;
            }
            else
            {
                // Если записи нет, добавляем её
                STFList.Add(stf);
            }
        }
    }
}
