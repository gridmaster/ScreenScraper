﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScreenScraper
{
    class Program
    {
        readonly static string directoryPath = string.Format("C:/Projects/Data/Options {0}{1}{2}",
                                     DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                                     DateTime.Now.Month.ToString("D2"), DateTime.Now.Day.ToString("D2"));

        private static void Main(string[] args)
        {
            string summaryUri = "http://finance.yahoo.com/q?s={0}";
            string baseUri = "http://finance.yahoo.com";
            string baseUrl = "http://finance.yahoo.com/q/op?s={0}+Options";
            string sym = "IBM";
            string webData = string.Empty;
            int totalCount = 0;
            int maxCount = 200;

            string blow = GetSymbolList();


            DataSet ds = DeSerializationToDataSet();
            DataTable companyTable = ds.Tables["company"];

            Directory.CreateDirectory(directoryPath);


            WriteLog("Starting at: " + DateTime.Now);
            foreach (DataRow row in companyTable.Rows)
            {
                int iNdx = 0;
                sym = row["symbol"].ToString();

                totalCount++;

                System.Net.WebClient wc = new System.Net.WebClient();

                try
                {
                    webData = string.Empty;
                    webData =
                        wc.DownloadString(string.Format(summaryUri, sym));
                    //deselected">Options</li>
                    if(webData.IndexOf("+Options\">Options</a>") < 0)
                    {
                        WriteLog(string.Format("Symbol {0} has no options...", sym));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    iNdx++;
                    if (iNdx > maxCount)
                        WriteLog(string.Format("Symbol {0} blew up fetching summary page...", sym));
                }

                iNdx = 0;

                do
                {
                    try
                    {
                        webData = string.Empty;
                        webData =
                            wc.DownloadString(string.Format("http://finance.yahoo.com/q/op?s={0}+Options", sym));
                    }
                    catch (Exception ex)
                    {
                        iNdx++;
                        if (iNdx > maxCount)
                            WriteLog(string.Format("Symbol {0} hit iNdx {1}", sym, iNdx));
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
                    continue;
                }

                if (string.IsNullOrEmpty(webData))
                {
                    WriteLog(string.Format("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx));
                    continue;
                }

                if (webData.IndexOf("View By Expiration:", System.StringComparison.Ordinal) == -1)
                {
                    continue;
                }

                WriteLog(string.Format("Count {0}: Symbol {1} hit iNdx {2} and was saved to file.", totalCount, sym, iNdx));

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

                string getSymbol = string.Empty;
                foreach (var item in mydic)
                {
                    string fileName = "/" + sym + " 20" + item.Key.Substring(item.Key.IndexOf(' ') + 1) +
                                      GetMonth(item.Key.Substring(0, 3));
                    using (StreamWriter sw = new StreamWriter(directoryPath + fileName + ".htm"))
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
                                if (iNdx > maxCount)
                                    WriteLog(string.Format("Symbol {0} hit iNdx {1}", sym, iNdx));
                            }
                        } while (((webData.IndexOf("The remote server returned an error: (999)") > -1) ||
                                  string.IsNullOrEmpty(webData)) && iNdx < maxCount);

                        if (webData.IndexOf("There is no Options data available") > -1 ||
                            webData.IndexOf("There are no results") > -1 ||
                            webData.IndexOf("Get Quotes Results for ") > -1 ||
                            webData.IndexOf("The remote server returned an error: (999)") > -1)
                        {
                            WriteLog(string.Format("No options data for Symbol {0} hit iNdx {1}", sym, iNdx));
                            continue;
                        }

                        if (string.IsNullOrEmpty(webData))
                        {
                            WriteLog(string.Format("Empty string returned for Symbol {0} hit iNdx {1}", sym, iNdx));
                            continue;
                        }
                        WriteLog(string.Format("Saving Symbol {0} hit iNdx {1}", fileName.Replace("/",""), iNdx));

                        sw.Write(webData);
                        sw.Flush();
                        sw.Close();
                    }
                }
                mydic.Clear();


            }
            WriteLog(string.Format("Ending at: " + DateTime.Now));

            Console.WriteLine("Done");

            Console.ReadKey();
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

        //private static string Get(string uri) 
        //{
        //    WebRequest request = WebRequest.Create(uri);
        //    request.Method = "POST";
        //    request.ContentType = CoreConstants.contentType;
        //    request.Headers.Add(CoreConstants.authorizationBearer + postData.token);
        //    Stream dataStream = request.GetRequestStream();
        //    dataStream.Write(requestData, 0, requestData.Length);
        //    dataStream.Dispose();

        //    string jsonResponse = string.Empty;
        //    using (WebResponse response = request.GetResponse())
        //    {
        //        if (((HttpWebResponse)response).StatusDescription == "Created")
        //        {
        //            dataStream = response.GetResponseStream();
        //            using (StreamReader reader = new StreamReader(dataStream))
        //            {
        //                jsonResponse = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    return jsonData;
        //}


        public static void tryThis(string uri)
        {  // this works
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = "GET";
            while (true)
            {
                // Get response   
                try
                {
                    using (WebResponse response = request.GetResponse() as WebResponse)
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            byte[] buffer = new byte[response.ContentLength];
                            MemoryStream ms = new MemoryStream();
                            int bytesRead, totalBytesRead = 0;

                            do
                            {
                                bytesRead = stream.Read(buffer, 0, buffer.Length);
                                totalBytesRead += bytesRead;

                                ms.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);

                            string path = @"D:\templates\fsx.jpg";
                            if (File.Exists(path))
                                File.Delete(path);

                            var fs = new FileStream(path, FileMode.Create);
                            fs.Write(ms.ToArray(), 0, totalBytesRead);
                            fs.Flush();
                            fs.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
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
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Error reading XML. Error: {0}", ex.Message));
            }

            return webData;
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
