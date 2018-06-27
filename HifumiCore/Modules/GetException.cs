using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hifumi_Bot.Modules
{
    public class GetException : ModuleBase<SocketCommandContext>
    {

        Dictionary<Type, string> Exceptions = new Dictionary<Type, string>() {
            { typeof(NullReferenceException), "This exception happens when there is no value assigned to the variable and you are trying to use the variable." },
            { typeof(StackOverflowException), "The exception that is thrown when the execution stack overflows because it contains too many nested method calls. This class cannot be inherited." }

        };

        [Command("exception")]
        public async Task GetExceptionFromText([Remainder] string text)
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = Program.embedColor,
                Description = "Getting exception from C# Library...",
            };
            IUserMessage message = await ReplyAsync("", false, embed.Build());
            try
            {
                embed = new EmbedBuilder()
                {
                    Color = Program.embedColor,
                };
                if (!text.Contains("."))
                {
                    List<Type> exceptions = getTypeByName(text);
                    if (exceptions != null && exceptions.Count > 0)
                    {
                        int foundExceptions = 0;
                        foreach (Type exception in exceptions)
                        {
                            if (Exceptions.TryGetValue(exception, out string exceptionDescription))
                            {
                                foundExceptions++;
                                embed.AddField(exception.FullName, exceptionDescription + "\n" +
                                    "[" + exception.FullName + " Documentation](https://msdn.microsoft.com/en-us/library/" + exception.FullName.ToLower() + ".aspx)");

                            }
                        }
                        if (foundExceptions <= 0) embed.Description = "Cannot find exception... <:hifumi_pout:379026692041211904>";
                        else embed.Title = "Exceptions found: " + foundExceptions;
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        embed = new EmbedBuilder()
                        {
                            Color = Program.embedColor,
                            Description = "Cannot find exception... <:hifumi_pout:379026692041211904>",
                        };
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
                else
                {
                    Type exception = Type.GetType(text);
                    if(Exceptions.TryGetValue(exception, out string exceptionDescription))
                    {
                        embed = new EmbedBuilder()
                        {
                            Color = Program.embedColor,
                            Title = "Exceptions found: 1"
                        };
                        
                        embed.AddField(exception.FullName, exceptionDescription + "\n" +
                                    "[" + exception.FullName + " Documentation](https://msdn.microsoft.com/en-us/library/" + exception.FullName.ToLower() + ".aspx)");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        embed = new EmbedBuilder()
                        {
                            Color = Program.embedColor,
                            Description = "Cannot find exception... <:hifumi_pout:379026692041211904>",
                        };
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static List<Type> getTypeByName(string className)
        {
            List<Type> returnVal = new List<Type>();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].Name == className)
                    {
                        returnVal.Add(assemblyTypes[j]);
                    }
                }
            }

            return returnVal;
        }
    }
}
