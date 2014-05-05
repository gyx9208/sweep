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

        public double Weight
        {
            get { return weight; }
        }

        public Dictionary(char w='\0')
        {
            this.word = w;
            nextWords = new List<Dictionary>();
            weight = 0;
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

        internal void addCount(string p, double weight)
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

    }
}
