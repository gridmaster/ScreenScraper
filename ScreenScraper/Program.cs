using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseUri = "http://finance.yahoo.com";
            string url = "http://finance.yahoo.com/q/op?s={0}+Options";
            string sym = "IBM";

            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString(string.Format("http://finance.yahoo.com/q/op?s={0}+Options", sym));

            string newtext = webData.Substring(webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal));
            string money = newtext.Substring(0, newtext.IndexOf("</a><table", System.StringComparison.Ordinal));

            string[] opps = money.Split('|');

            Dictionary<string, string> mydic = new Dictionary<string, string>();

            url = string.Format(url, sym);

            string key = opps[0];
            key = key.Substring(key.IndexOf('>') + 1);
            key = key.Substring(0, key.IndexOf('<'));

            mydic.Add(key, url);
            
            for (int i = 1; i < opps.Count()-1; i++)
            {
                key = opps[i];
                key = key.Substring(key.IndexOf('>') +1);
                key = key.Substring(0, key.IndexOf('<'));
                url = opps[i];
                url = url.Substring(opps[1].IndexOf('/')).Replace("amp;", "");
                url = baseUri + url.Substring(0, url.IndexOf('>') - 1);
                mydic.Add(key, url);
            }

            string path = @"D:\Projects\Data\";

            foreach (var item in mydic)
            {
                string fileName = sym + " 20" + item.Key.Substring(item.Key.IndexOf(' ') + 1) + GetMonth(item.Key.Substring(0, 3));
                using (StreamWriter sw = new StreamWriter(path + fileName + ".htm"))
                {
                    webData = wc.DownloadString(item.Value);
                    sw.Write(webData);
                }
            }


            Console.WriteLine("Done");
            string fetch = opps[1].Substring(opps[1].IndexOf('/')).Replace("amp;", "");
            fetch = baseUri + fetch.Substring(0, fetch.IndexOf('>') - 1);
            webData = wc.DownloadString(fetch);

            Console.ReadKey();
        }

        public static string GetMonth(string month)
        {
            switch (month.ToUpper())
            {
                case "JAN":
                    return "01";
                case "FEB":
                    return "02";
                case "MAR":
                    return "03";
                case "APR":
                    return "04";
                case "MAY":
                    return "05";
                case "JUN":
                    return "06";
                case "JUL":
                    return "07";
                case "AUG":
                    return "08";
                case "SEP":
                    return "09";
                case "OCT":
                    return "10";
                case "NOV":
                    return "11";
                case "DEC":
                    return "12";
                default:
                    return "00";
            }
        }
    }
}
