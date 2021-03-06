﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cleverbot.Net;
using Discord;
using Discord.Addons.Paginator;
using Discord.Commands;
using Discord.WebSocket;
using Hifumi_Bot.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Hifumi_Bot
{
    public class Program
    {
        public const ulong botID = 377558188826034216;
        public const char botPrefix = '!';
        public const string inviteURL = "https://discordapp.com/api/oauth2/authorize?client_id=377558188826034216&permissions=0&scope=bot";
        
        public static string cleverBotApiKey;
        public static string emailPassword;

        public static Color embedColor = new Color(114, 137, 218);

        public static DiscordSocketClient _client;

        public static CommandService _commands;

        public static IServiceProvider _services;

        public static AutoResetEvent autoEvent;

        public static CleverbotSession cleverbot;

        public static List<DiscordServer> discordServers;

        public static List<GlobalUser> globalUsers;

        public static Random random;

        public static Timer stateTimer;

        public Task OnJoinedGuild(SocketGuild guild)
        {
            discordServers.Add(new DiscordServer(guild));
            return Task.CompletedTask;
        }

        public Task OnLeftGuild(SocketGuild guild)
        {
            DiscordServer server = DiscordServer.GetServerFromID(guild.Id);
            if (server != null)
            {
                DiscordServer.DeleteServerFile(guild);
                discordServers.Remove(server);
            }
            return Task.CompletedTask;
        }

        public Task OnReady()
        {
            discordServers = new List<DiscordServer>();
            foreach (SocketGuild guild in _client.Guilds)
                discordServers.Add(new DiscordServer(guild));

            globalUsers = new List<GlobalUser>();
            foreach (DiscordServer server in discordServers)
                foreach (SocketUser user in server.Guild.Users)
                    if (!user.IsBot)
                        if (globalUsers.Find(x => x.UserID == user.Id) == null)
                            globalUsers.Add(new GlobalUser(user));

            return Task.CompletedTask;
        }

        public Task OnUserJoined(SocketGuildUser user)
        {
            DiscordServer server = DiscordServer.GetServerFromID(user.Guild.Id);
            if (server != null)
            {
                ((SocketTextChannel)server.WelcomeChannel).SendMessageAsync("Welcome to the server " + user.Mention + "!");
                user.AddRoleAsync(server.WelcomeRole);
                server.Users.Add(new ServerUser(user));
            }
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task RunBotAsync()
        {
            string botToken = "";
            // Get bot token
            if (File.Exists("botToken.txt"))
            {
                botToken = File.ReadAllText("botToken.txt");
            }
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("Bot Token does not exist, make sure its correct in the botToken.txt file");
                return;
            }

            if (File.Exists("emailPassword.txt"))
            {
                emailPassword = File.ReadAllText("emailPassword.txt");
            }

            if (File.Exists("cleverbotToken.txt"))
            {
                cleverBotApiKey = File.ReadAllText("cleverbotToken.txt");
            }

            random = new Random();
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddPaginator(_client)
                .BuildServiceProvider();

            cleverbot = new CleverbotSession(cleverBotApiKey);

            _client.Log += Log;

            _client.Ready += OnReady;
            _client.LeftGuild += OnLeftGuild;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;

            _client.MessageReceived += async (arg)=>
            {
                if (globalUsers == null)
                    return;

                if (arg is null || arg.Author.IsBot)
                    return;

                if (arg.Content.Contains("https://discord.gg/")) await arg.DeleteAsync();

                GlobalUser user = globalUsers.FirstOrDefault(x => x.UserID == arg.Author.Id);
                if (user == default(GlobalUser)) globalUsers.Add(new GlobalUser(arg.Author));

                if (arg.Content.ToLower().Contains("i love you"))
                {
                    await Responses.ILoveYou(user, arg);
                    return;
                }
            };
            _client.MessageUpdated += OnMessageUpdated;
            _client.LatencyUpdated += OnUpdate;

            _client.UserVoiceStateUpdated += Modules.VoiceChannelManager.OnUserJoinedVC;
            _client.ChannelDestroyed += Modules.VoiceChannelManager.CheckVoiceChannelOnDelete;

            await RegisterCommandsAsync();
            
            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private void AddPoints(SocketUserMessage message, DiscordServer server, ServerUser user)
        {
            if (server != null)
            {
                if (user != null)
                {
                    if (user.LatestTalk.AddSeconds(server.PointGainDelayInSeconds) < DateTime.Now)
                    {
                        user.LatestTalk = DateTime.Now;
                        user.Points += 1;
                        server.SaveData();
                    }
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if (message is null || message.Author.IsBot) return;

            DiscordServer server = discordServers.Find(x => x.Guild.GetChannel(message.Channel.Id) == message.Channel);
            ServerUser user = server.Users.Find(x => x.userID == message.Author.Id);
            await Swearing.CheckText(user, server, message);

            int argPos = 0;
            if (message.HasStringPrefix(botPrefix.ToString(), ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    if (result.ErrorReason != "Unknown command.")
                    {
                        Console.WriteLine(result.ErrorReason);
                        await message.Channel.SendMessageAsync(result.ErrorReason);
                    }
                }
            }
            else if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                Modules.Talk.TalkWithBot(message);
            }

            AddPoints(message, server, user);
            await Swearing.SwearChecker(message, server, user);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            if (arg.Exception != null)
            {
                Console.WriteLine(arg.Exception.StackTrace);
            }

            return Task.CompletedTask;
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            DiscordServer server = discordServers.Find(x => x.Guild.GetChannel(arg2.Channel.Id) == arg2.Channel);
            ServerUser user = server.Users.Find(x => x.userID == arg2.Author.Id);
            await Swearing.SwearChecker((SocketUserMessage)arg2, server, user);
        }

        private async Task OnUpdate(int e, int e2)
        {
            foreach (DiscordServer server in discordServers)
            {
                List<ServerUser> mutedUsers = server.Users.FindAll(x => x.isMuted == true);
                foreach (ServerUser mutedUser in mutedUsers)
                {
                    if (mutedUser.whenMuted.AddMinutes(5) <= DateTime.Now)
                    {
                        mutedUser.isMuted = false;
                        SocketGuildUser user = server.Guild.GetUser(mutedUser.userID);
                        if (user != null)
                        {
                            if (server.MuteRole != null)
                            {
                                await user.RemoveRoleAsync(server.MuteRole);
                            }
                        }
                    }
                }
            }
        }
    }
}