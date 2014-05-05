using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayesClassify.Dictionary
{
    class Word
    {
        string text;
        int start, length;
        double weight;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        public int Start
        {
            get { return start; }
            set { start = value; }
        }

        public double Weight
        {
            get { return weight; }
            set { weight = value; }
        }
        
    }
}
