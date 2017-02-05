using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.IO;

namespace ShoeBot
{
    [Serializable]
    public enum ShopState { Start, Category, Brand, Menu, Year, Color };

    [Serializable]
    public class ShoeShopDialog : IDialog<string>
    {
        public ShopState State { get; set; }

        public int postdisplay = -1;
        public int whatdisplay = 99; // Menu
        public string CurModel = null;

        public List<string> Cart = new List<string>();

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Hello! I am shoe bot. I will help you select shoes. You can always say help to get help, or menu for main menu.");
            State = ShopState.Start;
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IActivity> result)
        {

            var msg = (Activity)(await result);
            var txt = msg.Text;
            if (txt == null) txt = "";

            if (txt.ToLower().Contains("brand")) { whatdisplay = 0; postdisplay = -1; }
            else if (txt.ToLower().Contains("year")) { whatdisplay = 4; postdisplay = -1; }
            else if (txt.ToLower().Contains("category")) { whatdisplay = 5; postdisplay = -1; }
            else if (txt.ToLower().Contains("color")) { whatdisplay = 2; postdisplay = -1; }
            else if (txt.ToLower().Contains("menu")) { whatdisplay = 99; postdisplay = -1; }
            else if (txt.ToLower() == "add to cart") { Cart.Add(CurModel); whatdisplay = 99; postdisplay = -1; }
            else if (txt.ToLower() == "show cart") { whatdisplay = 97; postdisplay = -1; }
            else if (txt.ToLower() == "clear cart") { Cart.Clear(); whatdisplay = 99; postdisplay = -1; }
            if (msg.Attachments != null && msg.Attachments.Count > 0)
            {
                // This is image!
                whatdisplay = postdisplay = -1;
            }

            if (whatdisplay>=0)
            {
                if (whatdisplay==99) // Display menu
                {
                    var ms = GenMenu(context, new string[] { "Brand", "Category", "Year", "Color", "Show cart" });
                    ms.Text = "How do you want to find your shoes?";
                    await context.PostAsync(ms);
                    postdisplay = whatdisplay; whatdisplay = -1;
                }
                else if (whatdisplay==98) // Display model
                {
                    CurModel = new Data().Filter(3, txt).Content.First();
                    var d = CurModel.Split(';');
                    var ms = context.MakeMessage();
                    ms.Text = $"Model: {d[0]} {d[2]}, Year: {d[4]}";
                    var url = $"http://www.soshnikov.com/temp/xpeh/{d[1]}.jpg";
                    //ms.Attachments = new Attachment[] { new Attachment(contentUrl: url, thumbnailUrl: url, contentType:"image/jpg", name: d[3]) };
                    ms.Attachments = new Attachment[]
                    { new HeroCard()
                      {
                        Images = new CardImage[] { new CardImage(url)}
                      }.ToAttachment()
                    };
                    ms.AddHeroCard("Action", new string[] { "Add to cart", "Menu" });
                    await context.PostAsync(ms);
                    whatdisplay = 99; postdisplay = -1;
                }
                else if (whatdisplay==97) // Display cart
                {
                    var ms = GenCarousel(context, new Data(Cart.ToArray()).Select2(3, 1).Content);
                    ms.Text = "Shopping Cart";
                    await context.PostAsync(ms);
                    ms = GenMenu(context, new string[] { "Clear cart", "Checkout", "Menu" });
                    whatdisplay = 99; postdisplay = -1;
                    await context.PostAsync(ms);
                }
                else
                {
                    var ms = GenMenu(context, new Data().Select(whatdisplay).Unique().Content);
                    ms.Text = "What kind of shoes do you need?";
                    await context.PostAsync(ms);
                    postdisplay = whatdisplay;
                    whatdisplay = -1;
                }
            }
            else if (postdisplay>=0 && postdisplay!=99)
            {
                var ms = GenCarousel(context, new Data().Filter(postdisplay, msg.Text).Select2(3, 1).Content);
                ms.Text = "You may want to consider";
                await context.PostAsync(ms);
                postdisplay = -1; whatdisplay = 98;
            }

            context.Wait(MessageReceivedAsync);
        }

        Func<string, string> fst = (string x) =>
        {
            var a = x.Split(':');
            return a[0];
        };

        Func<string, string> snd = (string x) =>
        {
            var a = x.Split(':');
            if (a.Length > 1) return "http://www.soshnikov.com/temp/xpeh/" + a[1] + ".jpg";
            else return null;
        };

        public IMessageActivity GenMenu(IDialogContext ctx, string[] options)
        {
            var msg = ctx.MakeMessage();

            var Card = new HeroCard()
            {
                Buttons = (from x in options
                           select new CardAction
                           {
                               Type = "imBack",
                               Value = fst(x),
                               Title = fst(x),
                               Image = snd(x)
                           }).ToArray()
            };
            msg.Attachments = new Attachment[] { Card.ToAttachment() };
            return msg;
        }

        public IMessageActivity GenCarousel(IDialogContext ctx, string[] options)
        {
            var msg = ctx.MakeMessage();

            msg.Attachments = 
                (from x in options
                select new HeroCard()
                {
                    Images = new CardImage[] { new CardImage(snd(x), tap: new CardAction() { Type="imBack", Title=fst(x), Value=fst(x), Image=snd(x) })},
                    Text = fst(x)
                }.ToAttachment()).ToArray();
            return msg;
        }


    }

    public class Data
    {

        public string[] Content;

        public Data()
        {
            var f = System.Web.HttpContext.Current.Request.MapPath("~/Data.csv");
            Content = File.OpenText(f).ReadToEnd().Split('\n');
        }

        public Data(string[] x) { Content = x; }
        public Data(IEnumerable<string> x) { Content = x.ToArray(); }


    }

    public static class DataUtility
    {
        public static Data Filter(this Data d, int n, string val)
        {
            return new Data(from x in d.Content
                            where x.Field(n) == val
                            select x);
        }

        public static Data Select(this Data d, int n)
        {
            return new Data(from x in d.Content
                            select x.Field(n));
        }

        public static Data Select2(this Data d, int n1, int n2)
        {
            return new Data(from x in d.Content
                            select $"{x.Field(n1)}:{x.Field(n2)}");
        }

        public static string Field(this string x, int n)
        {
            var r = x.Split(';');
            if (r.Length > n) return r[n]; else return "";
        }

        public static Data Unique(this Data x)
        {
            return new Data(x.Content.OrderBy(z => z).Distinct());
        }

    }

}