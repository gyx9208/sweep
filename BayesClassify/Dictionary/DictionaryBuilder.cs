using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using data;
using System.Net;
using System.Text.RegularExpressions;

namespace BayesClassify.Dictionary
{
    public class DictionaryBuilder
    {
        private const int TIMEOUT = 1000;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private List<string> urls, DnsSafeHosts;
        private Dictionary localDictionary;
        private double averageDivider;

        public double AverageDivider
        {
            get { return averageDivider; }
        }
        private int learnCount;

        public DictionaryBuilder()
        {
            urls = new List<string>();
            DnsSafeHosts = new List<string>();
            localDictionary = new Dictionary();
            averageDivider = 3;//read from xml later;
            learnCount = 0;
        }

        private CookieContainer cookie;
        private const string contentType = "application/x-www-form-urlencoded";
        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.116 Safari/537.36";
        private const string sCharacterRegex = @"([\u4E00-\u9FA5])+";
        private const string sUrlRegex = @"http://t\.cn/[A-Za-z0-9]+";

        Regex CharacterRegex, UrlRegex;

        public void BuildFromDatabase()
        {
            cookie = new CookieContainer();
            CharacterRegex = new Regex(sCharacterRegex);
            UrlRegex = new Regex(sUrlRegex);

            List<posts> posts;
            using (var db = new sweepEntities1())
            {
                //posts = db.posts.Where(p => p.reason.Contains("http")).Take(100).ToList();
                posts = db.posts.ToList();
            }
            posts.ForEach(analysePosts);
        }

        private void modifyAverageDivider(double d)
        {
            learnCount++;
            averageDivider = averageDivider + (d - averageDivider) / learnCount;
        }

        private void analysePosts(posts p)
        {
            analyseWords(p.text);
            if (p.reason.Length > 0)
                analyseWords(p.reason);
        }

        private void analyseWords(string origin)
        {
            //analyse url
            /*
            var urlIterator = UrlRegex.Matches(origin).GetEnumerator();
            while (urlIterator.MoveNext())
            {
                string url = urlIterator.Current.ToString();
                saveUrl(url);
            }
            */

            //analyse words
            string onlyCharacters = UrlRegex.Replace(origin, "");
            var charactersIterator = CharacterRegex.Matches(onlyCharacters).GetEnumerator();
            while (charactersIterator.MoveNext())
            {
                string words = charactersIterator.Current.ToString();
                saveWords(words);
            }
        }


        private void saveWords(string words)
        {
            int length = words.Length;
            if(length<2)
                return;
            double expectWordsCount = length / averageDivider;
            var selectedChars = new List<Word>();

            #region select possible words
            Word[] chars = new Word[length - 1];
            int notZeroCount = 0;
            for (int i = 0; i < length - 1; i++)
            {
                chars[i] = new Word();
                chars[i].Text = words.Substring(i, 2);
                chars[i].Start = i;
                chars[i].Length = 2;
                chars[i].Weight = localDictionary.find(chars[i].Text).Weight;
                if (chars[i].Weight > 0)
                    notZeroCount++;
            }

            /*
            if (notZeroCount > expectWordsCount)
            {
                var orderedChars = chars.OrderByDescending(t => t.Count).ToArray();
                var selectedPosition=new int[length];
                for (int i = 0; i < length - 1; i++)
                {
                    bool conflict = false;
                    for (int j = orderedChars[i].Start; j < orderedChars[i].Start + orderedChars[i].Length; j++)
                    {
                        if (selectedPosition[j] == 1)
                        {
                            conflict = true;
                            break;
                        }
                    }
                    if (!conflict)
                    {
                        if (orderedChars[i].Count > 0)
                        {
                            for (int j = orderedChars[i].Start; j < orderedChars[i].Start + orderedChars[i].Length; j++)
                            {
                                selectedPosition[j] = 1;
                            }
                        }
                        selectedChars.Add(orderedChars[i]);
                    }
                }
            }
            else
            {
                selectedChars.AddRange(chars);
            }
             * */
            var orderedChars = chars.OrderByDescending(t => t.Weight).ToArray();
            double maxWeight=orderedChars[0].Weight;
            int index=0;
            while (selectedChars.Count < expectWordsCount)
            {
                var topLevel = new List<Word>();
                double minWeight = (maxWeight / 2);
                for (; index < orderedChars.Length && orderedChars[index].Weight >= minWeight; index++)
                {
                    var thisSelect = orderedChars[index];
                    thisSelect.Weight = thisSelect.Length;
                    for (int j = 0; j < index; j++)
                    {
                        int start = (thisSelect.Start < orderedChars[j].Start) ? thisSelect.Start : orderedChars[j].Start;
                        int end = ((thisSelect.Start + thisSelect.Length) > (orderedChars[j].Start + orderedChars[j].Length)) ? thisSelect.Start + thisSelect.Length : orderedChars[j].Start + orderedChars[j].Length;
                        int totalLength = end - start;
                        double conflictLength = totalLength - Math.Abs(thisSelect.Start - orderedChars[j].Start) - Math.Abs(thisSelect.Start + thisSelect.Length - orderedChars[j].Start - orderedChars[j].Length);
                        if (conflictLength > 0)
                        {
                            thisSelect.Weight -= conflictLength / 2;
                            orderedChars[j].Weight -= conflictLength / 2;
                        }
                    }
                    selectedChars.Add(thisSelect);
                }
                if (index < orderedChars.Length)
                    maxWeight = orderedChars[index].Weight;
                else
                    break;
            }
            #endregion
            double wordCount = 0;
            foreach (Word w in selectedChars)
            {
                wordCount += w.Weight / w.Length;
            }
            modifyAverageDivider(wordCount);
            addToDirectory(selectedChars);
        }

        private void addToDirectory(List<Word> selectedChars)
        {
            foreach (Word w in selectedChars)
            {
                localDictionary.addCount(w.Text, w.Weight);
            }
        }

        private void saveUrl(string url)
        {
            urls.Add(url);
            DateTime startTime=System.DateTime.Now, endTime;
            int count = 0;
            while (count < 3)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = TIMEOUT;
                    request.UserAgent = userAgent;
                    request.ContentType = contentType;
                    request.CookieContainer = cookie;
                    request.Accept = accept;
                    request.Method = "get";

                    WebResponse response = request.GetResponse();
                    addToDnsSafeHosts(response.ResponseUri.DnsSafeHost);
                    endTime = System.DateTime.Now;
                    break;
                }
                catch (WebException ex)
                {
                    endTime = System.DateTime.Now;
                    count++;
                    if(count==3)
                        log.Info(url + "第" + count + "次" + (endTime - startTime).TotalSeconds + "秒",ex);
                }
            }
            
        }

        private void addToDnsSafeHosts(string p)
        {
            bool duplicate = false;
            foreach (string s in DnsSafeHosts)
            {
                if (s.Equals(p))
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
                DnsSafeHosts.Add(p);
        }

        public Dictionary getDictionary()
        {
            return localDictionary;
        }

        public List<string> getDnsSafeHosts()
        {
            return DnsSafeHosts;
        }

        public List<string> getUrls()
        {
            return urls;
        }
    }
}
