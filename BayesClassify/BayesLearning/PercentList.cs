using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BayesClassify.Dictionary;
using data;
using data.Model;
using System.Text.RegularExpressions;
using System.IO;

namespace BayesClassify.BayesLearning
{
    public class PercentList
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary.Dictionary localDictionary;
        private List<MSinaAccount> accounts;
        List<BayesItem> BayesList;

        private const string sCharacterRegex = @"([\u4E00-\u9FA5])+";
        private const string sUrlRegex = @"http://t\.cn/[A-Za-z0-9]+";
        Regex CharacterRegex, UrlRegex;

        internal int totalCount;
        internal int wasteCount;
        internal int normalCount;

        int resultLength;
        private double averageDivider;

        public PercentList()
        {
            this.averageDivider = 3;
            BayesList = new List<BayesItem>();
            localDictionary = new Dictionary.Dictionary();
            CharacterRegex = new Regex(sCharacterRegex);
            UrlRegex = new Regex(sUrlRegex);
        }

        public PercentList(Dictionary.Dictionary d, double averageDivider)
        {
            localDictionary = d;
            this.averageDivider = 3;//averageDivider;
            totalCount = 0;
            BayesList = new List<BayesItem>();
            CharacterRegex = new Regex(sCharacterRegex);
            UrlRegex = new Regex(sUrlRegex);
        }

        internal void buildFromDatabase(int l)
        {
            resultLength = l;

            accounts = new List<MSinaAccount>();
            using (var db = new sweepEntities1())
            {
                var acs = db.sinaaccounts
                    .Where(ac => ac.isWaste != 0).ToList();
                foreach (var ac in acs)
                {
                    accounts.Add(new MSinaAccount
                    {
                        id = ac.id,
                        uid = ac.uid,
                        isWaste = ac.isWaste,
                        name = ac.name,
                        followers_count = ac.followers_count,
                        friends_count = ac.friends_count,
                        statuses_count = ac.statuses_count,
                        favourites_count = ac.favourites_count,
                        posts = MPost.ConvertFromPost(ac.posts.ToList())
                    });
                }
            }
            accounts.ForEach(calculateAccount);

            calculatePercent(localDictionary, "");
        }

        private void calculatePercent(Dictionary.Dictionary dictionary, string p)
        {
            string nextP = dictionary.word == '\0' ? "" : dictionary.word.ToString();
            for (int i = 0; i < dictionary.nextWords.Count; i++)
            {
                var d = dictionary.nextWords[i];
                d.wastePercent = (double)d.wasteCount / wasteCount;
                addIfLargeEnough(d, p + nextP);
                calculatePercent(d, p + nextP);
            }
        }

        private void addIfLargeEnough(Dictionary.Dictionary d,string p)
        {
            int count = BayesList.Count < resultLength ? BayesList.Count : resultLength;
            int index;
            for (index = count; index > 0; index--)
            {
                if (d.wastePercent < BayesList[index - 1].WastePercent)
                    break;
            }
            if (index < resultLength)
            {
                BayesItem b = new BayesItem()
                {
                    Word = p + d.word,
                    WastePercent = d.wastePercent,
                    NormalPercent = (double)(d.totalCount - d.wasteCount) / normalCount,
                    WasteCount = d.wasteCount,
                    NormalCount = d.totalCount - d.wasteCount
                };
                BayesList.Insert(index, b);
                if (BayesList.Count == resultLength + 1)
                    BayesList.RemoveAt(resultLength);
            }
        }

        private void calculateAccount(MSinaAccount account)
        {
            int addValue = account.isWaste == 2 ? 1 : 0;
            foreach (MPost p in account.posts)
            {
                List<string> list = new List<string>();
                List<string> textList = getWords(p.text);
                List<string> reasonList = getWords(p.reason);
                list.AddRange(textList);
                list.AddRange(reasonList);

                removeDuplicate(list);

                if (list.Count > 0)
                {
                    totalCount++;
                    wasteCount += addValue;
                    normalCount += (1 - addValue);
                }

                foreach (string s in list)
                {
                    localDictionary.addFinalCount(s, addValue);
                }
            }
        }

