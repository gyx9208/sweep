using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayesClassify.BayesLearning
{
    public class BayesItem
    {
        string word;
        double wastePercent;
        double normalPercent;
        int wasteCount;
        int normalCount;
        double multi;

        public double Multi
        {
            get { return multi; }
            set { multi = value; }
        }

        public int NormalCount
        {
            get { return normalCount; }
            set { normalCount = value; }
        }


        public int WasteCount
        {
            get { return wasteCount; }
            set { wasteCount = value; }
        }

        public string Word
        {
            get { return word; }
            set { word = value; }
        }
        
        public double WastePercent
        {
            get { return wastePercent; }
            set { wastePercent = value; }
        }
        

        public double NormalPercent
        {
            get { return normalPercent; }
            set { normalPercent = value; }
        }

    }
}
