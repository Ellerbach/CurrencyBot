using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using CurrencyBot.Models;

namespace CurrencyBot.Dialogs
{

    [Serializable]
    public class CurrencyDialog : IDialog<string>
    {
        private string currency;
        private int attempts = 3;

        public CurrencyDialog(string currency)
        {
            this.currency = currency;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await this.WelcomeMessageAsync(context);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = await result;
            if (message == null)
            {
                await this.WelcomeMessageAsync(context);
            }
            if (message.Text == "Yes")
            {
                await context.PostAsync("Please enter your currency (like EUR) or country in English (like Russia)");
                context.Wait(this.AskCurrency);
            }
            else if (message.Text == "No")
            {
                context.Done(this.currency);
            }
            else
            {

                await this.WelcomeMessageAsync(context);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            // check if we have a currency stored
            bool iscurrency = false;
            //string strCurrency;
            //context.PrivateConversationData.TryGetValue<string>("usercurrency", out strCurrency);
            if (currency != null)
                if (currency != "")
                    iscurrency = true;

            reply.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = "Yes", Value = "Yes", Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = "No", Value = "No", Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Buttons = cardButtons
            };
            if (iscurrency)
                plCard.Title = $"You already have a currency setup: {currency}. Do you want to setup a new currency?";
            else
                plCard.Title = "You don't have any currency setup, default one is USD. Do you want to setup a new currency?";

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);

            await context.PostAsync(reply);

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task AskCurrency(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            //Need to check if it is a currency or a country
            string crr = message.Text;
            if (crr.Length <= 3)
            {
                //chances it is a currency
                crr = crr.ToUpper();
                if (CurrencyList.DoesCurrencyExist(crr))
                {
                    context.Done(crr);
                    return;
                }
                //else
                //{
                //    var ret = GetCurrencyFromCountry(crr);
                //    if (ret != "")
                //    {
                //        context.Done(ret);
                //        return;
                //    }
                //}
            }
            else
            {
                //chances it is a country
                var ret = CurrencyList.GetListCountry(crr);
                if (ret != null)
                {
                    if (ret.Length > 1)
                    {
                        var reply = context.MakeMessage();
                        reply.Attachments = new List<Attachment>();
                        List<CardAction> cardButtons = new List<CardAction>();
                        for (int i = 0; i < ret.Length; i++)
                            cardButtons.Add(new CardAction() { Title = ret[i], Value = ret[i], Type = "postBack" });
                        HeroCard plCard = new HeroCard()
                        {
                            Title = $"Please select your country",
                            Buttons = cardButtons
                        };
                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);

                        await context.PostAsync(reply);

                        context.Wait(this.AskCurrency);
                        return;
                    }
                    else
                    {
                        var curr = CurrencyList.GetCurrencyFromCountry(ret[0]);
                        if (curr != null)
                        {
                            context.Done(curr);
                            return;
                        }
                    }
                }
            }

            --attempts;
            if (attempts > 0)
            {
                await context.PostAsync("I'm sorry, I don't understand your reply. Can you give me a country or a currency?");
                context.Wait(this.AskCurrency);
            }
            else
            {
                await context.PostAsync("I'm sorry, I don't understand your reply. So I will setup your currency to USD.");
            }

        }

    }

   

}