using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hifumi_Bot.Handler;

namespace Hifumi_Bot.Modules
{
    public class Talk : ModuleBase<SocketCommandContext>
    {

        [Command("speak")]
        [Summary("Speak as you speak as the bot (Administrator)")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Speak([Remainder] string message = "")
        {
            await PermissionWrapper.DeleteMessage(Context.Message);
            await ReplyAsync(message);
        }

        [Command("react")]
        [Summary("Reacts with an image (Administrator)")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task React([Remainder] string message = "")
        {
            await PermissionWrapper.DeleteMessage(Context.Message);

            if (string.IsNullOrEmpty(message))
            {
                await ListReactions();
                return;
            }

            if (File.Exists("Images/Reactions/" + message + ".png"))
            {
                await Context.Channel.SendFileAsync("Images/Reactions/" + message + ".png");
            } else
                await ReplyAsync("No such as reaction: " + message);
        }

        [Command("reactions")]
        [Summary("Shows a list of reactions (Administrator)")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task ListReactions()
        {
            if (!Directory.Exists("Images/Reactions")) return;

            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = Program.embedColor,
                Title = "Reactions",
            };

            DirectoryInfo directory = new DirectoryInfo("Images/Reactions");
            FileInfo[] files = directory.GetFiles();

            if (files.Length <= 0) return;

            string fileNames = files[0].Name;

            for(int i = 1; i < files.Length; i++)
            {
                fileNames += files[i].Name + "\n";
            }

            builder.AddField(x =>
            {
                x.Name = "Image Files";
                x.Value = fileNames;
                x.IsInline = false;
            });

            Embed embed = builder.Build();

            await ReplyAsync("", false, embed);
        }

        [Command("Talk")]
        public void TalkWBot([Remainder] string message = "")
        {
            TalkWithBot(Context.Message);
        }

        public static async Task TalkWithBot(IMessage message)
        {
            Cleverbot.Net.CleverbotResponse response = await Program.cleverbot.GetResponseAsync(message.Content);
            await message.Channel.SendMessageAsync(response.Response);
        }
    }
}
