using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hifumi_Bot.Handler;

namespace Hifumi_Bot.Modules
{
    public class SwearJar : ModuleBase<SocketCommandContext>
    {
        [Command("addswear")]
        [Summary("Adds a swearword to the server's swearjar. (Manage Messages)")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddSwear(string word)
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild == Context.Guild);
            if (server != null)
            {
                if (server.swearJar == null)
                    server.swearJar = new List<string>();
                if (server.swearJar.Find(x => x == word) == null)
                {
                    server.swearJar.Add(word);
                    await PermissionWrapper.DeleteMessage(Context.Message);
                    server.SaveData();
                    IUserMessage addedMessage = await ReplyAsync("The word has been added in the swearjar");
                    await Task.Delay(2500);
                }
                else
                {
                    await PermissionWrapper.DeleteMessage(Context.Message);
                    IUserMessage addedMessage = await ReplyAsync("The word is already in the swearjar");
                    await Task.Delay(2500);
                }
            }
        }

        [Command("s")]
        [Summary("Removes a swearword from the server's swearjar. (Manage Messages)")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task RemoveSwear(string word)
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild == Context.Guild);
            if (server != null)
            {
                if (server.swearJar != null)
                {
                    if (server.swearJar.Find(x => x == word) != null)
                    {
                        server.swearJar.Remove(word);
                        server.SaveData();
                        await ReplyAsync("The word has now been removed from the swearjar!");
                    }
                    else
                        await ReplyAsync("The word is not in the list");
                }
                else
                    await ReplyAsync("The word is not in the list");
            }
        }

    }
}
