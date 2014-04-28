using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScreenScraper
{
    class Program
    {
        readonly static string directoryPath = string.Format("D:/Projects/Data/Options {0}{1}{2}",
                                     DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                                     DateTime.Now.Month.ToString("D2"), DateTime.Now.Day.ToString("D2"));
        readonly static int maxCount = 200;
        readonly static string baseUrl = "http://finance.yahoo.com/q/op?s={0}+Options";
        readonly static string baseUri = "http://finance.yahoo.com";
        readonly static string summaryUri = "http://finance.yahoo.com/q?s={0}";
        readonly static string insiderUri = "http://finance.yahoo.com/q/it?s={0}+Insider+Transactions";
        private static int totalCount = 0;

        private static void Main(string[] args)
        {
            string sym = string.Empty;
            string webData = string.Empty;

            string result = GetSymbolList();
            
            DataSet ds = DeSerializationToDataSet(result);
            DataTable companyTable = ds.Tables["company"];

            DataView dv = new DataView(companyTable);
            dv.Sort = "symbol asc";

            DataTable sortedTable = dv.ToTable();

            WriteLog("Starting at: " + DateTime.Now);

            GetInsider(companyTable);

            //GetOptions(companyTable);
            
            WriteLog(string.Format("Ending at: " + DateTime.Now));

            Console.WriteLine("Done");

            Console.ReadKey();
        }

        public static void GetInsider(DataTable companyTable)
        {
            Directory.CreateDirectory(directoryPath.Replace("Options", "Insider"));

            foreach (DataRow row in companyTable.Rows)
            {
                string sym = row["symbol"].ToString();

                totalCount++;

                string webData = GetInsider(sym);

                if (string.IsNullOrEmpty(webData)) continue;

                webData = GetInsiderPage(sym, string.Format(insiderUri, sym));

                if (string.IsNullOrEmpty(webData)) continue;

                string fileName = "/" + sym + " Insider";

                webData = webData.Substring(webData.IndexOf("yfi_itrans_insider_purchases"));
                webData = webData.Substring(webData.IndexOf("<table"));
                var stuff = HtmlWorks.GetText(webData, "table");

                var result = GetRows(stuff);

                webData = webData.Substring(stuff.Length);

                webData = webData.Substring(webData.IndexOf("<table"));
                stuff = HtmlWorks.GetText(webData, "table");

                var bigResult = GetRows(stuff);

                //var bull = stuff.Replace("<tr>", "|").Split('|');

                //IList<string> rowData = new List<string>();

                //foreach (var item in bull)
                //{
                //    if (item == "") continue;
                //    if (item.Substring(0, "<td class".Length) == "<td class")
                //    {
                //        string workstr = string.Empty;
                //        workstr = item.Substring(item.IndexOf(">") + 1);
                //        string colData = string.Empty;
                //        colData = workstr.Substring(0, workstr.IndexOf("</td"));

                //        while (workstr.IndexOf("<td") != -1)
                //        {
                //            workstr = workstr.Substring(workstr.IndexOf("<td"));
                //            workstr = workstr.Substring(workstr.IndexOf(">") + 1);
                //            colData += "|" + workstr.Substring(0, workstr.IndexOf("</td"));
                //        }
                //        rowData.Add(colData);
                //    }
                //}

                using (StreamWriter sw = new StreamWriter(directoryPath + fileName + ".htm"))
                {
                    sw.Write(webData);
                    sw.Flush();
                    sw.Close();
                    WriteLog(string.Format("Count {0}: Symbol {1} and was saved to file.", totalCount, fileName.Substring(1)));
                }
            }
        }

        private static IList<string> GetRows(string webData)
        {
            var arraySplit = webData.Replace("<tr>", "|").Split('|');

            IList<string> rowData = new List<string>();

            foreach (var item in arraySplit)
            {
                if (item == "") continue;
                if (item.Substring(0, "<td class".Length) == "<td class")
                {
                    string workstr = string.Empty;
                    workstr = item.Substring(item.IndexOf(">") + 1);
                    string colData = string.Empty;
                    colData = workstr.Substring(0, workstr.IndexOf("</td"));

                    while (workstr.IndexOf("<td") != -1)
                    {
                        workstr = workstr.Substring(workstr.IndexOf("<td"));
                        workstr = workstr.Substring(workstr.IndexOf(">") + 1);
                        colData += "|" + workstr.Substring(0, workstr.IndexOf("</td"));
                    }
                    rowData.Add(colData);
                }
            }
            return rowData;
        }

        private static string GetInsiderPage(string sym, string url)
        {
            int iNdx = 0;
            string webData = string.Empty;

            System.Net.WebClient wc = new System.Net.WebClient();
            do
            {
                iNdx++;
                try
                {
                    webData = string.Empty;
                    webData = wc.DownloadString(url);
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("Error for Symbol {0} hit iNdx {1}. Error: {2}", sym, iNdx, ex.Message));
                }
            } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                      string.IsNullOrEmpty(webData)) && iNdx < maxCount);

            if (webData.IndexOf("There is no Insider Transactions data available") > -1 ||
                webData.IndexOf("There are no results") > -1 ||
                webData.IndexOf("Get Quotes Results for ") > -1 ||
                webData.IndexOf("is no longer valid") > -1 ||
                webData.IndexOf("The remote server returned an error: (999)") > -1)
            {
                WriteLog(string.Format("No insider data for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            if (string.IsNullOrEmpty(webData))
            {
                WriteLog(string.Format("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            WriteLog(string.Format("Symbol {0} returned insider data at iNdx {1}.", sym, iNdx));

            return webData;
        }
        
        public static void GetOptions(DataTable companyTable)
        {
            
            Directory.CreateDirectory(directoryPath);
            
            WriteLog("Starting at: " + DateTime.Now);
            foreach (DataRow row in companyTable.Rows)
            {
                string sym = row["symbol"].ToString();

                totalCount++;

                string webData = GetInsider(sym);

                webData = GetSummary(sym);
                if (string.IsNullOrEmpty(webData)) continue;

                webData = GetOpionsPage(sym, string.Format("http://finance.yahoo.com/q/op?s={0}+Options", sym));

                if (string.IsNullOrEmpty(webData)) continue;
                
                Dictionary<string, string> mydic = GetUris(sym, webData);

                string getSymbol = string.Empty;
                foreach (var item in mydic)
                {
                    string fileName = "/" + sym + " 20" + item.Key.Substring(item.Key.IndexOf(' ') + 1) +
                                      GetMonth(item.Key.Substring(0, 3));
                    using (StreamWriter sw = new StreamWriter(directoryPath + fileName + ".htm"))
                    {
                        webData = GetOpionsPage(fileName.Replace("/",""), item.Value);
                        if (string.IsNullOrEmpty(webData)) continue;

                        sw.Write(webData);
                        sw.Flush();
                        sw.Close();
                        WriteLog(string.Format("Count {0}: Symbol {1} and was saved to file.", totalCount, fileName.Substring(1)));
                    }
                }
                mydic.Clear();                
            }
        }

        public static string GetInsider(string sym)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            int iNdx = 0;
            string webData = string.Empty;

            try
            {
                webData =
                    wc.DownloadString(string.Format(summaryUri, sym));

                if (webData.IndexOf("+Insider+Transactions\">Insider Transactions</a>") < 0)
                {
                    WriteLog(string.Format("Symbol {0} has no insider data...", sym));
                    webData = "";
                }
            }
            catch (Exception ex)
            {
                iNdx++;
                if (iNdx > maxCount)
                    WriteLog(string.Format("Symbol {0} blew up fetching summary page...", sym));
                webData = "";
            }
            return webData;
        }
        
        public static string GetSummary(string sym)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            int iNdx = 0;
            string webData = string.Empty;

            try
            {
                webData =
                    wc.DownloadString(string.Format(summaryUri, sym));

                if (webData.IndexOf("+Options\">Options</a>") < 0)
                {
                    WriteLog(string.Format("Symbol {0} has no options...", sym));
                    webData = "";
                }
            }
            catch (Exception ex)
            {
                iNdx++;
                if (iNdx > maxCount)
                    WriteLog(string.Format("Symbol {0} blew up fetching summary page...", sym));
                webData = "";
            }
            return webData;
        }

        public static Dictionary<string, string> GetUris(string sym, string webData)
        {
            string newtext =
                webData.Substring(webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal));
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
            return mydic;
        }

        private static string GetOpionsPage(string sym, string url)
        {
            int iNdx = 0;
            string webData = string.Empty;

            System.Net.WebClient wc = new System.Net.WebClient();
            do
            {
                iNdx++;
                try
                {
                    webData = string.Empty;
                    webData = wc.DownloadString(url);
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("Error for Symbol {0} hit iNdx {1}. Error: {2}", sym, iNdx, ex.Message));
                }
            } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                      string.IsNullOrEmpty(webData)) && iNdx < maxCount);

            if (webData.IndexOf("There is no Options data available") > -1 ||
                webData.IndexOf("There are no results") > -1 ||
                webData.IndexOf("Get Quotes Results for ") > -1 ||
                webData.IndexOf("is no longer valid") > -1 ||
                webData.IndexOf("The remote server returned an error: (999)") > -1)
            {
                WriteLog(string.Format("No options data for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            if (string.IsNullOrEmpty(webData))
            {
                WriteLog(string.Format("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            if (webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal) == -1)
            {
                WriteLog(string.Format("Invalid data for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            WriteLog(string.Format("Symbol {0} returned options data at iNdx {1}.", sym, iNdx));

            return webData;
        }

        private static void WriteLog(string message)
        {
            using (StreamWriter log = File.AppendText(directoryPath + "/log.txt"))
            {
                log.Write(DateTime.Now.ToString() + ": " + message + "\r\n");
                log.Flush();
                log.Close();
            }
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

        private static string GetSymbolList()
        {
            string url = string.Format(
                "http://query.yahooapis.com/v1/public/yql?q={0}", "select%20*%20from%20yahoo.finance.industry%20where%20id%20in%20(select%20industry.id%20from%20yahoo.finance.sectors)&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys");
            string webData = string.Empty;

            try
            {
                using (WebClient client = new WebClient())
                {
                    webData = client.DownloadString(url);
                    webData = Regex.Replace(webData, @"[^\u0000-\u007F]", string.Empty).Replace("&", "&amp;");
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Error reading XML. Error: {0}", ex.Message));
            }

            return webData;
        }
        
        private static DataSet DeSerializationToDataSet(string data)
        {
            DataSet deSerializeDS = new DataSet();
            try
            {
                using (TextReader theReader = new StringReader(data))
                {
                    deSerializeDS.ReadXml(theReader);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Error desearializing XML. Error: {0}", ex.Message));
            }
            return deSerializeDS;
        }

        private static DataSet DeSerializationToDataSet()
        {
            DataSet deSerializeDS = new DataSet();
            try
            {
                deSerializeDS.ReadXmlSchema(@"C:\Projects\ScreenScraper\ScreenScraper\symbolslist.xml");
                deSerializeDS.ReadXml(@"C:\Projects\ScreenScraper\ScreenScraper\symbolslist.xml", XmlReadMode.IgnoreSchema);
            }
            catch (Exception ex)
            {
                // Handle Exceptions Here…..
            }
            return deSerializeDS;
        }
    }
}
