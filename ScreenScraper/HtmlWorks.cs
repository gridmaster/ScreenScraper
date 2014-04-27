using System;
using System.Web;
using System.Collections.Generic;

namespace ScreenScraper
{
    public class HtmlWorks
    {
        public static string MappedApplicationPath
        {
            get
            {
                string APP_PATH = System.Web.HttpContext.Current.Request.ApplicationPath.ToLower();
                if (APP_PATH == "/")      //a site
                    APP_PATH = "/";
                else if (!APP_PATH.EndsWith(@"/")) //a virtual
                    APP_PATH += @"/";

                string it = System.Web.HttpContext.Current.Server.MapPath(APP_PATH);
                if (!it.EndsWith(@"\"))
                    it += @"\";
                return it.Replace("market.strategy\\market.strategy", "market.strategy\\DataFiles");
            }
        }

        public static string GetText(string value, string tag)
        {
            var tag1 = "<" + tag;
            var tag2 = "</" + tag;
            var returnString = value.Substring(value.IndexOf(tag1) + tag1.Length);
            returnString = returnString.Substring(returnString.IndexOf('>') + 1);
            returnString = returnString.Substring(0, returnString.IndexOf(tag2));

            return returnString;
        }

        public static string GetEnclosedText(string value, string StartTag, string EndTag)
        {
            var returnString = value.Substring(value.IndexOf(StartTag) + StartTag.Length);
            returnString = returnString.Substring(0, returnString.IndexOf(EndTag));

            return returnString;
        }

        public static string GetTag(string title)
        {
            return GetTag(title, "div");
        }

        public static string GetTag(string title, string strTag)
        {
            var tag = 0;
            int i = 0;
            var tag1 = "<" + strTag;
            var tag2 = "</" + strTag;

            // get open div and matching close tag
            for (i = 0; i < title.Length; i++)
            {
                if (title[i] == '<')
                    if (title.Substring(i, tag1.Length) == tag1)
                    {
                        tag++;
                        i += 4;
                    }
                    else
                    {
                        if (title.Substring(i, tag2.Length) == tag2)
                        {
                            tag--;
                            i += 5;
                        }
                    }
                if (tag == 0) break;
            }

            return title.Substring(0, i + 1);
        }

        public static List<string> GetLinks(string value)
        {
            var mylinks = new List<string>();

            while (value.IndexOf("<a href") > -1)
            {
                var myref = value.Substring(value.IndexOf("<a href"));
                value = myref;
                mylinks.Add(myref.Substring(0, myref.IndexOf(">") + 1));
                value = value.Substring(myref.IndexOf(">"));
            };

            return mylinks;
        }

        public static Dictionary<string, string> GetCompanys(string sValue)
        {
            var mylinks = new Dictionary<string, string>();
            var key = "";
            var value = "";

            while (sValue.IndexOf("bgcolor") > -1)
            {
                value = GetText(sValue, "td");
                key = GetText(sValue, "tt");
                try
                {
                    mylinks.Add(key, value.Replace("<b>", "").Replace("</b>", ""));
                }
                catch (Exception ex)
                {
                    // duplicate key catch all...
                }

                sValue = sValue.Substring(sValue.IndexOf("bgcolor") + "bgcolor".Length);
            }

            return mylinks;
        }

        public static Dictionary<string, string> GetHRefs(string sValue)
        {
            var mylinks = new Dictionary<string, string>();
            var key = "";
            var value = "";

            if (sValue.IndexOf("Quote") > -1)
            {
                //mylinks.Add(key, value);
                return mylinks;
            }

            sValue = sValue.Replace("\n", " ").Replace("<small>", "").Replace("</small>", "");

            while (sValue.IndexOf("<a href") > -1)
            {
                sValue = sValue.Substring(sValue.IndexOf("<a href") + 3);
                value = sValue.Substring(0, sValue.IndexOf(">"));

                sValue = sValue.Substring(sValue.IndexOf(">"));
                key = sValue.Substring(0, sValue.IndexOf("<") + 1).Replace("<", "").Replace(">", "").Trim();

                mylinks.Add(key, value);
            };

            return mylinks;
        }

        public static List<string> GetColumns(string rows)
        {
            var aList = new List<string>();

            var myRow = GetTag(rows, "tr");
            myRow = myRow.Replace("<td class=", "^").Replace("<tr>", ""); ;
            var myCols = myRow.Split('^');
            foreach (var item in myCols)
            {
                if (item.Length > 5)
                {
                    var mystring = item.Substring(item.IndexOf(">") + 1);
                    while (mystring[0] == '<' || (mystring[0] == ' ' && mystring[1] == '<'))
                    {
                        mystring = mystring.Substring(mystring.IndexOf(">") + 1);
                    }

                    aList.Add(mystring.Substring(0, mystring.IndexOf("<")));
                }
            }

            return aList;
        }

        public static List<List<string>> GetRows(string rows)
        {
            var myList = new List<List<string>>();
            var aList = new List<string>();
            while (rows.Length > 0)
            {
                var myRow = GetTag(rows, "tr");
                myRow = myRow.Replace("<td class=", "^").Replace("<tr>", ""); ;
                var myCols = myRow.Split('^');
                foreach (var item in myCols)
                {
                    if (item.Length > 5)
                    {
                        var mystring = item.Substring(item.IndexOf(">") + 1);
                        while (mystring[0] == '<' || mystring[1] == '<')
                        {
                            mystring = mystring.Substring(mystring.IndexOf(">") + 1);
                        }

                        aList.Add(mystring.Substring(0, mystring.IndexOf("<")));
                    }
                }
                myList.Add(aList);

                rows = rows.Substring(rows.IndexOf("<tr"));
            }
            return myList;
        }

        public static Dictionary<string, string> GetRows(string[] rows)
        {
            // <th scope="row" width="48%">Prev Close:</th><td class="yfnc_tabledata1">4.78</td></tr>
            //><td class="yfnc_tablehead1" width="74%">Market Cap (intraday)<font size="-1"><sup>5</sup></font>:</td><td class="yfnc_tabledata1"><span id="yfs_j10_ahc">103.73M</span></td></tr>
            var mydick = new Dictionary<string, string>();
            return GetRows(rows, ref mydick);
        }

        public static Dictionary<string, string> GetRows(string[] rows, ref Dictionary<string, string> mydic)
        {
            // <th scope="row" width="48%">Prev Close:</th><td class="yfnc_tabledata1">4.78</td></tr>
            //><td class="yfnc_tablehead1" width="74%">Market Cap (intraday)<font size="-1"><sup>5</sup></font>:</td><td class="yfnc_tabledata1"><span id="yfs_j10_ahc">103.73M</span></td></tr>
            //var mydick = new Dictionary<string, string>();

            foreach (var item in rows)
            {
                if (item.Length < 1) continue;

                var strTitle = item.Substring(item.IndexOf("%\">") + 3);
                var strData = strTitle.Substring(strTitle.IndexOf("yfnc_tabledata1\">") + "yfnc_tabledata1\">".Length);
                strTitle = strTitle.Substring(0, strTitle.IndexOf("<"));

                strData = strData.Replace("<span>", "").Replace("</span>", "").Replace("<small>", "").Replace("</small>", "");
                var tempstr = "";

                while (strData.IndexOf("<span") > -1)
                {
                    if (strData.IndexOf("<span") > 0)
                    {
                        //4.16 x <span id="yfs_b60_ahc">1000</td></tr>
                        tempstr = strData.Substring(0, strData.IndexOf("<span"));
                        var spanStr = strData.Substring(strData.IndexOf("<span"), strData.IndexOf(">") - tempstr.Length + 1);
                        strData = strData.Replace(spanStr, "");
                    }
                    else
                    {
                        var spanStr = strData.Substring(strData.IndexOf("<span"), strData.IndexOf(">") + 1);
                        strData = strData.Replace(spanStr, "");
                    }
                }

                strData = strData.Substring(0, strData.IndexOf('<'));
                mydic.Add(strTitle.Replace("&amp;", "&"), strData.Replace("&amp;", "&"));
            }

            return mydic;
        }
    }
}

