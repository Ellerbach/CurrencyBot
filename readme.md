# Currency Bot
This is an example of bot using a multi dialog bot where information is passed from one dialog to another with information sent back as well to the root dialog. This bot is using Microsoft Bot Framework. 
This example illustrate as well storage of contextual data for a user. And usage of System.Globalization.CultureInfo.CurrentUICulture to convert correctly decimal numbers using the client local.
This bot has been build to be integrated in another bot [BrickBot](https://brickbot.azurewebsites.com) allowing users to choose their main currency.

## Global architecture
The Rootdialog is storing the user preference for the currency. It can call 2 specialized dialogs, one to choose the currency, one to convert a currency to another. One class CurrencyList to find a currency based on country, one for the exchange rate.
The shema below explain the all up path for the full bot.

![Architecturte](/docs/arch.png)

The main root dialog call the currency dialog to setup the currency, the current currency is passed to the dialog at initialization timeframe, and the selected currency is passed back to the main dialog when it's over with the currency dialog selection.
Same princile with the conversion rate dialog, the currency is passed, the conversion dialog is asking the user form which currency to convert, ask the amount, do the conversion and passing the result as a text to the main dialog. It's just done as an example of passing by an information.

## Getting the user local
One interesting element with Bot Framework is the ability to get the user culture information. It does allow to build smarter bots like converting decimal numbers automatically. Depending on the country you are, the decimal separator can be a dot . like un the US or a coma , like in France or Russia for exmaple. Most countries uses one of this one. 
The System.Globalization.CultureInfo.CurrentUICulture contains automatically this information for the user.
So we can convert automatically using the static float.TryParse function a text to a float using the culture decimal point from the user. 

Teh code extract bellow shows the usage of it. And please note that we can write back to the user the information. It's important in case the user is not using correctly its own local, let say you've setup your cultural settings in English but you're French and always write the decimal numbers with a coma. In case of error, the decimal separator is displayed to the user. The channel used may not support the local correctly or the browser or the direct line. So it is always important to tell the user. See next section for good practice design partners.

[!Decimal separator](/docs/errordecimal.png)

```C#
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
```
## Designing a maximum number of retry
A good practice for bot is to avoid having them asking 50 times the same things if they do not understand. This example implement in the 2 sub dialogs a maximum of 3 attempts. After that, it's back to the main menu.
As seen in the previous point, it is important as well to have clear messages, like in this case dsplaying to the user what decimal separator is expected. 
As per the code below, the principal is simple, you setup a private variable wich is your maximum number of attemps and then decrease it every time you have a problem. If you have multiple steps in the same dialog, don't forget to reset the counter every time you succeed at a step.
```C#
    public class ConvertDialog : IDialog<string>
    {
        const int MAXATTEMPTS = 3;
        private int attempts = MAXATTEMPTS;

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
```
As a result if the conversion is not successfull after 2 try, it goes back to the main menu. 

![Convertion failed](/docs/maxtry.png)

## Asking for more information
When the user is asked for his currency or country, the code is searching the keys or value of the currency list dictionnary. The search for the country is done in plain text. And if only one match, then it is validated as a correct one. But if there are multiple countries possible, it will prompt the user for his choice. In the example below, the country "united" will return multiple options and when one selected, the matching currency will be applied.

![Multiple country selection](/docs/multipleoptions.png) 

In general, it is a good practive on bots to ask such questions when there is an ambiguity. Creating a list with buttons makes it very easy to handle. And keep in mind as well the good practice of maximum number of attemps. The code is quite straight forward, you just have to fill buttons. In this case, I'm sure there are not too many buttons as the search is done only if more than 3 characers. If you would have a long list, you can ask the user for more characters or propose multiple list with a next button. It's bit more complicated but it is need to avoid having a very long list.
```C#
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
```

## Excel is your best friend :-)
Yea, sometimes, you need to create a lot of statics and writing them directly in any editor is a pain. For this I'm using Excel with formulas to generate exactly what I need. A copy/paste in the code will make the trick.
I've used this in the CurrencyList class which does contains a list of countries and their currency in a Dictionnary<string,string> class.

![Excel](/docs/excel.png)

Import your list in Excel, easy from a CSV, copy/paste from a webpage, an json, xml or anything like this
Then just put in as many cells as you needc your separators, in the case of the Dictionnary initialization, **{"** then the key then **","** then the value then **"},**.

Next step is to create a formula, see above for the example, it is just text concatenation.

Then you just select the created colum and past it in Visual Studio for example. Delete the last coma from and use the magic **Ctrl+k+d**. It will make it just super nice for you. 

It's simple, fast and efficient for simple projects which do not require a database for example. And it's easier to maintain texts in Excel than in the code. Still a copy/paste will be necessary if you need to update the code.

## Few considerations
* The currency preference is stored in the user, I'm using PrivateConversationData, see more details on the [Bot Framework documentation](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-state) 
* in this version, the answer text is just raw text, you should consider storing it into resource files so it can use the right local depending on the user choice
* I'm using a free currency exchange service [Currency Layer](https://currencylayer.com/documentation), you'll need to register, free account will be working perfectly for the demo and change the key ion the web.config file. I am refreshing the data only once per day. For my usage it is largely enought. Cashing informaiton is a good practice as well. If your need is to have real time data, you still cash them for the period they are valid. It does always reduce the number of non necessary calls. 
* To deploy this bot, you'll need to create an appID and a secret Key, for more information, check the [Bot Framework documentation](https://docs.microsoft.com/en-us/bot-framework/deploy-bot-overview) to choose the method you prefer

## How to extend this bot
### Internationalization
You can easilly internationalized the bot. Use a resource file like in the [BrickBot](https://brickbot.azurewebsites.com), see most of the source [here](https://www.github.com/ellerbach/brickbot). 

![Resource file](/docs/resourcefile.png)

Create 1 file per culture and then the usage in the code is quite straight forward, either with the name of the resource. In this case using the BrickBot and the resource called WelcomeBrickLink which is a string. It will send back a localized string based on the culture used by the user. 
```C#
var retsrt = BrickBotRes.WelcomeBricklink;
```
Either using the ResourceManager property and searching building your own string. This scenrio is useful when you have multiple strings trigged with a specific element. 
```C#
var retstr = BrickBotRes.ResourceManager.GetString($"{message.Text}Number");
```
The challenge with internationalization is that you really need to think about it at the start and not too late. It's usually a good practice to put all the strings in a resource file. 

### Conveting from any currency to any currency
So far the bot can only convert from or to USD. My need was only this so I didn't implement anything else. But you can extend the bot with 2 currency, the code adaptation will be very easy. It's just about asking 2 currency and returning a specific class from the main dialog that would contains the 2 cureency. So it would looks like that:
```C#
    public class CurrenciesFromTo
    {
        string CurrencyFrom;
        string CurrencyTo;
    }
    //main dialog definition
    public class ConvertDialog : IDialog<CurrenciesFromTo>

    //call of the currency dialog
    CurrenciesFromTo curr;
    context.Call(new CurrencyDialog(curr), this.CurrencyDialogResumeAfter);

    //returning the object
    CurrenciesFromTo curr;
    context.Done(curr);

    //the return function definiition of the main root dialog
    private async Task CurrencyDialogResumeAfter(IDialogContext context, IAwaitable<CurrenciesFromTo> result)
```
You can stay with the rest of the code, especially the Exchange class. Converting from EUR to RUB = EUR to USD and USD to RUB. So no need to make it more complex in this case.
You can easilly add localized names in the CurrencyList class in the language you want. They just have to be unique as they are the key of the Dictionnary<string,string> class used.