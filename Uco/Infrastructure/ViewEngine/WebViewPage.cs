using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.HtmlHelpers;
using Uco.Infrastructure.Repositories;
using Uco.Models;

namespace Uco.Infrastructure.ViewEngine
{
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
        private static ScriptHelper _scriptHelper
        {
            get
            {
                return System.Web.HttpContext.Current.Items["ScriptHelper"] as ScriptHelper;
            }
            set
            {
                System.Web.HttpContext.Current.Items["ScriptHelper"] = value;
            }
        }

        private static StyleHelper _styleHelper
        {
            get
            {
                return System.Web.HttpContext.Current.Items["StyleHelper"] as StyleHelper;
            }
            set
            {
                System.Web.HttpContext.Current.Items["StyleHelper"] = value;
            }
        }  
        public override void InitHelpers()
        {
            base.InitHelpers();
            if (_scriptHelper == null)
            {
                _scriptHelper = new ScriptHelper();
                _scriptHelper.Init();
            }
           
            if (_styleHelper == null)
            {
                _styleHelper = new StyleHelper();
                _styleHelper.Init();
            }
           
          
        }
        public ScriptHelper Script
        {
            get { 
                return _scriptHelper; 
            }
        }
        public StyleHelper Style
        {
            get
            {
                return _styleHelper;
            }
        }
        public IHtmlString T(string text)
        {
            return RP.T(text);
        }
        /// <summary>
        /// Localization Model Fields
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string M(string model,string text)
        {
            return RP.M(model, text);
        }
    }
    public abstract class WebViewPage : WebViewPage<dynamic>
    {
    }
}