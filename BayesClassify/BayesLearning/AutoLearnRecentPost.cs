using data;
using data.Model;
using NetDimension.Weibo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BayesClassify.BayesLearning
{
    public class AutoLearnRecentPost
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private PercentList percentList;

        Timer timerGet,timerSave;

        public AutoLearnRecentPost(string fileName,int i)
        {
            percentList = new PercentList();
            percentList.buildFromFile(fileName);
            index = i + 1;
        }

        public void StartLearn()
        {
            //analyseDatabase("");
            timerGet = new Timer(analyseRecent, null, 0, 30000);
            timerSave = new Timer(saveList, null, 600000, 600000);
        }
        static object mylock = new object();  

        int index = 0;
        private void saveList(object state)
        {
            lock (mylock)
            {
                percentList.writeToFile(@"F:\GitHub\sweep\LearningData\BayesClassify\PercentListLearn510_" + index + ".txt");
                index++;
            }
        }

        private static readonly string AppKey = "2031241233";
        private static readonly string AppSecret = "58be4648aa7511550ea8133a6227e3bf";
        private static readonly string CallbackUrl = "http://sweep.gyx.com";
        private static readonly string AccessToken = "2.00TEKaLC4ds9NCaf44fd47a4bbGdpC";

        private void analyseRecent(object state)
        {
            lock (mylock)
            {
                OAuth oauth = new OAuth(AppKey, AppSecret, AccessToken, "");
                Client Sina = new Client(oauth);
                try
                {
                    var list = Sina.API.Entity.Statuses.PublicTimeline(200);
                    foreach (var post in list.Statuses)
                    {
                        string text = post.Text;
                        string repost = "";
                        if (post.RetweetedStatus != null)
                        {
                            repost = post.RetweetedStatus.Text;
                        }
                        percentList.analyseAndSave(text, repost);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("AutoLearn Sina", ex);
                    System.Console.WriteLine(ex.ToString());
                }
            }
        }

        private void analyseDatabase(object state)
        {
            lock (mylock)
            {
                var accounts = new List<MSinaAccount>();
                using (var db = new sweepEntities1())
                {
                    var acs = db.sinaaccounts
                        .Where(ac => ac.isWaste == 1).ToList();
                    foreach (var ac in acs)
                    {
                        accounts.Add(new MSinaAccount
                        {
                            id = ac.id,
                            uid = ac.uid,
                            isWaste = ac.isWaste,
                            name = ac.name,
                            followers_count = ac.followers_count,
                            friends_count = ac.friends_count,
                            statuses_count = ac.statuses_count,
                            favourites_count = ac.favourites_count,
                            posts = MPost.ConvertFromPost(ac.posts.ToList())
                        });
                    }
                }

                foreach (var ac in accounts)
                {
                    foreach (var p in ac.posts)
                    {
                        string text = p.text;
                        string repost = p.reason;
                        percentList.analyseAndSave(text, repost);
                    }
                }
                System.Console.WriteLine(percentList.TotalCount + ":" + percentList.wasteCount);
                //percentList.outputTem();
            }
        }
    }
}
