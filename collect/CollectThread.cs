using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;

namespace collect
{
    class CollectThread
    {
        private const string matchRegex = "\"domid\":\"Pl_Core_OwnerFeed[\\s\\S]*?</script>";
        private const string matchRegex2 = "\"html\"[\\s\\S]*";
        private const string aRegex = @"<a name=[\s\S]*?</a>";
        private const string imgRegex = @"<img[^>]*?>";

        private const string detailNodeXPath = @"//div[@class='WB_detail']";
        private const string postNodeXPath = @"./div[@class='WB_text']";
        private const string reasonNodeXPath = @"./div/div/div[@class='WB_text']";
        private const string infoNodeXPath = @".//a[@node-type='feed_list_originNick']";

        private CookieContainer cookie,Vcookie;
        private const string contentType = "application/x-www-form-urlencoded";
        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.116 Safari/537.36";

        private string accessToken;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int count;

        public CollectThread(string accessToken)
        {
            this.accessToken = accessToken;
            cookie = new CookieContainer();
            cookie.Add(new Cookie("SUB", @"Ad0lbXkQSjEUYbTBSbXYQbERvNiifeT5wBhvN3lx9aFytQfauCTdUodXH2E8XmQ0ijBPwZ0dbwi2YeJ2olZHPRgKHQMvokwDRfIMYhaAZ9ChKs5brv3jNWmMTopt9HeY8A%3D%3D", "/", ".weibo.com"));
            cookie.Add(new Cookie("SUBP", @"002A2c-gVlwEm1FAWxfgXELuue1xVxBxA7v9Q5dHTeSLNfemxB7zzn6", "/", ".weibo.com"));
            cookie.Add(new Cookie("SINAGLOBAL", @"7692921457346.528.1397814768352", "/", ".weibo.com"));
            cookie.Add(new Cookie("ULV", @"1397816248009:2:2:2:3146960930898.7856.1397816247934:1397814768398", "/", ".weibo.com"));
            Vcookie = new CookieContainer();
            Vcookie.Add(new Cookie("SUB", @"AYzm%2BEi%2Bbs%2BqsrfAIPet4cSB4cRPjATjBhSoul6wsyKAIg9wpOZ2lLXoDCdxn%2FNl17qDXABt7SPaAHCAYk9XET5gd1Advn1dIGMuX1DeEjQUhK%2F0ThS%2B1rI8GQd2u%2FVXNA%3D%3D", "/", ".weibo.com"));
            Vcookie.Add(new Cookie("SUBP", @"002A2c-gVlwEm1dAWxfgXELuuu%3D", "/", ".weibo.com"));
            Vcookie.Add(new Cookie("SINAGLOBAL", @"7692921457346.528.1397814768352", "/", ".weibo.com"));
            Vcookie.Add(new Cookie("ULV", @"1398068186170:8:8:3:5035808768589.05.1398068186164:1398056174342", "/", ".weibo.com"));
            count = 0;
        }

        public void collect()
        {
            DateTime startTime=System.DateTime.Now;
            var follwersList = new FollowersList(accessToken);
            var friendsList = new FriendsList(accessToken);

            log.Info("start get list");
            follwersList.getList().ForEach(downloadPage);
            friendsList.getList().ForEach(downloadPage);

            log.Info("finish get list");

            using (var db = new sweepEntities1())
            {
                foreach (var ac in follwersList.getList())
                {
                    db.sinaaccounts.Add(ac);
                }
                foreach (var ac in friendsList.getList())
                {
                    db.sinaaccounts.Add(ac);
                }
                db.SaveChanges();
            }

            DateTime endTime = System.DateTime.Now;
            log.Info((endTime-startTime).TotalSeconds+"秒");
        }

        private const string imgReplace = @"<img[^>]*>";

        private void downloadPage(sinaaccounts account){
            string uid = account.uid;
            string url = @"http://weibo.com/u/" + uid;
            try
            {
                #region download html
                string html = downloadHtml(url, cookie);
                #endregion

                #region regex find infomation
                //match script
                Regex regex = new Regex(matchRegex);
                string html2 = regex.Match(html).ToString();
                if (html2.Length < 1)
                {
                    html = downloadHtml(url, Vcookie);
                    html2 = regex.Match(html).ToString();
                }
                //match html
                Regex regex2 = new Regex(matchRegex2);
                html2 = regex2.Match(html2).ToString();
                html2 = html2.Substring(8, html2.Length - 20);

                //clear \
                html2 = html2.Replace(@"\r", "");
                html2 = html2.Replace(@"\n", "");
                html2 = html2.Replace(@"\t", "");
                html2 = html2.Replace("\\\"", "\"");
                html2 = html2.Replace(@"\/", "/");
                html2 = html2.Replace(@"&", "&amp;");

                //delete wrong node
                Regex regex3 = new Regex(aRegex);
                html2 = regex3.Replace(html2, "");

                //delete img node
                Regex regex4 = new Regex(imgRegex);
                html2 = regex4.Replace(html2, "");
                #endregion


                #region parse xml
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(html2);

                var nodes = xml.SelectNodes(detailNodeXPath);
                foreach (XmlNode n in nodes)
                {
                    var postNode = n.SelectNodes(postNodeXPath);
                    string post = "";
                    string reason = "";
                    string rid = "";
                    if (postNode.Count > 0)
                        post = postNode[0].InnerText.Trim();
                    var reasonNode = n.SelectNodes(reasonNodeXPath);
                    if (reasonNode.Count > 0)
                    {
                        reason = reasonNode[0].InnerText.Trim();
                        var infoNode = reasonNode[0].ParentNode.SelectNodes(infoNodeXPath);
                        rid = infoNode[0].Attributes["usercard"].Value.Substring(3);
                    }

                    posts p = new posts
                    {
                        text = post,
                        reason = reason,
                        ruid = rid
                    };
                    account.posts.Add(p);
                }
                log.Info("success " + (count++));
                #endregion
            }
            catch (System.IO.IOException)
            {
                log.Error("io fail " + (count++) + ";collect user error uid：" + account.uid);
            }
            catch (WebException)
            {
                log.Error("time out " + (count++) + ";collect user error uid：" + account.uid);
            }
            catch (Exception ex)
            {
                log.Error("fail " + (count++) + ";collect user error uid：" + account.uid);
                log.Error("Exception", ex);
            }
        }

        private string downloadHtml(string url, CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = userAgent;
            request.ContentType = contentType;
            request.CookieContainer = cookie;
            request.Accept = accept;
            request.Method = "get";
            request.ReadWriteTimeout = 1000;

            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            string html = reader.ReadToEnd();
            response.Close();
            return html;
        }
    }
}
