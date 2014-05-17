using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace sweep.Models
{
    public class AppDefault
    {
        public static readonly string AppKey = ConfigurationManager.AppSettings["AppKey"];
        public static readonly string AppSecret = ConfigurationManager.AppSettings["AppSecret"];
        public static readonly string CallbackUrl = ConfigurationManager.AppSettings["CallbackUrl"];
    }
}