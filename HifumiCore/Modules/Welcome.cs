using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot.Modules
{
    public class Welcome : ModuleBase<SocketCommandContext>
    {
        [Command("setwelcome")]
        [Summary("Sets the welcome channel to a specific channel. (Manage Channels)")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SetWelcome(ISocketMessageChannel channel)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.WelcomeChannel = channel as SocketChannel;
            server.SaveData();
            await ReplyAsync("Welcome channel set to: " + ((SocketTextChannel)server.WelcomeChannel).Mention + "!");
            return;
        }

        [Command("getwelcome")]
        [Summary("Says where the welcome channel is")]
        public async Task GetWelcome()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server != null)
            {
                if(server.WelcomeChannel != null)
                {
                    await ReplyAsync("Welcome channel is: " + ((SocketTextChannel)server.WelcomeChannel).Mention);
                } else {
                    await ReplyAsync("There is no welcome channel set.");
                }
            }
        }

        [Command("autorole")]
        [Summary("Sets and gets the stuff for autorole. (Manage Channels)")]
        public async Task AutoRole(string mode = "get", string parameter = "")
        {
            if (((SocketGuildUser)Context.User).GuildPermissions.Has(Discord.GuildPermission.ManageChannels))
            {
                DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
                if (mode == "get")
                {
                    if (server.WelcomeRole != null)
                        await ReplyAsync("Current autorole is: " + server.WelcomeRole.Name + "!");
                    else
                        await ReplyAsync("There is no role set.");
                }
                else if (mode == "set")
                {
                    SocketRole FoundRole = null;
                    foreach (SocketRole role in Context.Guild.Roles)
                    {
                        if (role.Name == parameter)
                        {
                            FoundRole = role;
                            break;
                        }
                    }
                    if (FoundRole != null)
                    {
                        server.WelcomeRole = FoundRole;
                        server.SaveData();
                        await ReplyAsync("Auto-Role set to: " + server.WelcomeRole.Name + "!");
                    }
                    else
                        await ReplyAsync("There is no role such as " + parameter);
                }
            }
            else
            {
                await ReplyAsync("You dont have sufficient permissions to set the welcome channel!");
            }
        }

        [Command("setmute")]
        [Summary("Sets the mute role. (Manage Roles)")]
        public async Task SetMuteRole(string parameter)
        {
            if(((SocketGuildUser)Context.User).GuildPermissions.Has(Discord.GuildPermission.ManageRoles))
            {
                DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
                if (server != null)
                {
                    SocketRole FoundRole = null;
                    foreach(SocketRole role in Context.Guild.Roles)
                    {
                        if(role.Name == parameter)
                        {
                            FoundRole = role;
                            break;
                        }
                    }
                    if (FoundRole != null)
                    {
                        server.MuteRole = FoundRole;
                        server.SaveData();
                        await ReplyAsync("Mute Role set to: " + server.MuteRole.Name + "!");
                    }
                    else
                    {
                        await ReplyAsync("There is no role such as " + parameter);
                    }
                }
            }
            else
            {
                await ReplyAsync("You dont have sufficient permissions to set the mute role!");
            }
        }

        [Command("swearing")]
        [Summary("On true you can swear as much as you want. (Admin)")]
        public async Task EnableSwearing(bool value = true)
        {
            if(((SocketGuildUser)Context.User).GuildPermissions.Has(Discord.GuildPermission.Administrator))
            {
                DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
                if (server != null)
                {
                    server.swearing = value;
                    server.SaveData();
                    if (value) await ReplyAsync("You can now swear as much as you want");
                    else await ReplyAsync("You wont be able to swear anymore!");
                }
            }
        }
    }
}
