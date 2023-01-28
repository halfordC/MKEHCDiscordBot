using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEHCBot.Commands
{
    public class FunCommands : BaseCommandModule
    {
        [Command("ping")]
        public async Task testPing(CommandContext ctx) 
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }
    }
}
