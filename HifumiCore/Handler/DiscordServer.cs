using System;
using System.Collections.Generic;
using System.IO;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Hifumi_Bot.Modules;

namespace Hifumi_Bot
{
    public class DiscordServer
    {
        private SocketGuild _guild;
        public SocketGuild Guild { get { return _guild; } }

        public SaveDiscordServer discordServerSave;

        public float PointGainDelayInSeconds = 60f;

        //Savings
        public List<ServerUser> Users = new List<ServerUser>();
        public List<VoiceChannelManager.VoiceChannelRole> voiceChannelRoles = new List<VoiceChannelManager.VoiceChannelRole>();

        public List<LootItem> lootItems = new List<LootItem>();
        public float minimumBetPoints = 10;

        public SocketChannel WelcomeChannel;
        public SocketRole WelcomeRole;
        public SocketRole MuteRole;
        public bool swearing;

        public List<string> swearJar;

        public DateTime KickTimeStart;
        public int PlayersKickedWithinTime;

        public DiscordServer(SocketGuild guild)
        {
            _guild = guild;
            LoadData();
            swearJar = new List<string>();

            if (Users == null)
                Users = new List<ServerUser>();
            foreach (SocketGuildUser user in Guild.Users)
                if (Users.Find(x => x.userID == user.Id) == null)
                    Users.Add(new ServerUser(user));
                else
                    Users.Find(x => x.userID == user.Id).UpdateInfo(user);

            SaveData();
        }

        public SaveDiscordServer LoadData()
        {
            if (File.Exists("DiscordServerFiles/" + _guild.Id + ".json"))
            {
                String JSONstring = File.ReadAllText("DiscordServerFiles/" + _guild.Id + ".json");
                SaveDiscordServer save = JsonConvert.DeserializeObject<SaveDiscordServer>(JSONstring);
                if (save != null)
                {
                    discordServerSave = save;
                    if (save.WelcomeChannel != null)
                        WelcomeChannel = Guild.GetChannel(ulong.Parse(save.WelcomeChannel));
                    if (save.WelcomeRole != null)
                        WelcomeRole = Guild.GetRole(ulong.Parse(save.WelcomeRole));
                    if (save.swearJar != null)
                        swearJar = save.swearJar;
                    if (save.Users != null)
                        Users = save.Users;
                    if (save.voiceChannelRoles != null)
                        voiceChannelRoles = save.voiceChannelRoles;

                    if (save.lootItems != null) lootItems = save.lootItems;

                    minimumBetPoints = save.minimumBetPoints;

                    return save;
                }
            }
            return null;
        }

        public void SaveData()
        {
            discordServerSave = new SaveDiscordServer(this);

            string outputJSON = JsonConvert.SerializeObject(discordServerSave);

            string jsonFormatted = JToken.Parse(outputJSON).ToString(Formatting.Indented);

            FileStream stream = null;
            if (!Directory.Exists("DiscordServerFiles/"))
                Directory.CreateDirectory("DiscordServerFiles/");
            if (!File.Exists("DiscordServerFiles/" + _guild.Id + ".json"))
                stream = File.Create("DiscordServerFiles/" + _guild.Id + ".json");
            if (stream != null)
                stream.Close();
            File.WriteAllText("DiscordServerFiles/" + _guild.Id + ".json", jsonFormatted);
        }

        public static void DeleteServerFile(SocketGuild guild)
        {
            if (File.Exists("DiscordServerFiles / " + guild.Id + ".json"))
                File.Delete("DiscordServerFiles / " + guild.Id + ".json");
        }

        public static DiscordServer GetServerFromID(ulong id)
        {
            return Program.discordServers.Find(x => x.Guild.Id == id);
        }
    }

    public class SaveDiscordServer
    {
        public string ServerName;
        public string WelcomeChannel;
        public string WelcomeRole;
        public string MuteRole;
        public bool enabledSwearing;

        public List<string> swearJar;

        public DateTime KickTimeStart;
        public int PlayersKickedWithinTime;

        public List<LootItem> lootItems;
        public float minimumBetPoints;

        public List<ServerUser> Users;
        public List<VoiceChannelManager.VoiceChannelRole> voiceChannelRoles;

        public SaveDiscordServer(DiscordServer server)
        {
            if (server != null)
            {
                if (server.Guild.Name != null)
                    ServerName = server.Guild.Name;
                if (server.WelcomeChannel != null)
                    WelcomeChannel = server.WelcomeChannel.Id.ToString();
                if (server.WelcomeRole != null)
                    WelcomeRole = server.WelcomeRole.Id.ToString();
                if (server.swearJar != null)
                    swearJar = server.swearJar;
                if (server.Users != null)
                    Users = server.Users;
                if (server.voiceChannelRoles != null)
                    voiceChannelRoles = server.voiceChannelRoles;
                if (server.WelcomeRole != null)
                    WelcomeRole = server.WelcomeRole.Id.ToString();

                enabledSwearing = server.swearing;

                if (server.KickTimeStart != null)
                    KickTimeStart = server.KickTimeStart;
                PlayersKickedWithinTime = server.PlayersKickedWithinTime;

                if (server.lootItems != null)
                    lootItems = server.lootItems;

                minimumBetPoints = server.minimumBetPoints;
            }
        }
    }
}