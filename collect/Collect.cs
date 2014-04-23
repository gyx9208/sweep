using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace collect
{
    public class Collect : ICollect
    {
        public void CollectAll(string accessToken)
        {
            CollectThread ct = new CollectThread(accessToken);
            Thread t = new Thread(ct.collect);
            t.Start();
        }
    }
}
