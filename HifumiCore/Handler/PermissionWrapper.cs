using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Hifumi_Bot.Handler
{
    public static class PermissionWrapper
    {
        public static async Task DeleteMessage(IMessage message)
        {
            try
            {
                await message.DeleteAsync();
                return;
            }
            catch(Exception)
            {
                Console.WriteLine("Bot does not have permission to delete message");
                return;
            }
        }
    }
}
