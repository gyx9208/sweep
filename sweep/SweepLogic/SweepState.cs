using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using NetDimension.Weibo;
using sweep.Models;
using data;
using sweep.SweepLogic.CheckMethods;

namespace sweep.SweepLogic
{
    public class SweepState
    {
        public NetDimension.Weibo.Entities.user.Entity user;
        public int quickMode;
        public int selectTarget;
        public int includeUrl;

        public int finishCount;
        public int realTotalCount;
        public int totalCount;
        public string state;
        public int stateId;

        public List<NetDimension.Weibo.Entities.user.Entity> wasteList;
        private List<NetDimension.Weibo.Entities.user.Entity> targetList;

        private object SyncObj = new object();
        private string accessToken;

        private static readonly string AppKey = AppDefault.AppKey;
        private static readonly string AppSecret = AppDefault.AppSecret;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SweepState(string accessToken, NetDimension.Weibo.Entities.user.Entity user, int quick = 1, int target = 0, int url = 0)
        {
            this.accessToken = accessToken;
            this.user = user;
            quickMode = quick;
            selectTarget = target;
            includeUrl = url;

            stateId = 1;
            state = "正在初始化...";
            finishCount = 0;

            if (target == 0)
            {
                totalCount = user.FollowersCount;
            }
            else if (target == 1)
            {
                totalCount = user.FriendsCount;
            }
            realTotalCount = totalCount;
            wasteList = new List<NetDimension.Weibo.Entities.user.Entity>();
            targetList = new List<NetDimension.Weibo.Entities.user.Entity>();
        }

        private Thread thread;

        public void Start()
        {
            thread = new Thread(new ThreadStart(this.SweepRun));
            thread.Start();
        }

        private void SweepRun()
        {
            try
            {
                OAuth o = new OAuth(AppKey, AppSecret, accessToken, "");
                TokenResult result = o.VerifierAccessToken();
                if (result == TokenResult.Success)
                {
                    Client Sina = new Client(o);
                    string uid = user.ID;
                    if (selectTarget == 0)
                    {
                        var list = Sina.API.Entity.Friendships.Followers(uid);
                        targetList.AddRange(list.Users);
                        while (list.NextCursor != "0")
                        {
                            int nextCursor = int.Parse(list.NextCursor);
                            list = Sina.API.Entity.Friendships.Followers(uid, "", 50, nextCursor);
                            targetList.AddRange(list.Users);
                        }
                    }
                    else
                    {
                        var list = Sina.API.Entity.Friendships.Friends(uid);
                        targetList.AddRange(list.Users);
                        while (list.NextCursor != "0")
                        {
                            int nextCursor = int.Parse(list.NextCursor);
                            list = Sina.API.Entity.Friendships.Friends(uid, "", 50, nextCursor);
                            targetList.AddRange(list.Users);
                        }
                    }

                    lock (SyncObj)
                    {
                        realTotalCount = targetList.Count;
                    }

                    int roll = 3;
                    while (roll > 0 || targetList.Count > 0)
                    {
                        roll--;
                        for (int i = targetList.Count - 1; i >= 0; i--)
                        {
                            var current = targetList[i];
                            bool b1 = QuickCheck.Check(current);//in waste Database

                            if (quickMode == 1 || b1)
                            {
                                handleResult(targetList, i, b1);
                                continue;
                            }
                            HtmlDownloader down = new HtmlDownloader(targetList[i].ID);

                            if (down.resultState == HtmlDownloader.RESULTSTATE.TIMEOUT)  //3次机会
                                continue;

                            bool b2 = false;
                            if (down.resultState == HtmlDownloader.RESULTSTATE.SUCCESS)
                            {
                                b2 = BayesCheck.Check(down.resultList);
                            }
                            if (includeUrl == 0 || b2)
                            {
                                handleResult(targetList, i, b2);
                                continue;
                            }

                            bool b3 = false;
                            if (down.resultState == HtmlDownloader.RESULTSTATE.SUCCESS)
                            {
                                b3 = UrlCheck.Check(down.resultList);
                            }

                            handleResult(targetList, i, b3);


                        }
                    }

                    finishCount = realTotalCount;
                    stateId = 2;
                    state = "扫描完成";
                }
                else
                {
                    stateId = 2;
                    state = "服务器网络问题，请稍后再试";
                }
            }
            catch (ThreadAbortException e)
            {

            }
            catch (Exception ex)
            {
                log.Error("", ex);
                string dir = System.AppDomain.CurrentDomain.BaseDirectory;
                string[] files = System.IO.Directory.GetFiles(dir);
                foreach(string ss in files){
                    state = state + ss+"|";
                }
                state = state + "-------";
                files = System.IO.Directory.GetFiles(dir+"../");
                foreach (string ss in files)
                {
                    state = state + ss + "|";
                }
            }
            Thread.Sleep(10000);
            SweepCommon sc = new SweepCommon();
            sc.RemoveStatus(user.ID);
        }

        private void handleResult(List<NetDimension.Weibo.Entities.user.Entity> targetList, int i, bool b1)
        {
            lock (SyncObj)
            {
                finishCount++;
                int remain = realTotalCount - finishCount;
                int remainTime = (int)(remain * 0.9);
                string timefix;
                if (remainTime < 60)
                    timefix = "，预计剩余" + remainTime + "秒";
                else
                {
                    timefix = "，预计剩余" + (int)(remainTime / 60) + "分" + (remainTime % 60) + "秒";
                }
                state = "正在扫描 -> " + targetList[i].Name + timefix;
                if (b1)
                {
                    wasteList.Add(targetList[i]);

                }
                targetList.RemoveAt(i);
            }
        }

        public void End()
        {
            thread.Abort();
        }
    }
}