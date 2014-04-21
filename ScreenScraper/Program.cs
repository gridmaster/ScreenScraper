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
            string text = string.Empty;
           // string path = @"D:\Projects\screenscrape\opps.txt";
            string baseUri = "http://finance.yahoo.com";

            // This text is added only once to the file. 
            //if (!File.Exists(path))
            //{
            //    // Create a file to write to. 
            //    string createText = "Hello and Welcome" + Environment.NewLine;
            //    File.WriteAllText(path, createText);
            //}

            // This text is always added, making the file longer over time 
            // if it is not deleted. 
            //string appendText = "This is extra text" + Environment.NewLine;
            //File.AppendAllText(path, appendText);

            // Open the file to read from. 
            //string readText = File.ReadAllText(path);

            //string newtext = readText.Substring(readText.IndexOf("View By Expiration:", System.StringComparison.Ordinal));
            //string money = newtext.Substring(0, newtext.IndexOf("</a><table", System.StringComparison.Ordinal));

            //string[] opps = money.Split('|');

            string sym = "IBM";

            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString(string.Format("http://finance.yahoo.com/q/op?s={0}+Options", sym));

            string newtext = webData.Substring(webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal));
            string money = newtext.Substring(0, newtext.IndexOf("</a><table", System.StringComparison.Ordinal));

            string[] opps = money.Split('|');

            Dictionary<string, string> mydic = new Dictionary<string, string>();

            string url = "http://finance.yahoo.com/q/op?s={0}+Options";

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
                using (StreamWriter sw = new StreamWriter(path + sym + item.Key.ToString(CultureInfo.InvariantCulture) + ".htm"))
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
    }
}
