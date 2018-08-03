using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace Hifumi_Bot.Handler
{
    public class Swearing
    {
        public static async Task CheckText(ServerUser user, DiscordServer server, SocketUserMessage arg)
        {
            if (!server.swearing)
            {
                if (user.SpamTimeChecker.AddSeconds(1) > DateTime.Now)
                {
                    user.messageIn1Second++;
                }
                else
                {
                    user.isMuted = false;
                    user.messageIn1Second = 0;
                    user.SpamTimeChecker = DateTime.Now;
                }

                if (user.BigTextSpamWithin20Sec.AddSeconds(10) > DateTime.Now && arg.Content.Length > 700)
                {
                    user.bigTextSpamWithinTime++;
                    if (user.bigTextSpamWithinTime > 3)
                    {
                        await arg.DeleteAsync();
                        user.spamTimesWithinHour++;
                    }
                }
                else if (arg.Content.Length > 700)
                {
                    user.BigTextSpamWithin20Sec = DateTime.Now;
                    user.bigTextSpamWithinTime = 1;
                }

                if (user.messageIn1Second >= 4 || user.bigTextSpamWithinTime >= 2)
                {
                    if (!user.isMuted)
                    {
                        user.isMuted = true;
                        await arg.Channel.SendMessageAsync("We have now muted " + arg.Author.Mention + " for 30 seconds for excessive spamming." + (user.spamTimesWithinHour > 1 ? " (" + user.spamTimesWithinHour + ")" : ""));
                        user.SpamTimeChecker = DateTime.Now.AddSeconds(30);
                        if (user.FirstSpamWithinHour.AddHours(1) >= DateTime.Now)
                        {
                            user.spamTimesWithinHour++;
                        }
                        else
                        {
                            user.FirstSpamWithinHour = DateTime.Now;
                            user.spamTimesWithinHour = 1;
                        }

                        SocketGuildUser guildUser = server.Guild.Users.ToList().Find(x => x.Id == user.userID);
                        if (guildUser != null)
                        {
                            if (server.MuteRole != null)
                                await guildUser.AddRoleAsync(server.MuteRole);
                            user.whenMuted = DateTime.Now;
                        }
                    }
                    await arg.DeleteAsync();
                }

                if (user.isMuted && (user.spamTimesWithinHour > 4 || user.bigTextSpamWithinTime >= 2) && user.whenMuted < DateTime.Now)
                    if (server.MuteRole != null)
                    {
                        user.isMuted = false;
                        SocketGuildUser guildUser = server.Guild.Users.ToList().Find(x => x.Id == user.userID);
                        if (guildUser != null)
                            await guildUser.RemoveRoleAsync(server.MuteRole);
                    }

                if (user.spamTimesWithinHour > 4)
                {
                    await arg.Channel.SendMessageAsync("We have now kicked out " + arg.Author.Mention + " for excessive spamming! Bye bye o/");
                    await ((SocketGuildUser)arg.Author).KickAsync("Excessive Spamming.");
                }
            }
        }

        public static async Task SwearChecker(SocketUserMessage message, DiscordServer server, ServerUser user)
        {
            if (server.swearing)
                return;

            var punctuation = message.Content.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = message.Content.Split().Select(x => x.Trim(punctuation)).ToList();
            foreach (string swearWord in server.swearJar)
            {
                if (words.Find(x => x.ToLower().Contains(swearWord.ToLower())) != null)
                {
                    await PermissionWrapper.DeleteMessage(message);
                    user.Points -= 3;

                    if (user.FirstSwearWithinTime.AddHours(1) > DateTime.Now)
                    {
                        user.SwearsWithinTime++;
                    }
                    else
                    {
                        user.SwearsWithinTime = 1;
                        user.FirstSwearWithinTime = DateTime.Now;
                    }

                    if (user.SwearsWithinTime == 1 || user.SwearsWithinTime == 2)
                    {
                        await message.Channel.SendMessageAsync(Responses.GetRandomResponce(Responses.DontSwear) + message.Author.Mention);
                    }
                    else if (user.SwearsWithinTime == 3)
                    {
                        await message.Channel.SendMessageAsync(Responses.GetRandomResponce(Responses.WarningKickOut) + message.Author.Mention);
                    }
                    else if (user.SwearsWithinTime == 4)
                    {
                        await message.Channel.SendMessageAsync("I have now muted you for 5 minutes " + message.Author.Mention + ", this is your last chance.");
                        user.isMuted = true;
                        user.whenMuted = DateTime.Now;
                        if (server.MuteRole != null) await ((SocketGuildUser)message.Author).AddRoleAsync(server.MuteRole);
                    }
                    else if (user.SwearsWithinTime >= 5)
                    {
                        if (server.KickTimeStart.AddHours(1) > DateTime.Now)
                        {
                            server.PlayersKickedWithinTime++;
                        }
                        else
                        {
                            server.KickTimeStart = DateTime.Now;
                            server.PlayersKickedWithinTime++;
                        }

                        if (!((SocketGuildUser)message.Author).GuildPermissions.Administrator)
                        {
                            await ((SocketGuildUser)message.Author).KickAsync("For repeatedly swearing.");
                            server.Users.Remove(server.Users.Find(x => x.userID == message.Author.Id));
                        }
                        await message.Channel.SendMessageAsync("We have now kicked out: " + message.Author.Mention);

                        if (server.PlayersKickedWithinTime >= 3)
                            if (File.Exists("Images/Reactions/hifumi_pale.png"))
                                await message.Channel.SendFileAsync("Images/Reactions/hifumi_pale.png");
                    }

                    server.SaveData();
                }
            }
        }
    }
}
