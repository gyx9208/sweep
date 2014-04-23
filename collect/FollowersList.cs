using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetDimension.Weibo;

namespace collect
{
    public class FollowersList : CollectList
    {
        static string AppKey = SinaApp.Default.AppKey;
        static string AppSecret = SinaApp.Default.AppSecrect;

        public FollowersList(string AccessToken)
        {
            this.list = new List<data.sinaaccounts>();
            string accessToken = AccessToken;
            OAuth oauth = new OAuth(AppKey, AppSecret, accessToken, "");
            Client Sina = new Client(oauth);
            try
            {
                string uid = Sina.API.Entity.Account.GetUID();
                var list = Sina.API.Entity.Friendships.Followers(uid);
                addToList(list);
                while (list.NextCursor != "0")
                {
                    int nextCursor = int.Parse(list.NextCursor);
                    list = Sina.API.Entity.Friendships.Followers(uid, "", 50, nextCursor);
                    addToList(list);
                }

            }
            catch (WeiboException ex)
            {
                log.Error("followerslist ex", ex);
            }
        }
    }
}
