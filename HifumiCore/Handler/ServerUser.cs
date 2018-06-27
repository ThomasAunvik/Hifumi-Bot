using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot
{
    [Serializable]
    public class ServerUser
    {
        public string username;
        public ulong userID;
        public bool isBot;

        public DateTime LatestTalk;
        public float Points;

        public DateTime SpamTimeChecker;
        public int messageIn1Second = 0;

        public DateTime BigTextSpamWithin20Sec;
        public int bigTextSpamWithinTime = 0;

        public DateTime FirstSpamWithinHour;
        public int spamTimesWithinHour = 0;

        public DateTime FirstSwearWithinTime;
        public float SwearsWithinTime;
        public DateTime whenMuted;
        public bool isMuted = false;

        public ServerUser(SocketGuildUser user)
        {
            LatestTalk = DateTime.Now;
            FirstSwearWithinTime = DateTime.Now.AddYears(-1);
            SwearsWithinTime = 0;
            if(user != null)
            {
                username = user.Username;
                userID = user.Id;
                isBot = user.IsBot;
            }
        }

        public void UpdateInfo(SocketGuildUser user)
        {
            if(user != null)
            {
                username = user.Username;
                userID = user.Id;
                isBot = user.IsBot;
            }
        }
    }
}
