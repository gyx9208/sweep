using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace data.Model
{
    public class MSinaAccount
    {
        public int id { get; set; }
        public string uid { get; set; }
        public int isWaste { get; set; }
        public string name { get; set; }
        public int followers_count { get; set; }
        public int friends_count { get; set; }
        public int statuses_count { get; set; }
        public int favourites_count { get; set; }

        public List<MPost> posts { get; set; }
    }
}
