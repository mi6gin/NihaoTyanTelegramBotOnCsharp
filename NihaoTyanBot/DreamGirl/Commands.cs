using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using NihaoTyan.Bot.commandsList.userCommands;
using NihaoTyan.Bot.commandsList;
using NihaoTyan.Bot.commandsList.userCommands.NihaoGPT;
using NihaoTyan.Bot.commandsList.userCommands.STF.Main;
using static NihaoTyan.Bot.commandsList.userCommands.Models.UserManager;


namespace NihaoTyan.Bot
{
    public static class Commands
    {
        public static async Task HandleCommandsAsync(Message message, ITelegramBotClient botClient, Update update)
        {
            var chatId = message.Chat.Id;
            await Listofcommands.AllCommands(message, botClient);

            if (message == null || message.Type != MessageType.Text)
                return;
            
            if ( 
                message.Text.StartsWith("/") &
                (
                    !message.Text.StartsWith("/gpt") | 
                    !message.Text.StartsWith("/dedinside")
                    )
                ||
                message.Text.StartsWith("/")
                 )
                usr[message.From.Id].FSM = "Абвгдейка";
            
            var command = message.Text.ToLower();

            if (command.StartsWith("/start"))
            {
                await Tolking.Start(message, botClient);
            }
            else if (command.StartsWith("/help"))
            {
                await Tolking.Help(message, botClient);
            }
            else if (command.StartsWith("/anecdote"))
            {
                await Anecdote.SendAsync(message, botClient);
            }
            else if (command.StartsWith("/dedinside"))
            {
                await DedInside.DedInsideStart(message, botClient);
            }
            else if (command ="/stf")
            {
                await SteptoFreedomMain.StartSteptoFreedomMode(message, botClient);
            }
            else if (command.StartsWith("/gpt"))
            {
                await NihaoGPT.NihaoGPTstart(botClient, update);
            }
        }
    }
}
