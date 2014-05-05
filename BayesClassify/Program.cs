using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BayesClassify.Dictionary;

namespace BayesClassify
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
        public static void Main(string[] args)
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
                StreamWriter sw = new StreamWriter(@"F:\GitHub\sweep\LearningData\BayesClassify\Dictionary_5_5.txt", true);
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
            sw.WriteLine(p + dictionary.word + ":" + dictionary.Weight);
            foreach (Dictionary.Dictionary d in dictionary.nextWords)
            {
                outputDictionary(sw, d, p + dictionary.word);
            }
        }

    }
}
