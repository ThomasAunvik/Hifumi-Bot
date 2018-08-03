using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot
{
    public class Responses
    {
        public static string GetRandomResponce(List<String> list)
        {
            return list[Program.random.Next(0, list.Count)];
        }

        public static List<String> DontSwear = new List<String>() {
            "P-please dont swear, we have our family to protect ",
            "Watch your mouth! Ill subtract your special 3 points out of you ",
            "Are you a child? This is a server for adults "
        };

        // Kicking out people.
        public static List<String> WarningKickOut = new List<String>() {
            "Hey, i told you to keep shut, or else ill kick you ",
            "Quiet down please, or youll disturbe the neighbours, i would then have to move you to a different spot ",
            "Please, shut up, i cant deal with you anymore "
        };

        //Other responses
        public static async Task ILoveYou(GlobalUser user, SocketMessage arg)
        {
            if(!user.boolIsChattingWithBot)
            {
                user.boolIsChattingWithBot = true;
                if(user.saidLoveYou <= 0)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "Hmm", 2000, 1000);
                    await SpeakWithUser(arg, "You're not the first one to say that.", 500, 4000);
                    await SpeakWithUser(arg, "Try to focus on learning... but just between the two of us..", 750, 5000);
                    await SpeakWithUser(arg, "I love you too", 2000, 2000);
                }
                else if(user.saidLoveYou == 1)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "y-you already told me that, I know", 1000, 2000);
                }
                else if(user.saidLoveYou == 2)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "please stop", 1000, 2000);
                }
                else if(user.saidLoveYou == 3)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "say that one more time I'm blocking you", 1000, 2000);
                }
                else if(user.saidLoveYou == 4)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "I'm blocking you now goodbye", 1000, 2000);

                }
                else if(user.saidLoveYou == 5 || user.saidLoveYou == 6)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "I don't care", 1000, 2000);
                }
                else if(user.saidLoveYou == 7)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "I told you before I don't care", 1000, 2000);
                }
                else if(user.saidLoveYou == 8)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "please stop", 1000, 2000);
                }
                else if(user.saidLoveYou == 9)
                {
                    user.saidLoveYou++;
                    await SpeakWithUser(arg, "I'm blocking you", 1000, 2000);
                }
                user.boolIsChattingWithBot = false;
            }
        }
        private static async Task SpeakWithUser(SocketMessage arg, string text, int timeWait, int timeWrite)
        {
            await Task.Delay(timeWait);

            using(arg.Channel.EnterTypingState())
            {
                await Task.Delay(timeWrite);
            }
            await arg.Author.SendMessageAsync(text);
        }
    }
}
