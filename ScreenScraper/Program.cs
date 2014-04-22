using System;
using System.Collections.Generic;
using System.Data;
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
            string baseUrl = "http://finance.yahoo.com/q/op?s={0}+Options";
            string sym = "IBM";
            string webData = string.Empty;
            int totalCount = 0;

            DataSet ds = DeSerializationToDataSet();
            DataTable companyTable = ds.Tables["company"];

            foreach (DataRow row in companyTable.Rows)
            {
                int iNdx = 0;
                sym = row["symbol"].ToString();

                totalCount++;

                System.Net.WebClient wc = new System.Net.WebClient();
                do
                {
                    try
                    {
                        webData = string.Empty;
                        webData = wc.DownloadString(string.Format("http://finance.yahoo.com/q/op?s={0}+Options", sym));
                    }
                    catch (Exception ex)
                    {
                        iNdx++;
                        if (iNdx > 99)
                            Console.WriteLine("Symbol {0} hit iNdx {1}", sym, iNdx);
                    }
                } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                                        string.IsNullOrEmpty(webData)) && iNdx < 100);

                if (webData.IndexOf("There is no Options data available") > -1 ||
                        webData.IndexOf("There are no results") > -1 ||
                        webData.IndexOf("Get Quotes Results for ") > -1 ||
                            webData.IndexOf("The remote server returned an error: (999)") > -1)
                {
                    Console.WriteLine("No opetions data for Symbol {0} hit iNdx {1}", sym, iNdx);
                    continue;
                }

                if(string.IsNullOrEmpty(webData))
                {
                    Console.WriteLine("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx);
                    continue;
                }

                Console.WriteLine("Count {0}: Symbol {1} hit iNdx {2} and was saved to file.", totalCount, sym, iNdx);

                string newtext = webData.Substring(webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal));
                string money = newtext.Substring(0, newtext.IndexOf("<table", System.StringComparison.Ordinal));

                string[] opps = money.Split('|');

                Dictionary<string, string> mydic = new Dictionary<string, string>();

                string url = string.Format(baseUrl, sym);

                string key = opps[0];
                key = key.Substring(key.IndexOf('>') + 1);
                key = key.Substring(0, key.IndexOf('<'));

                mydic.Add(key, url);

                for (int i = 1; i < opps.Count(); i++)
                {
                    key = opps[i];
                    key = key.Substring(key.IndexOf('>') + 1);
                    if (key.IndexOf('<') > -1)
                        key = key.Substring(0, key.IndexOf('<'));
                    else
                        key = key;
                    url = opps[i];
                    url = url.Substring(opps[1].IndexOf('/')).Replace("amp;", "");
                    url = baseUri + url.Substring(0, url.IndexOf('>') - 1);
                    mydic.Add(key, url);
                }

                opps = null;

                string path = @"D:\Projects\Data\";
                string getSymbol = string.Empty;
                foreach (var item in mydic)
                {
                    string fileName = sym + " 20" + item.Key.Substring(item.Key.IndexOf(' ') + 1) + GetMonth(item.Key.Substring(0, 3));
                    using (StreamWriter sw = new StreamWriter(path + fileName + ".htm"))
                    {
                        iNdx = 0;

                        do
                        {
                            try
                            {
                                webData = string.Empty;
                                getSymbol = item.Value; // .Replace("&", "&amp;");
                                webData = wc.DownloadString(getSymbol);
                            }
                            catch (Exception ex)
                            {
                                iNdx++;
                                if (iNdx > 99)
                                    Console.WriteLine("Symbol {0} hit iNdx {1}", sym, iNdx);
                            }
                        } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                                        string.IsNullOrEmpty(webData)) && iNdx < 100);

                        if (webData.IndexOf("There is no Options data available") > -1 ||
                                webData.IndexOf("There are no results") > -1 ||
                                    webData.IndexOf("The remote server returned an error: (999)") > -1)
                        {
                            Console.WriteLine("No opetions data for Symbol {0} hit iNdx {1}", sym, iNdx);
                            continue;
                        }

                        if (string.IsNullOrEmpty(webData))
                        {
                            Console.WriteLine("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx);
                            continue;
                        }
                        sw.Write(webData);
                        sw.Flush();
                        sw.Close();
                    }
                }
                mydic.Clear();
            }
        


            Console.WriteLine("Done");
            //string fetch = opps[1].Substring(opps[1].IndexOf('/')).Replace("amp;", "");
            //fetch = baseUri + fetch.Substring(0, fetch.IndexOf('>') - 1);
            // webData = wc.DownloadString(fetch);

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


        private static DataSet DeSerializationToDataSet()
        {
            DataSet deSerializeDS = new DataSet();
            try
            {
                deSerializeDS.ReadXmlSchema(@"D:\Projects\ScreenScraper\ScreenScraper\symbolslist.xml");
                deSerializeDS.ReadXml(@"D:\Projects\ScreenScraper\ScreenScraper\symbolslist.xml", XmlReadMode.IgnoreSchema);
            }
            catch (Exception ex)
            {
                // Handle Exceptions Here…..
            }
            return deSerializeDS;

        }
    }
}
