using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot.Modules
{
    public class UserInfoCommands : ModuleBase<SocketCommandContext>
    {
        [Command("points")]
        [Summary("Checks how many point you have in the current server.")]
        public async Task Points(string mention = "")
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server != null)
            {
                ServerUser user = null;
                if(Context.Message.MentionedUsers.Count > 0)
                    user = server.Users.Find(x => x.userID == Context.Message.MentionedUsers.ElementAt(0).Id);
                else user = server.Users.Find(x => x.userID == Context.User.Id);
                if(user != null)
                {
                    await ReplyAsync(Context.User.Mention + ". " + user.username + " have " + user.Points + " points!");
                } else {
                    await ReplyAsync("Something went wrong!");
                }
            }
        }

        [Command("leaderboard")]
        [Summary("Checks the top 10 users, including your spot.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task ListPoints()
        {
            try
            {
                DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
                if (server != null)
                {
                    ServerUser user = server.Users.Find(x => x.userID == Context.User.Id);
                    if(user != null)
                    {
                        List<ServerUser> sortedUsers = server.Users;
                        sortedUsers.Sort(delegate (ServerUser c1, ServerUser c2) { return c2.Points.CompareTo(c1.Points); });
                        foreach(ServerUser sUser in sortedUsers.ToList()) if(sUser.isBot) sortedUsers.Remove(sUser);

                        int topView = (sortedUsers.Count < 10 ? sortedUsers.Count : 10);
                        EmbedBuilder embedBuilder = new EmbedBuilder()
                        {
                            Color = Program.embedColor,
                            Title = "Top Players!",
                            Description = "This shows the top " + topView + " players of the server!"
                        };
                        
                        for(int i = 0; i < (sortedUsers.Count < 10 ? sortedUsers.Count : 10); i++)
                        {
                            if(sortedUsers[i] != null)
                            {
                                if(sortedUsers[i].isBot)
                                    continue;
                                embedBuilder.AddField((i+1) + ": " + sortedUsers[i].username, "Points: " + sortedUsers[i].Points, false);
                            }
                        }

                        // Indents with special character (Alt+0173)
                        embedBuilder.AddField("­", "­");

                        int userRank = sortedUsers.IndexOf(user);
                        embedBuilder.AddField("Your rank: " + (userRank + 1), "Points: " + user.Points);

                        await ReplyAsync("", false, embedBuilder.Build());
                    } else {
                        server.Users.Add(new ServerUser((SocketGuildUser)Context.User));
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        [Command("clearbad")]
        [Summary("Clears the bad things that person has done. (Admin)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ClearBadStuff(SocketGuildUser inputUser)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server != null)
            {
                ServerUser user = server.Users.Find(x => x.userID == inputUser.Id);
                if(user != null)
                {
                    await ReplyAsync("Has removed all the bad stuff " + user.username + " has done!");
                    user.FirstSwearWithinTime = user.FirstSwearWithinTime.AddDays(-10);
                    user.SwearsWithinTime = 0;
                    user.isMuted = false;
                    user.messageIn1Second = 0;
                    user.FirstSpamWithinHour = DateTime.Now.AddYears(-1);
                    user.spamTimesWithinHour = 0;
                    user.bigTextSpamWithinTime = 0;
                    user.BigTextSpamWithin20Sec = DateTime.Now.AddYears(-1);

                    if(server.MuteRole != null)
                    {
                        SocketGuildUser guildUser = server.Guild.Users.ToList().Find(x => x.Id == user.userID);
                        if(guildUser != null)
                        {
                            await guildUser.RemoveRoleAsync(server.MuteRole);
                        }
                    }
                    server.SaveData();
                }
            }
        }

        private EmailAddressAttribute e = new EmailAddressAttribute();
        [Command("SetEmail")]
        [Summary("Sets the global email of the user")]
        public async Task SetEmail([Remainder]string email)
        {
            GlobalUser user = Program.globalUsers.FirstOrDefault(x => x.UserID == Context.User.Id);
            if (email.Contains("@") && email.Contains(".") && e.IsValid(email))
            {
                user.email = email;
                await ReplyAsync("Email Set to: " + email);
                await user.SaveData().ConfigureAwait(false);
            }
        }
    }
}
