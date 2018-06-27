using Discord;
using Discord.Addons.Paginator;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot
{
    public class Rewards : ModuleBase
    {
        private readonly PaginationService paginator;

        public Rewards(PaginationService paginationService)
        {
            paginator = paginationService;
        }

        [Command("rewardlist")]
        public async Task Paginate()
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild.Id == Context.Guild.Id);
            if (server != null)
            {
                Console.WriteLine(server.lootItems.Count);
                if(server.lootItems == null || !(server.lootItems.Count > 0))
                {
                    await ReplyAsync("There is no loot.");
                    return;
                }
                var pages = new List<string>();

                int itemPage = 0;
                int itemNumber = 0;
                string page = "";
                foreach (LootItem item in server.lootItems)
                {
                    itemPage++;
                    itemNumber++;
                    page += itemNumber + " - " + item.Name + " : " + item.Description + "\n";
                    if (itemPage > 5)
                    {
                        itemPage = 0;
                        pages.Add(page);
                        page = "";
                    }
                }
                if (page != "") pages.Add(page);

                var message = new PaginatedMessage(pages, "Reward List", new Color(0xb100c1), Context.User);

                await paginator.SendPaginatedMessageAsync(Context.Channel, message);
            }
        }

        [Command("addlootitem")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddItem(string name, string description = "", string link = "")
        {

            DiscordServer server = Program.discordServers.Find(x => x.Guild.Id == Context.Guild.Id);
            if (server != null)
            {
                if (server.lootItems == null) server.lootItems = new List<LootItem>();
                server.lootItems.Add(new LootItem(name, description, link));
                await ReplyAsync("Added " + name + " as a loot.");
                server.SaveData();
            }
        }
        [Command("removelootitem")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveItem(int index)
        {
            DiscordServer server = Program.discordServers.Find(x => x.Guild.Id == Context.Guild.Id);
            if (server != null)
            {
                if (server.lootItems != null || server.lootItems.Count > 0)
                {
                    try
                    {
                        if (server.lootItems[index - 1] == null) await ReplyAsync("There is no item at index " + index);
                        else
                        {
                            server.lootItems.RemoveAt(index - 1);
                            await ReplyAsync("Item Removed...");
                            server.SaveData();
                        }
                    }
                    catch(Exception)
                    {
                        await ReplyAsync("There is no item at index " + index);
                    }
                }
            }
        }

        public static async Task SendReward(GlobalUser user, LootItem item, SocketTextChannel channel)
        {
            if (string.IsNullOrEmpty(user.email))
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp-mail.outlook.com");

                mail.From = new MailAddress("hifumibot@outlook.com");
                mail.To.Add(user.email);
                mail.Subject = "Winning Item: " + item.Name;
                mail.Body = "You have won the item from the lottery \\o/, and the item is: " + item.Name + "! " + item.Description + "\nThe link for getting the reward is here: " + item.url;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("hifumibot@outlook.com", Program.emailPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            else await channel.SendMessageAsync(user.Username + " dont have an email address registered. Just PM me your email.");
        }
    }

    public class LootItem
    {
        public string Name;
        public string Description;
        public string url;
        
        public LootItem(string name, string description, string downloadLink)
        {
            Name = name;
            Description = description;
            downloadLink = url;
        }
    }
}
