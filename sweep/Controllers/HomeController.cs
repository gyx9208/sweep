using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NetDimension.Weibo;
using System.Configuration;
using collect;


namespace sweep.Controllers
{
    public class HomeController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(HomeController));
        private static readonly string AppKey = ConfigurationManager.AppSettings["AppKey"];
        private static readonly string AppSecret = ConfigurationManager.AppSettings["AppSecret"];
        private static readonly string CallbackUrl = ConfigurationManager.AppSettings["CallbackUrl"];

        public ActionResult Index()
        {
            string accessToken = (Request.Cookies["AccessToken"]!=null) ? Request.Cookies["AccessToken"].Value : null;
            string code = Request.QueryString["code"];
            if (accessToken != null)
            {
                goAfterOAuth(accessToken);
            }
            else if (code != null)
            {
                goAuth(code);
            }
            else
            {
                goDefaultPage();
            }
            return View();
        }

        private void goAfterOAuth(string accessToken, OAuth o = null)
        {
            if (o == null)
                o = new OAuth(AppKey, AppSecret, accessToken, "");
            TokenResult result = o.VerifierAccessToken();
            if (result == TokenResult.Success)
            {
                Client Sina = new Client(o);
                try
                {
                    string uid = Sina.API.Entity.Account.GetUID();
                    ViewBag.Name = Sina.API.Entity.Users.Show(uid).Name;
                }
                catch (WeiboException ex)
                {
                    log.Error("goAfterAuth", ex);
                }
            }
        }
                

        private void goAuth(string code)
        {
            OAuth o = new OAuth(AppKey, AppSecret, CallbackUrl);
            try
            {
                AccessToken accessToken = o.GetAccessTokenByAuthorizationCode(code);
                HttpCookie cookie=new HttpCookie("AccessToken");
                cookie.Value = accessToken.Token;
                cookie.Expires = System.DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
                goAfterOAuth(accessToken.Token, o);
            }
            catch (WeiboException ex)
            {
                log.Error("goAuth", ex);
            }
        }

        private void goDefaultPage()
        {
            
        }

        public ActionResult Login()
        {
            OAuth o = new OAuth(AppKey, AppSecret, CallbackUrl);
            string authorizeUrl = o.GetAuthorizeURL();
            return Redirect(authorizeUrl);
        }

        public ActionResult Logout()
        {
            Response.Cookies["AccessToken"].Expires = System.DateTime.Now.AddDays(-1);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Collect()
        {
            ICollect collect=CollectFactory.BuildCollect();
            collect.CollectAll(Request.Cookies["AccessToken"].Value);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
