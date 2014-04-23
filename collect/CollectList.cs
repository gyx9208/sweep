using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace collect
{
    public class CollectList
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected List<data.sinaaccounts> list;
        
        protected void addToList(NetDimension.Weibo.Entities.user.Collection list)
        {
            this.list.AddRange(
                list.Users.Select(
                    u => new data.sinaaccounts
                    {
                        uid = u.ID,
                        isWaste = 0, // 0:not decided
                        name = u.Name,
                        followers_count = u.FollowersCount,
                        friends_count = u.FriendsCount,
                        statuses_count = u.StatusesCount,
                        favourites_count = (int)u.FavouritesCount
                    }).ToList());
        }

        public List<data.sinaaccounts> getList()
        {
            return list;
        }
    }
}
