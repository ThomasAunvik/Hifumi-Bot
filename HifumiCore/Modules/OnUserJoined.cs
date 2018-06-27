using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot.Modules
{
    public static class OnUserJoined
    {
        public static Task OnUserJoinedServer(SocketUser user)
        {

            return Task.CompletedTask;
        }
    }
}
