using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace MKEHCBot
{
    public class PunisherUser
    {
        public DateTime unpunishmentStamp;
        public SocketGuildUser userName;

        public PunisherUser(SocketGuildUser inUser)
        {
            userName = inUser;
            unpunishmentStamp = DateTime.Now.AddDays(1);

        }
    }
}
