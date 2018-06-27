using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot
{
    public class Gambling : ModuleBase<SocketCommandContext>
    {

        [Command("slot")]
        [Summary("Starts a slot.")]
        public async Task Slot(float betPoints)
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild == Context.Guild);
            if (betPoints < server.minimumBetPoints)
            {
                await ReplyAsync("You cant gamble less than + " + server.minimumBetPoints + " points!");
                return;
            }
            await StartSlotting(betPoints);
        }

        [Command("slot")]
        [Summary("Starts a slot.")]
        public async Task Slot()
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild == Context.Guild);
            await Slot(server.minimumBetPoints);
        }

        [Command("minimumbet")]
        [Summary("Sets the minimum amount of bet points")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetMinimumBetPoints(float points = 10)
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild == Context.Guild);
            if (points > 0)
            {
                server.minimumBetPoints = points;
                await ReplyAsync("Minimum bet points set to: " + points);
            }
            else
            {
                await ReplyAsync("Minimum points cannot be lower than 1");
            }
        }

        public string[] rolls = new string[] {
            ":apple:", ":pineapple:", ":kiwi:", ":tangerine:", ":pear:", ":peach:", ":star:"
        };

        public int[] int3()
        {
            List<int> numbers = new List<int>();
            for(int i = 0; i < rolls.Length; i++)
                numbers.Add(i);

            int one = numbers[Program.random.Next(0, numbers.Count)];
            numbers.Remove(one);
            int two = numbers[Program.random.Next(0, numbers.Count)];
            numbers.Remove(two);
            int three = numbers[Program.random.Next(0, numbers.Count)];
            numbers.Remove(three);
            return new int[3] { one, two, three };
        }

        public async Task StartSlotting(float betPoints)
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild.Id == Context.Guild.Id);
            if (server == null) return;

            ServerUser user = server.Users.Find(x => x.userID == Context.User.Id);
            if (user == null) return;
            
            if(user.Points < betPoints)
            {
                await ReplyAsync("You dont have enough points to bet: " + betPoints + "! (" + user.Points + ")");
                return;
            }
            user.Points -= betPoints;
            
            int[] one = int3();
            int[] two = int3();
            int[] three = int3();
            List<string> selectedRoll = new List<string>() {
                    rolls[one[0]],
                    rolls[two[0]],
                    rolls[three[0]],

                    rolls[one[1]],
                    rolls[two[1]],
                    rolls[three[1]],

                    rolls[one[2]],
                    rolls[two[2]],
                    rolls[three[2]]
                };
            bool won = false;
            if(selectedRoll[0] == selectedRoll[1] && selectedRoll[0] == selectedRoll[2])
            {
                won = true;
            }else if(selectedRoll[3] == selectedRoll[4] && selectedRoll[3] == selectedRoll[5])
            {
                won = true;
            }
            else if(selectedRoll[6] == selectedRoll[7] && selectedRoll[6] == selectedRoll[8])
            {
                won = true;
            }

            LootItem wonItem = null;
            if(server.lootItems.Count > 0) wonItem = server.lootItems[Program.random.Next(0, server.lootItems.Count)];

            Embed newEmbed = new EmbedBuilder()
                    .WithTitle("Slot Machine")
                    .WithColor(Program.embedColor)
                    .WithTimestamp(DateTimeOffset.Now)
                    .WithFooter(footer => {
                        footer
                            .WithText("Slot Machine");
                    })
                    .AddField("D-did you win?",
                    selectedRoll[0] + " | " + selectedRoll[1] + " | " + selectedRoll[2] + "\n"
                    + selectedRoll[3] + "  | " + selectedRoll[4] + "  | " + selectedRoll[5] + "\n"
                    + selectedRoll[6] + "  | " + selectedRoll[7] + "  | " + selectedRoll[8] + "")
                    .AddField("Results:","Betting: " + betPoints + " Points.\n" +
                    (won ? "Won: " + (server.lootItems != null || server.lootItems.Count > 0 ? wonItem.Name : "Nothing") : "Lost: " + betPoints + " Points.")).Build();

            await ReplyAsync("", false, newEmbed);

            GlobalUser gUser = Program.globalUsers.Find(x => x.UserID == Context.User.Id);
            if (won && server.lootItems.Count > 0)
                 if(gUser != null) await Rewards.SendReward(gUser, wonItem, (SocketTextChannel)Context.Channel);

            if (string.IsNullOrEmpty(gUser?.email) && won)
            {
                await ReplyAsync("We reccomend you to register your email so we can send you the rewards. PM " + Program._client.CurrentUser.Mention + " your email!");
            }
        }
    }
}