        private void removeDuplicate(List<string> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                bool d = false;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (list[i].Equals(list[j]))
                    {
                        d = true;
                        break;
                    }
                }
                if (d)
                {
                    list.RemoveAt(i);
                }
            }
        }

        private List<string> getWords(string origin)
        {
            List<string> list = new List<string>();
            string onlyCharacters = UrlRegex.Replace(origin, "");
            var charactersIterator = CharacterRegex.Matches(onlyCharacters).GetEnumerator();
            while (charactersIterator.MoveNext())
            {
                string words = charactersIterator.Current.ToString();

                int length = words.Length;
                if (length < 2)
                    break;
                double expectWordsCount = (double)(length + 1) / averageDivider;

                Word[] chars = new Word[length - 1];
                for (int i = 0; i < length - 1; i++)
                {
                    chars[i] = new Word();
                    chars[i].Text = words.Substring(i, 2);
                    chars[i].Start = i;
                    chars[i].Length = 2;
                    chars[i].Weight = localDictionary.find(chars[i].Text).Weight;
                }

                var orderedChars = chars.OrderByDescending(t => t.Weight).ToArray();
                var selectedPosition = new int[length];

                int selectCount = 0;
                for (int i = 0; i < length - 1 && selectCount < expectWordsCount; i++)
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
                        for (int j = orderedChars[i].Start; j < orderedChars[i].Start + orderedChars[i].Length; j++)
                        {
                            selectedPosition[j] = 1;
                        }
                        list.Add(orderedChars[i].Text);
                        selectCount++;
                    }
                }
            }

            return list;
        }

        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }

        public Dictionary.Dictionary getDictionary()
        {
            return localDictionary;
        }

        public List<BayesItem> getList()
        {
            return BayesList;
        }

        public void buildFromFile(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            string line;
            line = sr.ReadLine();
            string[] parts = line.Split(',');
            totalCount = int.Parse(parts[0]);
            wasteCount = int.Parse(parts[1]);
            normalCount = totalCount - wasteCount;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                parts = line.Split(',');
                BayesItem item = new BayesItem()
                {
                    Word = parts[0],
                    WastePercent = double.Parse(parts[1]),
                    NormalPercent = double.Parse(parts[2])
                };
                item.WasteCount = (int)(wasteCount * item.WastePercent);
                item.NormalCount = (int)(normalCount * item.NormalPercent);
                BayesList.Add(item);
                localDictionary.addFinalWeight(parts[0],1);
            }
            sr.Close();
        }

        public void writeToFile(string fileName)
        {
            try
            {
                StreamWriter sw = new StreamWriter(fileName, true);
                sw.WriteLine(totalCount + "," + wasteCount);
                foreach (BayesItem b in BayesList)
                {
                    b.WastePercent = (double)b.WasteCount / this.wasteCount;
                    b.NormalPercent = (double)b.NormalCount / this.normalCount;
                    sw.WriteLine(b.Word + "," + b.WastePercent + "," + b.NormalPercent);
                }

                sw.Close();
            }
            catch (Exception)
            {
            }
        }

        private const double FLAG = 0.9;

        public int analyseAndSave(string text, string reason)
        {
            //log.Info(text + reason);

            List<string> list = new List<string>();
            int length = 0;
            List<string> textList = getPureWords(text, ref length);
            List<string> reasonList = getPureWords(reason, ref length);
            int ll = length - textList.Count * 2 - reasonList.Count * 2;
            int wl = textList.Count * 2 + reasonList.Count * 2;
            list.AddRange(textList);
            list.AddRange(reasonList);

            removeDuplicate(list);

            List<double> plist = new List<double>();
            List<BayesItem> findList = new List<BayesItem>();

            double multiP = 1, multiNP = 1;
            for (int i = 0; i < list.Count; i++)
            {
                BayesItem b = findBayesItem(list[i]);
                if (b != null)
                {
                    findList.Add(b);
                    double p = ((double)b.WasteCount) / ((double)b.WasteCount + (double)b.NormalCount);
                    plist.Add(p);
                    //log.Info(list[i] + ":" + p);
                }
                else
                {
                    //log.Warn("没这个词语。不科学：" + list[i]);
                }
            }

            for (int i = 0; i < plist.Count; i++)
            {
                multiP *= plist[i];
                multiNP *= (1 - plist[i]);
            }
            double finalP = multiP / (multiP + multiNP) * lengthMul(ll,wl);


            //log.Info("最终P=" + finalP);
            if (finalP > FLAG)
            {
                //log.Info("-----------垃圾的微博------------");
                totalCount++;
                wasteCount++;
                foreach (var b in findList)
                {
                    b.WasteCount++;
                }
                return 2;
            }
            else
            {
                //log.Info("~~~~~~~~~~经检验不是垃圾的微博~~~~~~~~~~~~");
                totalCount++;
                normalCount++;
                foreach (var b in findList)
                {
                    b.NormalCount++;
                }
                return 1;
            }
        }

        public double analyse(string text, string reason)
        {
            List<string> list = new List<string>();
            int length = 0;
            List<string> textList = getPureWords(text, ref length);
            List<string> reasonList = getPureWords(reason, ref length);
            int ll = length - textList.Count * 2 - reasonList.Count * 2;
            int wl = textList.Count * 2 + reasonList.Count * 2;
            list.AddRange(textList);
            list.AddRange(reasonList);

            removeDuplicate(list);

            List<double> plist = new List<double>();
            List<BayesItem> findList = new List<BayesItem>();

            double multiP = 1, multiNP = 1;
            for (int i = 0; i < list.Count; i++)
            {
                BayesItem b = findBayesItem(list[i]);
                if (b != null)
                {
                    findList.Add(b);
                    double p = ((double)b.WasteCount) / ((double)b.WasteCount + (double)b.NormalCount);
                    plist.Add(p);
                }
            }

            for (int i = 0; i < plist.Count; i++)
            {
                multiP *= plist[i];
                multiNP *= (1 - plist[i]);
            }
            double finalP = multiP / (multiP + multiNP) * lengthMul(ll, wl);
            return finalP;
        }


        private const double K = 0.3142857, L = 10.0;
        private double lengthMul(int ll, int wl)
        {
            double result;
            if (ll <= 0)
                result = 1;
            else if (wl <= 0)
                result = 0;
            else
            {
                double x = (double)ll / wl;
                result = 1.0 / (1 + Math.Exp(K* (x - L)));
            }
            
            return result;
        }

        private BayesItem findBayesItem(string p)
        {
            for (int i = 0; i < BayesList.Count; i++)
            {
                if (BayesList[i].Word.Equals(p))
                    return BayesList[i];
            }
            return null;
        }

        private List<string> getPureWords(string origin, ref int ll)
        {
            List<string> list = new List<string>();
            string onlyCharacters = UrlRegex.Replace(origin, "");
            var charactersIterator = CharacterRegex.Matches(onlyCharacters).GetEnumerator();
            while (charactersIterator.MoveNext())
            {
                string words = charactersIterator.Current.ToString();

                int length = words.Length;
                ll += length;
                if (length < 2)
                    break;

                for (int i = 0; i < length - 1; i++)
                {
                    var w = words.Substring(i, 2);
                    if (localDictionary.find(w).Weight > 0)
                    {
                        list.Add(w);
                    }
                }
            }
            return list;
        }

        //internal void outputTem()
        //{
        //    StreamWriter sw = new StreamWriter(@"F:\GitHub\sweep\LearningData\BayesClassify\x.txt", true);
        //    foreach (double d in tem)
        //    {
        //        sw.WriteLine(d);

        //    }
        //    sw.Close();
        //}
    }
}
