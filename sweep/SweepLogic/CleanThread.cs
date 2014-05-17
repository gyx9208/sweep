using NetDimension.Weibo;
using sweep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sweep.SweepLogic
{
    public class CleanThread
    {
        private string[] targetList;
        private static readonly string AppKey = AppDefault.AppKey;
        private static readonly string AppSecret = AppDefault.AppSecret;
        private string accessToken;

        public CleanThread(string token, string[] list)
        {
            accessToken = token;
            targetList = list;
        }

        public void CleanFriends()
        {
            try
            {
                OAuth o = new OAuth(AppKey, AppSecret, accessToken, "");
                TokenResult result = o.VerifierAccessToken();
                if (result == TokenResult.Success)
                {
                    Client Sina = new Client(o);
                    foreach (string uid in targetList)
                    {
                        Sina.API.Entity.Friendships.Destroy(uid);
                    }
                }
            }
            catch (WeiboException ex)
            {

            }
        }
    }
}