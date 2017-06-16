using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using CurrencyBot.Models;

namespace CurrencyBot.Dialogs
{
    [Serializable]
    public class ConvertDialog : IDialog<string>
    {
        private string currency;
        const int MAXATTEMPTS = 3;
        private int attempts = MAXATTEMPTS;
        private bool fromUSD = false;

        public ConvertDialog(string currency)
        {
            this.currency = currency;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await this.WelcomeMessageAsync(context);
        }

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            // check if we have a currency stored
            bool iscurrency = false;
            if (currency != null)
                if (currency != "")
                    iscurrency = true;
            if (!iscurrency)
            {
                context.Fail(new Exception("No currency set, please set a currency"));
                return;
            }

            reply.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            cardButtons.Add(new CardAction() { Title = $"From USD to {currency}", Value = "USD", Type = "postBack" });
            cardButtons.Add(new CardAction() { Title = $"From {currency} to USD", Value = $"{currency}", Type = "postBack" });
            HeroCard plCard = new HeroCard()
            {
                Title = $"Please selecxt which conversion you want to do",
                Buttons = cardButtons
            };

            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);

            await context.PostAsync(reply);

            context.Wait(this.AskForAmount);
        }

        public async Task AskForAmount(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            //Need to check if it is a currency or a country
            if (message.Text == "USD")
            {
                fromUSD = true;
                attempts = MAXATTEMPTS;
                reply.Text = $"How much USD do you want to convert in {currency}?";
                await context.PostAsync(reply);
                context.Wait(this.Convert);
            }
            else if (message.Text == currency)
            {
                fromUSD = false;
                attempts = MAXATTEMPTS;
                reply.Text = $"How much {currency} do you want to convert in USD?";
                await context.PostAsync(reply);
                context.Wait(this.Convert);
            }
            else
            {
                attempts--;
                if (attempts > 0)
                {
                    reply.Text = "I don't understand which currency you want to use, please try again";
                    await context.PostAsync(reply);
                    await this.WelcomeMessageAsync(context);
                }
                else
                {
                    context.Fail(new Exception("Sorry, I don't understand the currency you want to convert"));
                }
            }
        }

        public async Task Convert(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            float amount;
            if (float.TryParse(message.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentUICulture, out amount))
            {
                var reply = context.MakeMessage();
                var curr = Exchange.GetCurrency(currency);
                if (fromUSD)
                {
                    reply.Text = $"{amount} USD = {curr * amount} {currency}";
                }
                else
                {
                    reply.Text = $"{amount} {currency} = {amount / curr } USD";
                }
                await context.PostAsync(reply);
                context.Done(reply.Text);
            }
            else
            {

                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync($"I'm sorry, I can't convert your amout, please try again. Make sure you're using {System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator} as a decimal separator.");
                    context.Wait(this.AskForAmount);
                }
                else
                {
                    await context.PostAsync("I'm sorry, I really can't convert your amount, so try another convertion.");
                }
            }
        }
    }
}