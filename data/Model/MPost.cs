using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data.Model
{
    public class MPost
    {
        public int pid { get; set; }
        public int uid { get; set; }
        public string text { get; set; }
        public string ruid { get; set; }
        public string reason { get; set; }

        public static List<MPost> ConvertFromPost(ICollection<posts> posts)
        {
            var list = new List<MPost>();
            foreach (var p in posts)
            {
                list.Add(new MPost
                {
                    pid=p.pid,
                    uid=p.uid,
                    text=p.text,
                    ruid=p.ruid,
                    reason=p.reason
                });
            }
            return list;
        }
    }
}
