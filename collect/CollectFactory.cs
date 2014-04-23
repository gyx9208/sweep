using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace collect
{
    public class CollectFactory
    {
        public static ICollect BuildCollect()
        {
            return new Collect();
        }
    }
}
