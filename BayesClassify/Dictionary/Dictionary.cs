using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayesClassify.Dictionary
{
    public class Dictionary
    {
        internal char word;
        internal List<Dictionary> nextWords;
        private double weight;
        internal int totalCount, wasteCount;
        internal double wastePercent;

        public double Weight
        {
            get { return weight; }
        }

        public Dictionary(char w='\0')
        {
            this.word = w;
            nextWords = new List<Dictionary>();
            weight = 0;
            totalCount = 0;
            wasteCount = 0;
        }

        public Dictionary find(string p)
        {
            int length = p.Length;
            Dictionary result = this;
            for (int i = 0; i < length; i++)
            {
                char w = p[i];
                bool find = false;
                foreach (Dictionary d in result.nextWords)
                {
                    if (w.Equals(d.word))
                    {
                        result = d;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    result = new Dictionary();
                    break;
                }
            }
            return result;
        }

        internal void addWeight(string p, double weight)
        {
            int length = p.Length;
            Dictionary result = this;
            for (int i = 0; i < length; i++)
            {
                char w = p[i];
                bool find = false;
                foreach (Dictionary d in result.nextWords)
                {
                    if (w.Equals(d.word))
                    {
                        result = d;
                        result.weight += weight;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    var dw = new Dictionary(w);
                    result.nextWords.Add(dw);
                    result = dw;
                    result.weight += weight;
                }
            }
        }

        internal void addFinalWeight(string p, double weight)
        {
            int length = p.Length;
            Dictionary result = this;
            for (int i = 0; i < length; i++)
            {
                char w = p[i];
                bool find = false;
                foreach (Dictionary d in result.nextWords)
                {
                    if (w.Equals(d.word))
                    {
                        result = d;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    var dw = new Dictionary(w);
                    result.nextWords.Add(dw);
                    result = dw;
                }
                if (i == length - 1)
                    result.weight += weight;
            }
        }


        internal void addFinalCount(string p, int addValue)
        {
            int length = p.Length;
            Dictionary result = this;
            for (int i = 0; i < length; i++)
            {
                char w = p[i];
                bool find = false;
                foreach (Dictionary d in result.nextWords)
                {
                    if (w.Equals(d.word))
                    {
                        result = d;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    var dw = new Dictionary(w);
                    result.nextWords.Add(dw);
                    result = dw;
                }
                if (i == length - 1)
                {
                    result.wasteCount += addValue;
                    result.totalCount++;
                }
            }
        }
    }
}
