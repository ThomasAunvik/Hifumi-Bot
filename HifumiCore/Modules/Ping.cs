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
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Pong!")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }

        [Command("Embed")]
        [Summary("Embed Test. (Admin)")]
        public async Task EmbedTest()
        {
            if(((SocketGuildUser)Context.User).GuildPermissions.Administrator)
            {
                var builder = new EmbedBuilder()
    .WithTitle("Slot Machine")
    .WithColor(Program.embedColor)
    .WithTimestamp(DateTimeOffset.Now)
    .WithFooter(footer => {
        footer
            .WithText("Slot Machine");
    })
    .AddField("Round 1", "" +
    ":apple: | :apple: | :apple:\n" +
    ":pineapple:  | :pineapple:  | :pineapple:\n" +
    ":kiwi:  | :kiwi:  | :kiwi:");
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync("", false, embed)
                    .ConfigureAwait(false);


            }
        }
    }
}
