using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BayesClassify.BayesLearning
{
    public class UrlList
    {
        private const string sUrlRegex = @"http://t\.cn/[A-Za-z0-9]+";
        private CookieContainer cookie;
        private const string contentType = "application/x-www-form-urlencoded";
        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.116 Safari/537.36";
       

        List<string> list;
        Regex UrlRegex;

        public UrlList()
        {
            list = new List<string>();
            UrlRegex = new Regex(sUrlRegex);
            cookie = new CookieContainer();
        }

        public void BuildFromFile(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            while (!sr.EndOfStream)
            {
                list.Add(sr.ReadLine());
            }
        }

        public void analyse(string text, string reason, ref int count,ref int total)
        {
            analyseString(text,ref count,ref total);
            analyseString(reason, ref count, ref total);
        }

        private bool analyseString(string origin, ref int count, ref int total)
        {
            var urlIterator = UrlRegex.Matches(origin).GetEnumerator();
            bool b = false;
            while (urlIterator.MoveNext())
            {
                string url = urlIterator.Current.ToString();
                total++;
                int c = 0;
                while (c < 3)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.Timeout = 1000;
                        request.UserAgent = userAgent;
                        request.ContentType = contentType;
                        request.CookieContainer = cookie;
                        request.Accept = accept;
                        request.Method = "get";

                        WebResponse response = request.GetResponse();

                        var resultHost = response.ResponseUri.DnsSafeHost;
                        if (findIfWaste(resultHost))
                            count++;
                        break;
                    }
                    catch (WebException ex)
                    {
                        c++;
                    }
                }
            }

            return b;
        }

        private bool findIfWaste(string resultHost)
        {
            bool b=false;

            for (int i = 0; i < list.Count; i++)
            {
                if (resultHost.Contains(list[i]))
                {
                    b = true;
                    break;
                }
            }

            return b;
        }


    }
}
