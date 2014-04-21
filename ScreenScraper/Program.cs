using System;
using System.Collections.Generic;
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
            string path = @"D:\Projects\screenscrape\opps.txt";
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
            string readText = File.ReadAllText(path);

            string newtext = readText.Substring(readText.IndexOf("View By Expiration:", System.StringComparison.Ordinal));
            string money = newtext.Substring(0, newtext.IndexOf("</a><table", System.StringComparison.Ordinal));

            string[] opps = money.Split('|');

            System.Net.WebClient wc = new System.Net.WebClient();
            string webData = wc.DownloadString("http://finance.yahoo.com/q/op?s=NLY+Options");

            //<a href="/q/op?s=NLY&amp;m=2014-05">May 14</a>

            //using (StreamWriter sw = new StreamWriter())
            //{

            //}

            // make dictionary of key values and then save files.

            Console.WriteLine(readText);
            string fetch = opps[1].Substring(opps[1].IndexOf('/')).Replace("amp;", "");
            fetch = baseUri + fetch.Substring(0, fetch.IndexOf('>') - 1);
            webData = wc.DownloadString(fetch);

            Console.ReadKey();
        }
    }
}
