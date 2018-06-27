﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Example
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _provider;

        public async Task Install(IServiceProvider provider)
        {
            _commands = new CommandService();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _provider = provider;
            _client = provider.GetService<DiscordSocketClient>();

            _client.MessageReceived += HandleCommand;
        }

        private async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix("p~>", ref argPos))) return;

            var context = new CommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);
            if (!result.IsSuccess)
                await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
        }
    }
}
