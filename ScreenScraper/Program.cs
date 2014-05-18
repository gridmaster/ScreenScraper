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
using Newtonsoft.Json;
using ScreenScraper.BulkLoad;
using ScreenScraper.Models;

namespace ScreenScraper
{
    class Program
    {
        readonly static string directoryPath = string.Format("D:/Projects/Data/{0} {1}{2}{3}", "Insider",
                                     DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                                     DateTime.Now.Month.ToString("D2"), DateTime.Now.Day.ToString("D2"));
        readonly static int maxCount = 200;
        readonly static string baseUrl = "http://finance.yahoo.com/q/op?s={0}+Options";
        readonly static string baseUri = "http://finance.yahoo.com";
        readonly static string summaryUri = "http://finance.yahoo.com/q?s={0}";
        readonly static string insiderUri = "http://finance.yahoo.com/q/it?s={0}+Insider+Transactions";
        readonly static string sectorsUri = "http://tickersymbol.info/Sectors";
        readonly static string industriesUri = "http://tickersymbol.info/Industries";
        private static string BaseUri = @"http://localhost:45667"; // @"http://tickersymbol.info"; // 

        private static int totalCount = 0;

        private static void Main(string[] args)
        {
            Log.WriteLog("Starting at: " + DateTime.Now);

            string sym = string.Empty;
            string webData = string.Empty;

            System.Uri suri = new Uri(BaseUri + "/GetSectors");


            // var sectors = RetrieveAsset(suri, out sym);

            string sectors = GetWebData(BaseUri + "/GetSectors");
            // string sectorResult = System.Text.Encoding.UTF8.GetString(sectors);
            var secs = JsonConvert.DeserializeObject<Sectors>(sectors);

            string industries = GetWebData(BaseUri + "/GetIndustries").Replace("&amp;", "&");

            Industries indtries = JsonConvert.DeserializeObject<Industries>(industries);

            string result = GetSymbolList();
            
            DataSet ds = DeSerializationToDataSet(result);

            DataTable industryTable = ds.Tables["industry"];

            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (DataRow item in industryTable.Rows)
            {
                dic.Add(item["industry_id"].ToString(), item["name"].ToString().Replace("&amp;", "&"));
            }
            
            DataTable companyTable = ds.Tables["company"];
            Dictionary<string, string> bigdic = new Dictionary<string, string>();
            string actualValue;
            int dups = 1;
            foreach (DataRow item in companyTable.Rows)
            {
                string key = dic[item["industry_id"].ToString()] + ":" + item["symbol"].ToString();
                string value = item["name"].ToString();
                if (bigdic.TryGetValue(key, out actualValue))
                {
                    Log.WriteLog(string.Format("{0} item # {1} - Duplicate Key found: {2} - value: {3}", dups, bigdic.Count, key, value));
                    dups++;
                }
                else
                {
                    bigdic.Add(key, value);
                }
            }

            SymbolDetails sdList = new SymbolDetails();
            dups = 1;
            
            foreach (var ball in bigdic)
            {
                SymbolDetail sd = new SymbolDetail();

                var keys = ball.Key.Split(':');
                var value = ball.Value;

                Industry ind = indtries.FirstOrDefault(i => i.Name == keys[0]);

                if (ind == null )
                    continue;

                Sector sc = secs.FirstOrDefault(i => i.Id == ind.SectorId);

                sd.SectorId = sc.Id;
                sd.Sector = sc.Name;
                sd.Industry = keys[0];
                sd.IndustryId = ind.Id;
                sd.Symbol = keys[1];
                sd.Name = value;
                sd.Date = new DateTime(2014, 5, 16);
                sdList.Add(sd);
                dups++;
            }

            dups = 1;

            SymbolDetails bulkSymbols = new SymbolDetails();
            
            foreach (SymbolDetail symbolDetail in sdList)
            {
                SymbolDetail mf = GetActiveLinks(symbolDetail);
                bulkSymbols.Add(mf);
            }

            Log.WriteLog(string.Format("Load Bulk at: " + DateTime.Now));

            BulkLoadSymbols bls = new BulkLoadSymbols();

            var dt = bls.ConfigureDataTable();

            dt = bls.LoadDataTableWithIndustries(bulkSymbols, dt);

            bls.BulkCopy<SymbolDetails>(dt);

            //SymbolDetails bullSymbols = new SymbolDetails();

            //foreach (var symbolDetail in bulkSymbols)
            //{
            //    SymbolDetail bfd = symbolDetail;
            //    bfd.Date = DateTime.Now;
            //    bullSymbols.Add(bfd);
            //}

            //BulkLoadSymbols blx = new BulkLoadSymbols();

            //var dxt = blx.ConfigureDataTable();

            //dxt = blx.LoadDataTableWithIndustries(bullSymbols, dxt);

            //blx.BulkCopy<SymbolDetails>(dxt);

            //DataView dv = new DataView(companyTable);
            //dv.Sort = "symbol asc";

            //DataTable sortedTable = dv.ToTable();

            // GetInsider(companyTable);

            //GetOptions(companyTable);
            
            Log.WriteLog(string.Format("Ending at: " + DateTime.Now));

            Console.WriteLine("Done");

            Console.ReadKey();
        }

