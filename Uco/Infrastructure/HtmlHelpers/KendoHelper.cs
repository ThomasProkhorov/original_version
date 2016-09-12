using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Uco.Infrastructure.HtmlHelpers
{

    public static class HtmlKendoHelperExtension
    {
        public static UcoHtmlFactory UcoKendo<TModel>(this HtmlHelper<TModel> helper) where TModel : class
        {
            return new UcoHtmlFactory(helper);
        }
        public static UcoHtmlFactory UcoKendo(this HtmlHelper helper)
        {
            return new UcoHtmlFactory(helper);
        }
    }
    public class UcoHtmlFactory
    {
        private HtmlHelper _helper;
        public UcoHtmlFactory(HtmlHelper helper)
        {
            _helper = helper;
        }
        public GridConfig<T> Grid<T>() where T : class
        {
            return new GridConfig<T>(_helper);
        }
    }
    public class GridConfig<T> : IHtmlString where T : class
    {
        public GridConfig(HtmlHelper helper)
        {
            _HtmlHelper = helper;
            _data = new Dictionary<string, object>();
        }
        private HtmlHelper _HtmlHelper;
        
        private Dictionary<string, object> _data;
        private UcoGridColumn<T> _Columns;
        public GridConfig<T> Column(Action<UcoGridColumn<T>> configurator)
        {
            if (_Columns==null)
            {
                _Columns = new UcoGridColumn<T>();
            }
            configurator(_Columns);
            return this;
        }
        public string GetPropertyName(Expression<Func<T, object>> action)
        {
            var expression = (MemberExpression)action.Body;
            string name = expression.Member.Name;
            return name;
        }
        public string ToHtmlString()
        {
            var Model = (T)Activator.CreateInstance(typeof(T));
            _HtmlHelper.ViewData["UcoColumns"]= _Columns.GetColumns();
            return _HtmlHelper.Partial("~/Views/Generic/_Grid.cshtml", Model).ToHtmlString();
                //"<i> Encoded </i>";
        }
    }

    public class UcoGridColumn<T> where T:class
    {
        private Dictionary<string, string> _Columns = new Dictionary<string, string>();
        public Dictionary<string, string> GetColumns()
        {
            return _Columns;
        }
        public string GetPropertyName<R>(Expression<Func<T, R>> action)
        {
           
                var expression = (MemberExpression)action.Body;
                string name = expression.Member.Name;
                _Columns[name]=expression.Expression.NodeType.ToString();
                return name;
           
        }
    }
}