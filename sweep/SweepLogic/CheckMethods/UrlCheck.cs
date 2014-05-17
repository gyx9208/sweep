using data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BayesClassify.BayesLearning;

namespace sweep.SweepLogic.CheckMethods
{
    public class UrlCheck
    {
        private static UrlList urlList=null;

        public const double FLAG = 0.8;

        public static bool Check(List<posts> resultList)
        {
            if (resultList.Count == 0)
                return false;
            if (urlList == null)
            {
                urlList = new UrlList();
                urlList.BuildFromFile(System.AppDomain.CurrentDomain.BaseDirectory + "/../LearningData/BayesClassify/Hosts.txt");
            }

            int count = 0;
            int total = 0;
            for (int i = 0; i < resultList.Count; i++)
            {
                urlList.analyse(resultList[i].text, resultList[i].reason, ref count, ref total);
            }

            if (count > (double)total * FLAG && total > (double)resultList.Count * FLAG)
                return true;
            else
                return false;
        }
    }
}