        public static SymbolDetail GetActiveLinks(SymbolDetail sd)
        {
            int iNdx = 0;
            string webData = string.Empty;
            SymbolDetail newSymbolDetail = sd; // new SymbolDetail();

            using (WebClient wc = new WebClient())
            {
                iNdx++;
                try
                {
                    webData = string.Empty;
                    webData = wc.DownloadString(string.Format(summaryUri, sd.Symbol));

                    string compairString = webData.ToUpper();

                    if (webData.IndexOf("There are no results for the given search term", System.StringComparison.Ordinal) > -1)
                    {
                        return newSymbolDetail;
                    }

                    int symLen = sd.Symbol.IndexOf(".", System.StringComparison.Ordinal) > -1 ? sd.Symbol.IndexOf(".", System.StringComparison.Ordinal) : sd.Symbol.Length;
                    string matchThis = string.Format("NO SUCH TICKER SYMBOL: <STRONG>{0}</STRONG>", sd.Symbol.Substring(0, symLen)).ToUpper();
                    if (compairString.IndexOf(matchThis, System.StringComparison.Ordinal) > -1)
                    {
                        return newSymbolDetail;
                    }

                    if (compairString.IndexOf(string.Format("Get Quotes Results for {0}", sd.Symbol).ToUpper(), System.StringComparison.Ordinal) > -1)
                    {
                        return newSymbolDetail;
                    }

                    sd.HasSummary = true;
                    
                    // <span class="rtq_exch"><span class="rtq_dash">-</span>HKSE  </span>
                    if ((webData.IndexOf("rtq_exch", System.StringComparison.Ordinal) > -1))
                    {
                        string span = webData.Substring(webData.IndexOf("rtq_exch", System.StringComparison.Ordinal));
                        span = span.Substring(span.IndexOf("</span>", System.StringComparison.Ordinal) + "</span>".Length);
                        span = span.Substring(0, span.IndexOf("</span>", System.StringComparison.Ordinal));
                        newSymbolDetail.ExchangeName = span.Trim();
                    }

                    if (webData.IndexOf("+Options\">Options</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Options...", sd.Name));
                    }
                    else
                    {
                        sd.HasOptions = true;
                    }
                    if (webData.IndexOf("+Historical+Prices\">Historical Prices</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Historical Prices...", sd.Name));
                    }
                    else
                    {
                        sd.HasData = true;
                    }
                    if (webData.IndexOf("Key+Statistics\">Key Statistics</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Key Statistics...", sd.Name));
                    }
                    else
                    {
                        sd.HasKeyStats = true;
                    }
                    if (webData.IndexOf("Analyst+Opinion\">Analyst Opinion</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Analyst Opinion...", sd.Name));
                    }
                    else
                    {
                        sd.HasAnalyst = true;
                    }
                    if (webData.IndexOf("Analyst+Estimates\">Analyst Estimates</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Analyst Estimates...", sd.Name));
                    }
                    else
                    {
                        sd.HasEstimates = true;
                    }
                    if (webData.IndexOf("Insider+Transactions\">Insider Transactions</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Major Holders...", sd.Name));
                    }
                    else
                    {
                        sd.HasHolders = true;
                    }

                    if (webData.IndexOf("Insider+Transactions\">Insider Transactions</a>", System.StringComparison.Ordinal) < 0)
                    {
                        Log.WriteLog(string.Format("Symbol {0} has no Insider Transactions...", sd.Name));
                    }
                    else
                    {
                        sd.HasInsider = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(string.Format("Error for Symbol {0}. Error: {1}", sd.Name, ex.Message));
                }
            }
            
            return newSymbolDetail;
        }

        private static Byte[] RetrieveAsset(Uri uri, out string contentType)
        {
            try
            {
                Byte[] bytes;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
                webRequest.KeepAlive = true;
                webRequest.ProtocolVersion = HttpVersion.Version10;
                webRequest.ServicePoint.ConnectionLimit = 24;
               // webRequest.Headers.Add("UserAgent", "Pentia; MSI");
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    contentType = webResponse.ContentType;
                    using (Stream stream = webResponse.GetResponseStream())
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            // Stream the response to a MemoryStream via the byte array buffer
                            Byte[] buffer = new Byte[0x1000];
                            Int32 bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, bytesRead);
                            }
                            bytes = memoryStream.ToArray();
                        }
                    }
                }
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve asset from '" + uri + "': " + ex.Message, ex);
            }
        }

        public static string GetWebData(string uri)
        {
            string webData = string.Empty;

            try
            {
                using (WebClient client = new WebClient())
                {
                    webData = client.DownloadString(uri);
                    webData = Regex.Replace(webData, @"[^\u0000-\u007F]", string.Empty).Replace("&", "&amp;");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(string.Format("Error reading XML. Error: {0}", ex.Message));
            }

            return webData;
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

                IList<string> insider = GetRows(stuff);

                webData = webData.Substring(stuff.Length);

                webData = webData.Substring(webData.IndexOf("<table"));
                stuff = HtmlWorks.GetText(webData, "table");

                IList<string> institution = GetRows(stuff);

                string[] purchaseArray = insider[0].Split('|');
                string[] salesArray = insider[1].Split('|');
                string[] netArray = insider[2].Split('|');
                string[] totalArray = insider[3].Split('|');
                string[] percentArray = insider[4].Split('|');

                string[] institutionNet = institution[0].Split('|');
                string[] institutionPercent = institution[1].Split('|');

                InsiderTransaction it = new InsiderTransaction
                {
                    Symbol = sym,
                    SymbolId = 0,
                    PurchasesShares = purchaseArray[1],
                    PurchasesTrans = purchaseArray[2],
                    SalesShares = salesArray[1],
                    SalesTrans = salesArray[2],
                    NetSharesPurchasedSoldShares = netArray[1],
                    NetSharesPurchasedSoldTrans = netArray[2],
                    TotalInsiderSharesHeldShares = totalArray[1],
                    TotalInsiderSharesHeldTrans = totalArray[2],
                    PercentNetSharesPurchasedSoldShares = percentArray[1],
                    PercentNetSharesPurchasedSoldTrans = percentArray[2],
                    InstitutioNetSharePurchasedSold = institutionNet[1],
                    InstitutionPercentChangeInSharesHeld = institutionPercent[1],
                    Date = DateTime.Now
                };

                try
                {
                    using (var db = new ScreenScraperContext())
                    {
                        db.InsiderTransactions.Add(it);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
 
                }

                //using (StreamWriter sw = new StreamWriter(directoryPath + fileName + ".htm"))
                //{
                //    sw.Write(webData);
                //    sw.Flush();
                //    sw.Close();
                //    WriteLog.WriteLog(string.Format("Count {0}: Symbol {1} and was saved to file.", totalCount, fileName.Substring(1)));
                //}
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
                    Log.WriteLog(string.Format("Error for Symbol {0} hit iNdx {1}. Error: {2}", sym, iNdx, ex.Message));
                }
            } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                      string.IsNullOrEmpty(webData)) && iNdx < maxCount);

            if (webData.IndexOf("There is no Insider Transactions data available") > -1 ||
                webData.IndexOf("There are no results") > -1 ||
                webData.IndexOf("Get Quotes Results for ") > -1 ||
                webData.IndexOf("is no longer valid") > -1 ||
                webData.IndexOf("The remote server returned an error: (999)") > -1)
            {
                Log.WriteLog(string.Format("No insider data for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            if (string.IsNullOrEmpty(webData))
            {
                Log.WriteLog(string.Format("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            Log.WriteLog(string.Format("Symbol {0} returned insider data at iNdx {1}.", sym, iNdx));

            return webData;
        }
        
        public static void GetOptions(DataTable companyTable)
        {
            Directory.CreateDirectory(directoryPath);
            
            Log.WriteLog("Starting at: " + DateTime.Now);
            foreach (DataRow row in companyTable.Rows)
            {
                string sym = row["symbol"].ToString();

                totalCount++;

                string webData = GetInsider(sym);

                webData = GetSummary(sym);
                if (string.IsNullOrEmpty(webData)) continue;

                webData = GetOptionsPage(sym, string.Format("http://finance.yahoo.com/q/op?s={0}+Options", sym));

                if (string.IsNullOrEmpty(webData)) continue;
                
                Dictionary<string, string> mydic = GetUris(sym, webData);

                string getSymbol = string.Empty;
                foreach (var item in mydic)
                {
                    string fileName = "/" + sym + " 20" + item.Key.Substring(item.Key.IndexOf(' ') + 1) +
                                      GetMonth(item.Key.Substring(0, 3));
                    using (StreamWriter sw = new StreamWriter(directoryPath + fileName + ".htm"))
                    {
                        webData = GetOptionsPage(fileName.Replace("/",""), item.Value);
                        if (string.IsNullOrEmpty(webData)) continue;

                        sw.Write(webData);
                        sw.Flush();
                        sw.Close();
                        Log.WriteLog(string.Format("Count {0}: Symbol {1} and was saved to file.", totalCount, fileName.Substring(1)));
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
                    Log.WriteLog(string.Format("Symbol {0} has no insider data...", sym));
                    webData = "";
                }
            }
            catch (Exception ex)
            {
                iNdx++;
                if (iNdx > maxCount)
                    Log.WriteLog(string.Format("Symbol {0} blew up fetching summary page...", sym));
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
                    Log.WriteLog(string.Format("Symbol {0} has no options...", sym));
                    webData = "";
                }
            }
            catch (Exception ex)
            {
                iNdx++;
                if (iNdx > maxCount)
                    Log.WriteLog(string.Format("Symbol {0} blew up fetching summary page...", sym));
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

        private static string GetOptionsPage(string sym, string url)
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
                    Log.WriteLog(string.Format("Error for Symbol {0} hit iNdx {1}. Error: {2}", sym, iNdx, ex.Message));
                }
            } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                      string.IsNullOrEmpty(webData)) && iNdx < maxCount);

            if (webData.IndexOf("There is no Options data available") > -1 ||
                webData.IndexOf("There are no results") > -1 ||
                webData.IndexOf("Get Quotes Results for ") > -1 ||
                webData.IndexOf("is no longer valid") > -1 ||
                webData.IndexOf("The remote server returned an error: (999)") > -1)
            {
                Log.WriteLog(string.Format("No options data for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            if (string.IsNullOrEmpty(webData))
            {
                Log.WriteLog(string.Format("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            if (webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal) == -1)
            {
                Log.WriteLog(string.Format("Invalid data for Symbol {0} hit iNdx {1}", sym, iNdx));
                webData = string.Empty;
            }

            Log.WriteLog(string.Format("Symbol {0} returned options data at iNdx {1}.", sym, iNdx));

            return webData;
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
                    if (webData.Length < 500)
                    {
                        webData = "";
                        using( StreamReader sr = new StreamReader(@"D:\Projects\ScreenScraper\ScreenScraper\symbolslist.xml") )
                        {
                            webData = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(string.Format("Error reading XML. Error: {0}", ex.Message));
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
                Log.WriteLog(string.Format("Error desearializing XML. Error: {0}", ex.Message));
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
