using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurrencyBot.Models
{
    public class Currencies
    {
        public Currency[] currencies { get; set; }
        public string name { get; set; }
        public string relevance { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
    }

    //private bool DoesCurrencyExist(string curr)
    //{
    //    //https://restcountries.eu/rest/v2/currency/usd?fields=name;currencies
    //    try
    //    {
    //        string url = $"https://restcountries.eu/rest/v2/currency/{curr}?fields=name;currencies";
    //        var strret = ExecuteRequest(url);
    //        var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Currencies>>(strret);
    //        if (retobjdeser != null)
    //            return true;
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    return false;
    //}

    //private string[] GetListCountry(string curr)
    //{
    //    //https://restcountries.eu/rest/v2/currency/usd?fields=name;currencies
    //    try
    //    {
    //        string url = $"https://restcountries.eu/rest/v2/name/{curr}?fields=name;currencies";
    //        var strret = ExecuteRequest(url);
    //        var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Currencies>>(strret);
    //        //create a string list and return it
    //        string[] ret = new string[retobjdeser.Count];
    //        for (int i = 0; i < retobjdeser.Count; i++)
    //            ret[i] = retobjdeser[i].name;
    //        return ret;
    //    }
    //    catch (Exception ex)
    //    {
    //        return null;
    //    }

    //}

    //private string GetCurrencyFromCountry(string curr)
    //{
    //    //https://restcountries.eu/rest/v2/currency/usd?fields=name;currencies
    //    try
    //    {
    //        string url = $"https://restcountries.eu/rest/v2/name/{curr}?fields=name;currencies";
    //        var strret = ExecuteRequest(url);
    //        var retobjdeser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Currencies>>(strret);
    //        return retobjdeser[0].currencies[0].code;
    //    }
    //    catch (Exception ex)
    //    {
    //        return null;
    //    }

    //}

    //private string ExecuteRequest(string url)
    //{
    //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
    //    try
    //    {
    //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
    //        Stream dataStream = response.GetResponseStream();
    //        // Open the stream using a StreamReader for easy access.
    //        StreamReader reader = new StreamReader(dataStream);
    //        // Read the content.
    //        return reader.ReadToEnd();
    //    }
    //    catch (Exception)
    //    {
    //        return "";
    //    }
    //}
}