using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BayesClassify.Dictionary;
using BayesClassify.BayesLearning;

namespace BayesClassify
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            PercentList pl = new PercentList();
            pl.buildFromFile(@"F:\GitHub\sweep\LearningData\BayesClassify\PercentList.txt");
            var list = pl.getList();
            foreach (var item in list)
            {
                if (item.NormalPercent != 0)
                    item.Multi = item.WastePercent / item.NormalPercent;
                else
                    item.Multi = 1000;
            }
            var ordered = list.OrderByDescending(t => t.Multi).ToList();
            
            try
            {
                StreamWriter sw = new StreamWriter(@"F:\GitHub\sweep\LearningData\BayesClassify\PercentListO.txt", true);
                sw.WriteLine(pl.totalCount + "," + pl.wasteCount);
                foreach (BayesItem b in ordered)
                {
                    sw.WriteLine(b.Word + "," + b.WastePercent + "," + b.NormalPercent + "," + b.Multi);
                }
                sw.Close();
            }
            catch (Exception)
            {
            }

        }

        public static void Main4(string[] args)
        {
            //System.Console.WriteLine("Input file name");
            //string filename = System.Console.ReadLine();
            AutoLearnRecentPost alrp = new AutoLearnRecentPost(@"F:\GitHub\sweep\LearningData\BayesClassify\PercentListLearn510_47.txt",47);
            alrp.StartLearn();
            Console.ReadLine();
        }

        public static void Main2(string[] args)
        {
            log.Info("learn start");
            DateTime startTime = System.DateTime.Now;
            DictionaryBuilder db = new DictionaryBuilder();
            db.BuildFromFile(@"F:\GitHub\sweep\LearningData\BayesClassify\Dictionary_5_5_2.txt");
            PercentList pl = new PercentList(db.getDictionary(), db.AverageDivider);
            pl.buildFromDatabase(1000);

            pl.writeToFile(@"F:\GitHub\sweep\LearningData\BayesClassify\PercentList_5_10_5.txt");
            
            DateTime endTime = System.DateTime.Now;
            log.Info((endTime - startTime).TotalSeconds + "秒");
            log.Info("learn end");
        }

        private static void outputPercentList(StreamWriter sw, List<BayesItem> list)
        {
            foreach (BayesItem b in list)
            {
                sw.WriteLine(b.Word + "," + b.WastePercent + "," + b.NormalPercent );
            }
        }

        private static void outputPercentList(StreamWriter sw, Dictionary.Dictionary dictionary, string p)
        {
            string nextP = dictionary.word == '\0' ? "" : dictionary.word.ToString();
            sw.WriteLine(p + nextP + ":" + dictionary.wasteCount + "/" + dictionary.totalCount + "=" + dictionary.wastePercent);
            foreach (Dictionary.Dictionary d in dictionary.nextWords)
            {
                outputPercentList(sw, d, p + nextP);
            }
        }

        public static void Main3(string[] args)
        {
            log.Info("learn start");
            DateTime startTime = System.DateTime.Now;
            DictionaryBuilder db = new DictionaryBuilder();
            db.BuildFromDatabase();
            
            /* url
            var u = db.getDnsSafeHosts();
            try
            {
                StreamWriter sw = new StreamWriter(@"F:\GitHub\sweep\LearningData\BayesClassify\Hosts.txt",true );
                u.ForEach(delegate(string s)
                {
                    sw.WriteLine(s);
                });
                sw.Close();
            }
            catch (Exception e)
            {
                log.Error("Main",e);
            }
            */

            var dictionary = db.getDictionary();
            try
            {
                StreamWriter sw = new StreamWriter(@"F:\GitHub\sweep\LearningData\BayesClassify\Dictionary_5_5_2.txt", true);
                sw.WriteLine("AverageDivider:"+ db.AverageDivider);
                outputDictionary(sw, dictionary, "");
                
                sw.Close();
            }
            catch (Exception ex)
            {
                log.Error("Main", ex);
            }


            DateTime endTime = System.DateTime.Now;
            log.Info((endTime - startTime).TotalSeconds + "秒");
            log.Info("learn end");
        }

        private static void outputDictionary(StreamWriter sw, Dictionary.Dictionary dictionary, string p)
        {
            string nextP = dictionary.word == '\0' ? "" : dictionary.word.ToString();
            sw.WriteLine(p + nextP + ":" + dictionary.Weight);
            foreach (Dictionary.Dictionary d in dictionary.nextWords)
            {
                outputDictionary(sw, d, p + nextP);
            }
        }

    }
}
