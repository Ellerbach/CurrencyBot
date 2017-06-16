using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CurrencyBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>

    {
        private string currency;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = await result;

            if (message.Text == "Currency")
            {
                var reply = context.MakeMessage();
                try
                {
                    string setcurrency;
                    //need to implement a way to check the currncy.
                    context.PrivateConversationData.TryGetValue("usercurrency", out setcurrency);
                    if (setcurrency == null)
                        setcurrency = "USD";
                    this.currency = setcurrency;
                    reply.Text = $"Your currency is set to {setcurrency}";
                }
                catch (Exception err)
                {
                    reply.Text = $"Error: {err.Message}";
                }
                await context.PostAsync(reply);
                context.Call(new CurrencyDialog(this.currency), this.CurrencyDialogResumeAfter);
            }
            else if (message.Text == "Convert")
            {
                context.Call(new ConvertDialog(this.currency), this.ConvertDialogResumeAfter);
            }
            else
            {
                await this.SendWelcomeMessageAsync(context);
                context.Wait(this.MessageReceivedAsync);
            }
            
        }



        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            //await context.PostAsync("Hi, I'm currency bot.");
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = $"Setup a curency", Value = "Currency", Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = $"Convert a currency", Value = "Convert", Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = $"I'm the currency bot, please select what you want to do",
                Buttons = cardButtons
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);

            await context.PostAsync(reply);
            
        }



        private async Task CurrencyDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {

                this.currency = await result;
                context.PrivateConversationData.SetValue("usercurrency", this.currency);

                await context.PostAsync($"Your currency set to {this.currency}");
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Error: {ex.Message}");
            }
            await this.SendWelcomeMessageAsync(context);
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ConvertDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                await this.SendWelcomeMessageAsync(context);
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Error: {ex.Message}");
            }
            context.Wait(this.MessageReceivedAsync);

        }
    }

}