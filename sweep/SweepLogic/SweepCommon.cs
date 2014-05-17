using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sweep.SweepLogic
{
    public class SweepCommon
    {
        public static object SyncObj = new object();

        private static IDictionary<string, SweepState> ProcessStatus { get; set; }

        public SweepCommon()
        {
            if (ProcessStatus == null)
            {
                ProcessStatus = new Dictionary<string, SweepState>();
            }
        }

        public void AddStatus(string id,SweepState ss)
        {
            lock (SyncObj)
            {
                ProcessStatus.Add(id, ss);
            }
        }

        public void RemoveStatus(string id)
        {
            lock (SyncObj)
            {
                ProcessStatus.Remove(id);
            }
        }

        public SweepState GetStatus(string id)
        {
            lock (SyncObj)
            {
                if (ProcessStatus.Keys.Count(p => p == id) == 1)
                {
                    return ProcessStatus[id];
                }
                return null;
            }
        }
    }
}