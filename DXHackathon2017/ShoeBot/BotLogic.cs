using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;

namespace ShoeBot
{
    public class BotLogic
    {  

        public bool FirstTime { get; private set; } = false;

        protected ConnectorClient connector { get; set; }
        protected Activity curactivity { get; set; }

        public BotLogic(ConnectorClient connector)
        {
            this.connector = connector;
        }

        public virtual async Task Reply(Activity msg)
        {
            curactivity = msg;
        }
        public async Task Post(string s, Attachment[] Attachments = null)
        {
            Activity reply = curactivity.CreateReply(s);
            reply.Attachments = Attachments;
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        public Attachment[] GenOptions(string[] options)
        {
            var Card = new HeroCard()
            {
                Buttons = (from x in options
                           select new CardAction
                           {
                               Type = "imBack",
                               Value = x,
                               Title = x
                           }).ToArray()
            };
            return new Attachment[] { Card.ToAttachment() };
        }

    }
}