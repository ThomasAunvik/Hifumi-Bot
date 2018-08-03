using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot.Modules
{
    public class VoiceChannelManager : ModuleBase<SocketCommandContext>
    {
        [Serializable]
        public struct VoiceChannelRole
        {
            public ulong voiceChannelID;
            public ulong textChannelID;
            public ulong roleID;
        }

        [Command("createvoicerole")]
        [Summary("Creates a voice channel with text channel that hides itself")]
        [RequireUserPermission(Discord.GuildPermission.ManageChannels)]
        [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
        [RequireBotPermission(Discord.GuildPermission.ManageChannels)]
        [RequireBotPermission(Discord.GuildPermission.ManageRoles)]
        public async Task CreateVoiceChannelForRole(string channelName)
        {
            RestRole newRole = await Context.Guild.CreateRoleAsync(channelName);

            RestTextChannel textChannel = await Context.Guild.CreateTextChannelAsync(channelName);
            RestVoiceChannel voiceChannel =  await Context.Guild.CreateVoiceChannelAsync(channelName);

            Discord.OverwritePermissions newRolePermission = new Discord.OverwritePermissions(
                readMessages: Discord.PermValue.Allow
            );

            Discord.OverwritePermissions everyonePermission = new Discord.OverwritePermissions(
                readMessages: Discord.PermValue.Deny
            );

            await textChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, everyonePermission);
            await textChannel.AddPermissionOverwriteAsync(newRole, newRolePermission);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.voiceChannelRoles.Add(new VoiceChannelRole()
            {
                voiceChannelID = voiceChannel.Id,
                textChannelID = textChannel.Id,
                roleID = newRole.Id
            });
            server.SaveData();
        }

        public static Task OnUserJoinedVC(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            SocketGuildUser guildUser = user as SocketGuildUser;
            DiscordServer server = DiscordServer.GetServerFromID(guildUser.Guild.Id);

            if (afterState.VoiceChannel != null)
            {
                VoiceChannelRole vcRole = server.voiceChannelRoles.Find(x => x.voiceChannelID == afterState.VoiceChannel.Id);
                if (vcRole.roleID == 0) return Task.CompletedTask;

                SocketRole role = server.Guild.GetRole(vcRole.roleID);
                if (role == null)
                {
                    server.voiceChannelRoles.Remove(vcRole);
                    server.SaveData();
                    return Task.CompletedTask;
                }

                guildUser.AddRoleAsync(role);
            }

            if(beforeState.VoiceChannel != null)
            {
                VoiceChannelRole vcRole = server.voiceChannelRoles.Find(x => x.voiceChannelID == beforeState.VoiceChannel.Id);
                if (vcRole.roleID == 0) return Task.CompletedTask;

                SocketRole role = server.Guild.GetRole(vcRole.roleID);
                if(role == null)
                {
                    server.voiceChannelRoles.Remove(vcRole);
                    server.SaveData();
                    return Task.CompletedTask;
                }
                guildUser.RemoveRoleAsync(role);
            }

            return Task.CompletedTask;
        }

        public static Task CheckVoiceChannelOnDelete(SocketChannel channel)
        {
            if (channel is SocketVoiceChannel voiceChannel)
            {
                DiscordServer server = DiscordServer.GetServerFromID(voiceChannel.Guild.Id);
                VoiceChannelRole vcRole = server.voiceChannelRoles.Find(x => x.voiceChannelID == voiceChannel.Id);
                
                server.voiceChannelRoles.Remove(vcRole);

                SocketRole role = server.Guild.GetRole(vcRole.roleID);

                if (vcRole.voiceChannelID != 0)
                {
                    SocketTextChannel textChannelDeletion = server.Guild.GetTextChannel(vcRole.textChannelID);
                    if (textChannelDeletion != null) textChannelDeletion.DeleteAsync();
                }

                if (role != null)
                {
                    role.DeleteAsync();
                }

                server.SaveData();
            }

            if(channel is SocketTextChannel textChannel)
            {
                DiscordServer server = DiscordServer.GetServerFromID(textChannel.Guild.Id);
                VoiceChannelRole vcRole = server.voiceChannelRoles.Find(x => x.textChannelID == textChannel.Id);

                server.voiceChannelRoles.Remove(vcRole);

                SocketRole role = server.Guild.GetRole(vcRole.roleID);

                if (vcRole.voiceChannelID != 0)
                {
                    SocketVoiceChannel voiceChannelDeletion = server.Guild.GetVoiceChannel(vcRole.voiceChannelID);
                    if (voiceChannelDeletion != null) voiceChannelDeletion.DeleteAsync();
                }

                if (role != null)
                {
                    role.DeleteAsync();
                }

                server.SaveData();
            }

            return Task.CompletedTask;
        }
    }
}
