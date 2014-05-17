using data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BayesClassify.BayesLearning;

namespace sweep.SweepLogic.CheckMethods
{
    public class BayesCheck
    {
        private static PercentList percentList = null;

        public const double FLAG = 0.8;

        public static bool Check(List<posts> resultList)
        {
            if (resultList.Count == 0)
                return false;
            if (percentList == null)
            {
                percentList = new PercentList();
                percentList.buildFromFile(System.AppDomain.CurrentDomain.BaseDirectory+"/../LearningData/BayesClassify/PercentList.txt");
            }

            double[] p = new double[resultList.Count];
            int count=0;
            for (int i = 0; i < resultList.Count; i++)
            {
                p[i] = percentList.analyse(resultList[i].text, resultList[i].reason);
                if (p[i] > FLAG)
                    count += 1;
            }

            if (count > (double)resultList.Count * FLAG)
                return true;
            else
                return false;
        }
    }
}