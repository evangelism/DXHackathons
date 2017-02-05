using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace ShoeBot
{
    public class ShopLogic : BotLogic
    {

        public ShopLogic(ConnectorClient c) : base(c) { }

        public override async Task Reply(Activity msg)
        {
            await base.Reply(msg);
            await Post("Hello");
        }

    }
}