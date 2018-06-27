using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot
{


    public class GlobalUser
    {

        public SaveDiscordUser savedUser;
        public string Username;

        public ulong UserID;

        public string email;

        public bool boolIsChattingWithBot = false;
        public int saidLoveYou;

        public GlobalUser(SocketUser user)
        {
            Username = user.Username;
            UserID = user.Id;

            LoadData();
            SaveData();
        }

        public SaveDiscordUser LoadData()
        {
            if(File.Exists("DiscordUserFiles/" + UserID + ".json"))
            {
                String JSONstring = File.ReadAllText("DiscordUserFiles/" + UserID + ".json");
                SaveDiscordUser save = JsonConvert.DeserializeObject<SaveDiscordUser>(JSONstring);
                if(save != null)
                {
                    Username = save.Username;
                    UserID = save.UserID;
                    email = save.email;
                    return save;
                }
            }
            return null;
        }

        public async Task SaveData()
        {
            savedUser = new SaveDiscordUser(this);

            string outputJSON = JsonConvert.SerializeObject(savedUser);

            string jsonFormatted = JToken.Parse(outputJSON).ToString(Formatting.Indented);

            FileStream stream = null;
            if (!Directory.Exists("DiscordUserFiles/"))
                Directory.CreateDirectory("DiscordUserFiles/");
            if (!File.Exists("DiscordUserFiles/" + UserID + ".json"))
                stream = File.Create("DiscordUserFiles/" + UserID + ".json");

            if(stream != null)
                stream.Close();
            File.WriteAllText("DiscordUserFiles/" + UserID + ".json", jsonFormatted);

        }

        public static void DeleteServerFile(SocketUser user)
        {
            if(File.Exists("DiscordUserFiles / " + user.Id + ".json"))
                File.Delete("DiscordUserFiles / " + user.Id + ".json");
        }
        
    }
    public class SaveDiscordUser
    {
        public string Username;
        public ulong UserID;

        public string email;

        public SaveDiscordUser(GlobalUser user)
        {
            if(user != null)
            {
                UserID = user.UserID;
                Username = user.Username;
                email = user.email;
            }
        }

    }
}
