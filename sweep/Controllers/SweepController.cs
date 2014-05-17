using sweep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sweep.SweepLogic;
using System.Threading;


namespace sweep.Controllers
{
    public class SweepController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //
        // GET: /Sweep/

        public ActionResult Index()
        {
            string accessToken = (Request.Cookies["AccessToken"] != null) ? Request.Cookies["AccessToken"].Value : null;
            object u = (Session["User"] != null) ? Session["User"] : null;
            if (accessToken != null && u != null)
            {
                var user = (NetDimension.Weibo.Entities.user.Entity)u;
                ViewBag.Name = user.Name;
                ViewBag.Url = "http://weibo.com/" + user.ID;
                ViewBag.FriendsCount = user.FriendsCount;
                ViewBag.FollowersCount = user.FollowersCount;
                ViewBag.StatusesCount = user.StatusesCount;
                ViewBag.Location = user.Location;
                ViewBag.Description = user.Description;
                ViewBag.ImgUrl = user.AvatarLarge;

                SweepCommon sc = new SweepCommon();
                var state = sc.GetStatus(user.ID);
                ViewBag.StateId = 0;
                ViewBag.Quick = 1;
                ViewBag.Target = 0;
                ViewBag.IncludeUrl = 1;
                if (state != null)
                {
                    if (state.stateId == 2)
                    {
                        state.End();
                        sc.RemoveStatus(user.ID);
                    }
                    else
                    {
                        ViewBag.Quick = state.quickMode;
                        ViewBag.Target = state.selectTarget;
                        ViewBag.IncludeUrl = state.includeUrl;
                        ViewBag.StateId = state.stateId;
                        ViewBag.State = state.state;
                        double p = (double)state.finishCount / state.realTotalCount;
                        ViewBag.Percent = p.ToString("0.0");
                    }
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult StartSweep()
        {
            var res = new JsonResult();
            var quickMode = Request.Form["QuickMode"];
            var selectTarget = Request.Form["SelectTarget"];
            object u = (Session["User"] != null) ? Session["User"] : null;
            string accessToken = (Request.Cookies["AccessToken"] != null) ? Request.Cookies["AccessToken"].Value : null;

            if (selectTarget != null && quickMode != null && u != null && accessToken != null)
            {
                SweepCommon sc = new SweepCommon();
                var user = (NetDimension.Weibo.Entities.user.Entity)u;
                var state = sc.GetStatus(user.ID);
                if (state != null)
                {
                    if (state.stateId == 2)
                    {
                        state.End();
                        sc.RemoveStatus(user.ID);
                    }
                    else
                    {
                        RedirectToAction("Index", "Sweep");
                    }
                }

                int p1 = int.Parse(quickMode);
                int p2 = int.Parse(selectTarget);
                var includeUrl = Request.Form["IncludeUrl"];
                int p3 = 1;
                if (includeUrl != null)
                    p3 = int.Parse(includeUrl);

                state = new SweepState(accessToken, user, p1, p2, p3);
                sc.AddStatus(user.ID, state);
                state.Start();

                res.Data = new { result = 1 };
            }
            else
            {
                res.Data = new { result = 0 };
            }
            return res;
        }

        public JsonResult EndSweep()
        {
            var res = new JsonResult();
            object u = (Session["User"] != null) ? Session["User"] : null;
            if (u != null)
            {
                SweepCommon sc = new SweepCommon();
                var user = (NetDimension.Weibo.Entities.user.Entity)u;
                var state = sc.GetStatus(user.ID);
                if (state == null)
                {
                    res.Data = new { result = 1 };
                }
                else
                {
                    state.End();
                    sc.RemoveStatus(user.ID);
                }

                res.Data = new { result = 1 };
            }
            else
            {
                res.Data = new { result = 0 };
            }
            return res;
        }

        public JsonResult GetSweepState()
        {
            JsonResult res = new JsonResult();
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            object u = (Session["User"] != null) ? Session["User"] : null;
            if (u != null)
            {
                var user = (NetDimension.Weibo.Entities.user.Entity)u;
                SweepCommon sc = new SweepCommon();
                var state = sc.GetStatus(user.ID);
                if (state != null)
                {
                    res.Data = new { finished = state.finishCount, total = state.realTotalCount, stateid = state.stateId, state = state.state };
                }
                else
                {
                    res.Data = new { stateId = 0, state = "已过时或服务器错误或没有正在进行的扫描" };
                }
            }
            return res;
        }

        public JsonResult GetSweepResult()
        {
            JsonResult res = new JsonResult();
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            object u = (Session["User"] != null) ? Session["User"] : null;
            if (u != null)
            {
                var user = (NetDimension.Weibo.Entities.user.Entity)u;
                SweepCommon sc = new SweepCommon();
                var state = sc.GetStatus(user.ID);

                if (state != null && state.stateId == 2)
                {
                    object[] waste = new object[state.wasteList.Count];
                    for (int i = 0; i < state.wasteList.Count; i++)
                    {
                        waste[i] = new
                        {
                            id = state.wasteList[i].ID,
                            name = state.wasteList[i].Name,
                            imgurl = state.wasteList[i].AvatarLarge,
                            url = "http://weibo.com/" + state.wasteList[i].ID
                        };
                    }
                    res.Data = new
                    {
                        result = 0,
                        target = state.selectTarget,
                        list = waste,
                        wasteCount = waste.Length,
                        realCount = state.realTotalCount,
                        totalCount = state.totalCount
                    };
                }
                else
                {
                    res.Data = new { result = 1 };
                }
            }
            else
            {
                res.Data = new { result = 1 };
            }
            return res;
        }

        public EmptyResult DeleteFans()
        {
            var length = int.Parse(Request.Form["length"]);
            string[] list = new string[length];
            for (int i = 0; i < length; i++)
            {
                list[i] = Request.Form[i];
            }
            string accessToken = (Request.Cookies["AccessToken"] != null) ? Request.Cookies["AccessToken"].Value : null;
            if (accessToken != null)
            {
                CleanThread ct = new CleanThread(accessToken,list);
                Thread t = new Thread(ct.CleanFriends);
                t.Start();
            }
            return new EmptyResult();
        }

    }
}