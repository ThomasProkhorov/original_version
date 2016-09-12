using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure;
using Uco.Models;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Uco.Infrastructure.Livecycle;
using System.Collections;
using Kendo.Mvc;
using System.Linq.Dynamic;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace Uco.Models
{
    /// <summary>
    /// Generic controller, used for model from namespace Uco.Models
    /// </summary>
    /// <typeparam name="T">Entity from namespace Uco.Models</typeparam>

    public class BaseGenericController<T> : BaseController where T : class
    {
        public ActionResult Index()
        {
            var dnAttribute = GetGeneralAttribute(typeof(T));
            if (!dnAttribute.Show)
            {
                TempData["MessageRed"] = "You don't have permissions to see this page";
            }
            ViewBag.MessageRed = TempData["MessageRed"];
            ViewBag.MessageYellow = TempData["MessageYellow"];
            ViewBag.MessageGreen = TempData["MessageGreen"];
            //return Content(item.GetType().ToString());
         
            var model = (T)Activator.CreateInstance(typeof(T), null);
            var viewfolder = typeof(T).Name;
            var area = GetAreaName(RouteData);
            if(!string.IsNullOrEmpty(area))
            {
                area = "/Areas/" + area;
            }
            var viewName = "Index";
            if( Request.Browser.IsMobileDevice)
            {
                viewName = "Index.Mobile";
                var view2 = string.Format("~{2}/Views/{0}/{1}.cshtml", viewfolder, viewName, area);
                var prev2 = ControllerContext.RouteData.Values["controller"];
                ControllerContext.RouteData.Values["controller"] = viewfolder;
                var result2 = ViewEngines.Engines.FindPartialView(this.ControllerContext, view2);

                var ex2 = result2.View != null;
                ControllerContext.RouteData.Values["controller"] = prev2;
                if (ex2)
                {
                    ControllerContext.RouteData.Values["controller"] = viewfolder;
                    var viewHtml = View(view2, model);
                    // ControllerContext.RouteData.Values["controller"] = prev;
                    return viewHtml;

                }
            }
            var view = string.Format("~{2}/Views/{0}/{1}.cshtml", viewfolder, viewName, area);
            var prev = ControllerContext.RouteData.Values["controller"];
            ControllerContext.RouteData.Values["controller"] = viewfolder;
            var result = ViewEngines.Engines.FindPartialView(this.ControllerContext, view);

            var ex = result.View != null;
            ControllerContext.RouteData.Values["controller"] = prev;
            if(ex)
            {
                ControllerContext.RouteData.Values["controller"] = viewfolder;
                var viewHtml = View(view, model);
               // ControllerContext.RouteData.Values["controller"] = prev;
                return viewHtml;

            }
            return View(model );
        }

        ///// <summary>
        ///// Child action for grid with filtered parameter
        ///// </summary>
        ///// <param name="Filed">Field name</param>
        ///// <param name="ID">Field value</param>
        ///// <returns>ActionResult</returns>
        //[ChildActionOnly]
        //public ActionResult Grid(string Filed, int ID)
        //{
        //   var t = typeof(T);
        //   if (t.GetProperty(Filed) != null)
        //   {
        //       ViewBag.AdditionalFilterField = Filed;
        //       ViewBag.AdditionalFilterID = ID;
        //   }
        //    var model = (T)Activator.CreateInstance(t, null);
        //    var dnAttribute = GetGeneralAttribute(t);
            
        //    if (!dnAttribute.DependedShow)
        //    {
        //        ViewBag.TabMessageRed = "You don't have permissions to see this data";
        //    }
        //    return View(model);
        //}
        /// <summary>
        /// Child action for grid with filtered parameter
        /// </summary>
        /// <param name="Filed">Field name</param>
        /// <param name="ID">Field value</param>
        /// <returns>ActionResult</returns>
        [ChildActionOnly]
        public ActionResult Grid(string Filed, object ID)
        {
            var t = typeof(T);
            if (t.GetProperty(Filed) != null)
            {
                ViewBag.AdditionalFilterField = Filed;
                ViewBag.AdditionalFilterID = ID;
            }
            var model = (T)Activator.CreateInstance(t, null);
            var dnAttribute = GetGeneralAttribute(t);

            if (!dnAttribute.DependedShow)
            {
                ViewBag.TabMessageRed = "You don't have permissions to see this data";
            }
            return View(model);
        }
        /// <summary>
        /// Action for autocomplete combobox
        /// </summary>
        /// <param name="request">kendo datasource request</param>
        /// <param name="text">search string</param>
        /// <returns>Json {Data:[],Errors:[]}</returns>
        public ActionResult _AjaxAutoComplete([DataSourceRequest]DataSourceRequest request, string text, string keyFiled)
        {

            try
            {


                var t = typeof(T);
                var dnMainAttribute = GetGeneralAttribute(t);
                IQueryable items = null;
                if (string.IsNullOrEmpty(keyFiled))
                {
                    keyFiled = "ID";
                }
                //check Acl
                // AccessTest
                bool distinct = false;
                if (dnMainAttribute.Acl)
                {
                    MethodInfo methodList = t.GetMethod("AccessList", BindingFlags.Public | BindingFlags.Static);
                    if (methodList != null)
                    {
                        items = (IQueryable)methodList.Invoke(null, new object[] { dnMainAttribute, null });
                    }
                }
                else
                {
                    Dictionary<string, object> param = new Dictionary<string, object>();
                    if (request.Filters != null)
                    {
                        param = GetPredicate(t, param, request.Filters);
                        if (request.Filters.Count != param.Count)
                        {
                            distinct = true;
                        }
                    }
                    MethodInfo methodList = t.GetMethod("AccessList", BindingFlags.Public | BindingFlags.Static);
                    if (methodList != null)
                    {
                        items = (IQueryable)methodList.Invoke(null, new object[] { dnMainAttribute, param });
                    }
                }


                if (items == null) items = _db.Set<T>().AsQueryable();


                var properties = t.GetProperties();
                //p=>p.Name.Contains(text.ToLower()) || p=>p.Name.StartsWith(text.ToLower())
                #region predicate
                ParameterExpression entityParameter = Expression.Parameter(t, "p");
                ConstantExpression foreignKeysParameter = Expression.Constant(text.ToLower(), typeof(string));
                string SearchField = "Name";
                var deepRef = t.GetProperties().FirstOrDefault(x =>
                    //   x.PropertyType.Namespace == "Uco.Models"
                                            x.GetCustomAttributes<ModelAttribute>() != null
   && x.GetCustomAttributes<ModelAttribute>().Count() > 0
   && x.GetCustomAttributes<ModelAttribute>().FirstOrDefault().IsDropDownName);
                if (request.Filters != null)
                {
                    items = items.Where(request.Filters);
                    request.Filters = null;
                }
                if (deepRef != null)
                {

                    var addattrname = t.GetProperties().Where(x => x.GetCustomAttributes<ModelAttribute>() != null
    && x.GetCustomAttributes<ModelAttribute>().Count() > 0
    && (
    x.GetCustomAttributes<ModelAttribute>().FirstOrDefault().ShowInParentGrid

    )
    );
                    string addSelect = "";
                    foreach (var s in addattrname)
                    {
                        addSelect += " +\" \" +outer." + s.Name;
                    }
                    SearchField = deepRef.Name;
                    if (deepRef.PropertyType.Namespace == "Uco.Models")
                    {

                        var outer = items;
                        var inner = _db.Set(deepRef.PropertyType).AsQueryable();
                        LambdaExpression outerSelectorLambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(outer.ElementType, null, SearchField + "ID", outer);
                        LambdaExpression innerSelectorLambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(inner.ElementType, null, "ID", inner);

                        ParameterExpression[] parameters = new ParameterExpression[] {
            Expression.Parameter(outer.ElementType, "outer"), Expression.Parameter(inner.AsQueryable().ElementType, "inner") };
                        LambdaExpression resultsSelectorLambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameters, null, "new ( outer.ID,  inner.Name" + addSelect + " as Name )", outer);

                        items = outer.Provider.CreateQuery(
                           Expression.Call(
                               typeof(Queryable), "Join",
                               new Type[] { outer.ElementType, inner.AsQueryable().ElementType, outerSelectorLambda.Body.Type, resultsSelectorLambda.Body.Type },
                               outer.Expression, inner.AsQueryable().Expression, Expression.Quote(outerSelectorLambda), Expression.Quote(innerSelectorLambda), Expression.Quote(resultsSelectorLambda)));
                        //  var list1 = items2.Cast<T>().ToList();

                        entityParameter = Expression.Parameter(deepRef.PropertyType, "p");
                        SearchField = "Name";
                    }
                    else
                    {

                        addattrname = t.GetProperties().Where(x => x.GetCustomAttributes<ModelAttribute>() != null
  && x.GetCustomAttributes<ModelAttribute>().Count() > 0
  && (
   x.GetCustomAttributes<ModelAttribute>().FirstOrDefault().IsDropDownName
  )
  );
                        addSelect = "";
                        foreach (var s in addattrname)
                        {
                            addSelect += " +\" \" +" + s.Name;
                        }

                        var selectFields = addSelect.Remove(0, 2).Replace("outer.", "") + " as Name";
                        items = items.Select("new ( " + keyFiled + ", " + selectFields + " )");
                        entityParameter = Expression.Parameter(items.ElementType, "p");
                        SearchField = "Name";
                    }

                }

                MemberExpression memberExpression = Expression.Property(entityParameter, SearchField);
                Expression convertExpression = Expression.Convert(memberExpression, typeof(string));
                //StartsWith
                if (text.Length < 3)
                {
                    MethodCallExpression containsExpression = Expression.Call(convertExpression
                        , "StartsWith", new Type[] { }, foreignKeysParameter);

                    var ContainsPredicate = Expression.Lambda(containsExpression, entityParameter);
                    items = items.Where(SearchField + ".StartsWith(@0)", text.ToLower());
                }
                else
                {
                    MethodCallExpression containsExpression = Expression.Call(convertExpression
                           , "Contains", new Type[] { }, foreignKeysParameter);

                    var ContainsPredicate = Expression.Lambda(containsExpression, entityParameter);
                    items = items.Where(SearchField + ".Contains(@0)", text.ToLower());
                }

                #endregion
                request.PageSize = 100;
                if (distinct)
                {
                    items = items.Distinct();
                }
                DataSourceResult result = items.ToDataSourceResult(request);
                return Json(result);
            }
            catch (Exception error)
            {
                SF.LogError(error);
                return Json(error.Message + " " + error.StackTrace);
            }
        }

        protected ModelGeneralAttribute GetGeneralAttribute(Type type) {
            return LS.GetModelGeneral(type, ViewBag.CurrentRole);
        }
        protected ModelAttribute GetModelFieldAttribute(PropertyInfo p)
        {
            return p.GetCustomAttributes<ModelAttribute>().Where(x => x.Role == ViewBag.CurrentRole || x.Role == null).OrderByDescending(x => x.Role).ToList().FirstOrDefault();

        }


        protected Dictionary<string, object> GetPredicate(Type t,Dictionary<string, object> param, IList<IFilterDescriptor> filters)
        {
            List<IFilterDescriptor> forRemove = new List<IFilterDescriptor>();
            foreach (var f in filters)
            {
                if (f is CompositeFilterDescriptor)
                {
                    param = GetPredicate(t,param, (f as CompositeFilterDescriptor).FilterDescriptors);
                }
                else
                {
                    param[(f as FilterDescriptor).Member] = (f as FilterDescriptor).Value;
                    var pt = t.GetProperty((f as FilterDescriptor).Member);
                    if (pt != null)
                    {
                        if (pt.PropertyType.BaseType.Name == "Enum")
                        {
                            (f as FilterDescriptor).Value = Enum.ToObject(pt.PropertyType, Convert.ToInt32(
                                (f as FilterDescriptor).Value));
                            (f as FilterDescriptor).MemberType = pt.PropertyType;
                        }else if(pt.PropertyType.Name == "Guid")
                        {
                            (f as FilterDescriptor).Value = new Guid( (f as FilterDescriptor).Value as string);
                            (f as FilterDescriptor).MemberType = pt.PropertyType;
                            (f as FilterDescriptor).Operator = FilterOperator.IsEqualTo;
                        }
                    }
                    else
                    {
                        forRemove.Add(f);
                        
                    }
                }
            }
            foreach (var f in forRemove)
            {
                filters.Remove(f);
            }

            return param;
        }


        /// <summary>
        /// Grid read action
        /// </summary>
        /// <param name="request">Kendo datasource request</param>
        /// <returns>Json {Data:[],Errors:[]}</returns>
        [HttpPost]
        public ActionResult _AjaxRead([DataSourceRequest]DataSourceRequest request)
        {
            //AccessList
            var t = typeof(T);
            var dnMainAttribute = GetGeneralAttribute(t);
            IQueryable<T> items = null;
            //check Acl
            // AccessTest
            Dictionary<string, object> param = new Dictionary<string, object>();

            if (request.Filters != null)
            {
                param = GetPredicate(t,param, request.Filters);
               
            }

            if (request.Sorts != null)
            {
                List<Kendo.Mvc.SortDescriptor> forRemove = new List<Kendo.Mvc.SortDescriptor>();
                var sortVariants = new List<JsonKeyValue>();
               foreach(var sortDescriptor in request.Sorts)
               {
                   sortVariants.Add( new JsonKeyValue(){ Name = sortDescriptor.Member, Value = sortDescriptor.SortDirection.ToString() });
                   if(sortDescriptor.Member.Contains("."))
                   {
                       forRemove.Add(sortDescriptor);
                   }
               }
                foreach(var r in forRemove)
                {
                    request.Sorts.Remove(r);
                }
               param["_SortVariants"] = sortVariants;
            }
            if (dnMainAttribute.Acl)
            {
                MethodInfo methodList = t.GetMethod("AccessList", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    items = (IQueryable<T>)methodList.Invoke(null, new object[] { dnMainAttribute, param });
                }
            }
            else { 
                
                
                MethodInfo methodList = t.GetMethod("AccessList", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    items = (IQueryable<T>)methodList.Invoke(null, new object[] { dnMainAttribute,param });
                }
            }


            if(items==null) items = _db.Set<T>().AsQueryable();
            DataSourceResult result = items.ToDataSourceResult(request);
           
            #region load reference data
            
            var properties = t.GetProperties();

            foreach (var p in properties)
            {
                // var ModelAttribute = p.GetCustomAttributes<ModelAttribute>().Where(x => x.Role == ViewBag.CurrentRole || x.Role == null).OrderByDescending(x => x.Role).ToList().FirstOrDefault();
                var uihint = p.GetCustomAttributes<UIHintAttribute>().FirstOrDefault();
                if (uihint != null && uihint.UIHint == "Image" && properties.FirstOrDefault(x=>x.Name == "SmallGridImage") !=null) //SmallGridImage
                {
                    var gridImagePropertie = properties.FirstOrDefault(x => x.Name == "SmallGridImage");
                    foreach (var item in result.Data)
                    {
                      gridImagePropertie.SetValue(item,  SF.GetImage((string)p.GetValue(item), 80, 80, true, true));

                    }
                }
                if (p.PropertyType.Namespace == "Uco.Models")
                {
                    var opt = GetModelFieldAttribute(p);
                    if (opt != null && !opt.Show)
                    {
                        continue;
                    }

                    var key = p.Name + "ID";
                    if (opt!=null && !string.IsNullOrEmpty(opt.KeyField))
                    {
                        key = opt.KeyField;
                    }
                    var foreignKey = "ID";
                    if (opt!=null && !string.IsNullOrEmpty(opt.ForeignKey))
                    {
                        foreignKey = opt.ForeignKey;
                    }
                    var fk = properties.FirstOrDefault(x => x.Name == key);
                    if (fk != null)
                    {
                        // get list of IDs to get ref objects, OPTIMIZED
                        #region get list of ref objects

                        //Firest stage, check for cache

                        //getting entity settings
                        var dnAttribute = GetGeneralAttribute(p.PropertyType);

                        Func<object> cachedFunc = () =>
                       {
                           MethodInfo methodList = typeof(LS).GetMethod("GetAllRefList", BindingFlags.Public | BindingFlags.Static);
                           MethodInfo genericMethodList = methodList.MakeGenericMethod(p.PropertyType);
                           return genericMethodList.Invoke(null, new object[] { });
                       };
                        Func<object> NoncachedFunc = () =>
                        {
                            object foreignKeys = new List<int>();
                            var forGuids = new List<Guid>();
                            var exprType = typeof(List<int>);
                            var exprParamType = typeof(int);
                            if (fk.PropertyType.Name == "Int32")
                            {
                                foreach (var item in result.Data)
                                {
                                    var k = (int)fk.GetValue(item);
                                    if (k > 0)
                                    {
                                        (foreignKeys as List<int>).Add(k);
                                    }

                                }
                            }
                            else if (fk.PropertyType.Name == "Guid")
                            {
                                exprType = typeof(List<Guid>);
                                 exprParamType = typeof(Guid);
                                 foreignKeys = new List<Guid>();
                                foreach (var item in result.Data)
                                {
                                    var k = (Guid)fk.GetValue(item);
                                    if ( k != Guid.Empty)
                                    {
                                        (foreignKeys as List<Guid>).Add(k);
                                    }

                                }
                            }
                            else if (fk.PropertyType.Name == "String")
                            {
                                exprType = typeof(List<string>);
                                exprParamType = typeof(string);
                                foreignKeys = new List<string>();
                                foreach (var item in result.Data)
                                {
                                    var k = (string)fk.GetValue(item);
                                    if (!string.IsNullOrEmpty(k))
                                    {
                                        (foreignKeys as List<string>).Add(k);
                                    }

                                }
                            }
                            //var refObjectList = LS.GetRefList<G>(x=>foreignKeys.Contains(x.ID)) // G another from T
                            #region get ref list
                            //make predicate x=>foreignKeys.Contains(x.ID)
                            ParameterExpression entityParameter = Expression.Parameter(p.PropertyType, "p");
                            ConstantExpression foreignKeysParameter = Expression.Constant(foreignKeys, exprType);
                            MemberExpression memberExpression = Expression.Property(entityParameter, foreignKey);
                            Expression convertExpression = Expression.Convert(memberExpression, exprParamType);
                            MethodCallExpression containsExpression = Expression.Call(foreignKeysParameter
                                , "Contains", new Type[] { }, convertExpression);

                            var ContainsPredicate = Expression.Lambda(containsExpression, entityParameter);

                            //call LS.GetRefList<G>(predicate)

                            MethodInfo methodList = typeof(LS).GetMethod("GetRefList", BindingFlags.Public | BindingFlags.Static);
                            MethodInfo genericMethodList = methodList.MakeGenericMethod(p.PropertyType);
                            return genericMethodList.Invoke(null, new object[] { ContainsPredicate });
                        };
                        IList refObjectList = null;
                        if (dnAttribute.Cached)
                        {
                            //cache read
                            MethodInfo methodList = typeof(LS).GetMethod("GetCustom", BindingFlags.Public | BindingFlags.Static);
                            MethodInfo genericMethodList = methodList.MakeGenericMethod(p.PropertyType);
                            refObjectList = (IList)genericMethodList.Invoke(null, new object[] { "", cachedFunc });
                        }
                        else
                        {
                             refObjectList = (IList)NoncachedFunc();
                        }
                       
                        #endregion
                        var IDRefProperty = p.PropertyType.GetProperty(foreignKey);
                        #endregion
                        //
                        //set refferenced objects of G to his parent T 
                        foreach (var item in result.Data)
                        {
                            var k = fk.GetValue(item);
                            if (k != null)
                            {
                                //var frepository = _db.Set(p.PropertyType);
                                foreach (var li in refObjectList)
                                {
                                    var id = IDRefProperty.GetValue(li);
                                    if (id.Equals(k))
                                    {
                                        var deepRefList = li.GetType().GetProperties().Where(x =>
                                            x.PropertyType.Namespace == "Uco.Models"
                                        && x.GetCustomAttributes<ModelAttribute>() != null
&& x.GetCustomAttributes<ModelAttribute>().Count() > 0
&& ( x.GetCustomAttributes<ModelAttribute>().FirstOrDefault().IsDropDownName
|| x.GetCustomAttributes<ModelAttribute>().FirstOrDefault().ShowInParentGrid
));
                                        foreach (var deepRef in deepRefList)
                                        {
                                            if (deepRef != null)
                                            {
                                                //load sub ref
                                                var pr = li.GetType().GetProperty(deepRef.Name + "ID");
                                                if (pr != null)
                                                {
                                                    var idDeep = (int)pr.GetValue(li);
                                                    #region get by id in deep
                                                    ParameterExpression pe = Expression.Parameter(deepRef.PropertyType, "p");
                                                    Expression left = Expression.Property(pe, "ID");
                                                    Expression right = Expression.Constant(idDeep);
                                                    Expression e1 = Expression.Equal(left, right);


                                                    var gc = typeof(Func<,>);

                                                    Type Gfunct = gc.MakeGenericType(deepRef.PropertyType, typeof(bool));

                                                    var typeExpression = typeof(Expression<>);

                                                    Type Gexpression = typeExpression.MakeGenericType(Gfunct);


                                                    var predicate = Expression.Lambda(Expression.Equal(left, right),
                        new[] { pe });//.Compile();

                                                    MethodInfo method = typeof(LS).GetMethod("GetFirst", BindingFlags.Public | BindingFlags.Static);
                                                    MethodInfo genericMethod = method.MakeGenericMethod(deepRef.PropertyType);
                                                    var refObject = genericMethod.Invoke(null, new object[] { predicate });

                                                    // dynamic cExpression = Activator.CreateInstance(Gexpression, Expression.Equal(left, right),
                                                    //  new[] { pe });

                                                    //   var predicate = cExpression.Compile();
                                                    // var refObject = frepository.Cast<Product>().FirstOrDefault(x=>x.ID==k);
                                                    if (refObject != null)
                                                    {
                                                        deepRef.SetValue(li, refObject);
                                                    }
                                                    #endregion
                                                }
                                            }

                                        }
                                        var childProperties = li.GetType().GetProperties();
                                        var uihintChild1 = childProperties.Where(x =>
                                            x.GetCustomAttributes<UIHintAttribute>() != null
&& x.GetCustomAttributes<UIHintAttribute>().Count() > 0
&& (x.GetCustomAttributes<UIHintAttribute>().FirstOrDefault().UIHint == "Image"

)).FirstOrDefault();
                                        if (uihintChild1 != null && childProperties.FirstOrDefault(x => x.Name == "SmallGridImage") != null) //SmallGridImage
                                        {
                                            var gridImagePropertie = childProperties.FirstOrDefault(x => x.Name == "SmallGridImage");
                                              gridImagePropertie.SetValue(li, SF.GetImage((string)uihintChild1.GetValue(li), 80, 80, true, true));

                                            
                                        }

                                        p.SetValue(item, li);
                                        continue;
                                    }
                                }
                            }
                            continue;
                            // get ref object, Not OPTIMIZED!!!
                            #region get by id in foreach
//                            ParameterExpression pe = Expression.Parameter(p.PropertyType, "p");
//                            Expression left = Expression.Property(pe, "ID");
//                            Expression right = Expression.Constant(k);
//                            Expression e1 = Expression.Equal(left, right);


//                            var gc = typeof(Func<,>);

//                            Type Gfunct = gc.MakeGenericType(p.PropertyType, typeof(bool));

//                            var typeExpression = typeof(Expression<>);

//                            Type Gexpression = typeExpression.MakeGenericType(Gfunct);


//                            var predicate = Expression.Lambda(Expression.Equal(left, right),
//new[] { pe });//.Compile();

//                            MethodInfo method = typeof(LS).GetMethod("GetFirst", BindingFlags.Public | BindingFlags.Static);
//                            MethodInfo genericMethod = method.MakeGenericMethod(p.PropertyType);
//                            string refObject = null;// genericMethod.Invoke(null, new object[] { predicate });

//                            // dynamic cExpression = Activator.CreateInstance(Gexpression, Expression.Equal(left, right),
//                            //  new[] { pe });

//                            //   var predicate = cExpression.Compile();
//                            // var refObject = frepository.Cast<Product>().FirstOrDefault(x=>x.ID==k);
//                            if (refObject != null)
//                            {
//                                p.SetValue(item, refObject);
//                            }
                            #endregion
                        }
                    }
                }

            }
            #endregion

            return Json(result);
        }

        /// <summary>
        /// Create action
        /// </summary>
        /// <returns>Html page</returns>
        [HttpGet]
        public ActionResult Create(T item)
        {
            var t = typeof(T);
            var dnAttribute = GetGeneralAttribute(t);
            if (!dnAttribute.Create)
            {

                TempData["MessageRed"] = "You don't have permissions to create this element";
                return RedirectToAction("Index");
            }
            //
            if (Session["back_" + t.Name] == null
              && Request.UrlReferrer != null
              && Request.UrlReferrer.Host == Request.Url.Host
              && Request.UrlReferrer.PathAndQuery != Request.Url.PathAndQuery
              && !Request.UrlReferrer.PathAndQuery.Contains(string.Format("/{0}/Create", t.Name))
               && !Request.UrlReferrer.PathAndQuery.Contains(string.Format("/{0}/Edit", t.Name))

              )
            {
                Session["back_" + t.Name] = Request.UrlReferrer.PathAndQuery;
            }

            if (Session["back_" + t.Name] != null)
            {
                ViewBag.BackUrl = Session["back_" + t.Name];
            }
            var viewfolder = t.Name;
            var area = GetAreaName(RouteData);
            if (!string.IsNullOrEmpty(area))
            {
                area = "/Areas/" + area;
            }
            var view = string.Format("~{2}/Views/{0}/{1}.cshtml", viewfolder, "Create", area);
            var prev = ControllerContext.RouteData.Values["controller"];
            ControllerContext.RouteData.Values["controller"] = viewfolder;
            var result = ViewEngines.Engines.FindPartialView(this.ControllerContext, view);

            var ex = result.View != null;
            ControllerContext.RouteData.Values["controller"] = prev;
            if (ex)
            {
                ControllerContext.RouteData.Values["controller"] = viewfolder;
                var viewHtml = View(view, item);
                // ControllerContext.RouteData.Values["controller"] = prev;
                return viewHtml;

            }
            return View(item);
        }

        /// <summary>
        /// Create post
        /// </summary>
        /// <param name="item">T item</param>
        /// <returns>Html page or redirect</returns>
        [HttpPost,ActionName("Create")]
        public ActionResult CreatePost(T item)
        {
            var t = typeof(T);
            var dnAttribute = GetGeneralAttribute(t);
            if (!dnAttribute.Create)
            {

                TempData["MessageRed"] = "You don't have permissions to create this element";
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                //   item.DomainID = CurrentSettings.ID;
                var createAtrr = item.GetType().GetProperty("CreateDate");
                if (createAtrr != null)
                {
                    createAtrr.SetValue(item, DateTime.UtcNow);
                }
                var updateAtrr = item.GetType().GetProperty("UpdateDate");
                if (updateAtrr != null)
                {
                    updateAtrr.SetValue(item, DateTime.UtcNow);
                }
                MethodInfo methodCreatingList = t.GetMethod("OnCreating", BindingFlags.Public | BindingFlags.Static);
                if (methodCreatingList != null)
                {
                    methodCreatingList.Invoke(null, new object[] { item });
                }
                _db.Set<T>().Add(item);
                var errorMsg = "";
                try
                {
                    _db.SaveChanges();
                }catch(Exception genexception)
                {
                     errorMsg = genexception.Message +
                        (genexception.InnerException != null && genexception.InnerException.InnerException!=null 
                        ? " "+genexception.InnerException.InnerException.Message : "");
                    
                    
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    if (errorMsg.Contains("unique index"))
                    {
                        MethodInfo methodError = t.GetMethod("DuplicateError", BindingFlags.Public | BindingFlags.Static);
                        if (methodError != null)
                        {
                            errorMsg = (string)methodError.Invoke(null, new object[] { item });
                        }
                    }
                    else
                    {
                        MethodInfo methodError = t.GetMethod("InsertGeneralError", BindingFlags.Public | BindingFlags.Static);
                        if (methodError != null)
                        {
                            errorMsg = (string)methodError.Invoke(null, new object[] { item });
                        }
                    }
                    TempData["MessageRed"] = errorMsg;
                    return RedirectToAction("Index");
                }
                MethodInfo methodList = t.GetMethod("OnCreated", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    methodList.Invoke(null, new object[] { item });
                }
                CleanCache.CleanOutputCache();
                TempData["MessageGreen"] = "Create success";
                return RedirectToAction("Edit", new  { ID = item.GetType().GetProperty("ID").GetValue(item) });
            }
            var viewfolder = t.Name;
            var area = GetAreaName(RouteData);
            if (!string.IsNullOrEmpty(area))
            {
                area = "/Areas/" + area;
            }
            var view = string.Format("~{2}/Views/{0}/{1}.cshtml", viewfolder, "Create", area);
            var prev = ControllerContext.RouteData.Values["controller"];
            ControllerContext.RouteData.Values["controller"] = viewfolder;
            var result = ViewEngines.Engines.FindPartialView(this.ControllerContext, view);

            var ex = result.View != null;
            ControllerContext.RouteData.Values["controller"] = prev;
            if (ex)
            {
                ControllerContext.RouteData.Values["controller"] = viewfolder;
                var viewHtml = View(view, item);
                // ControllerContext.RouteData.Values["controller"] = prev;
                return viewHtml;

            }
            return View(item);
        }

        /// <summary>
        /// Edit action
        /// </summary>
        /// <param name="ID">ID of entity</param>
        /// <returns>Html page</returns>
        public ActionResult Edit(int ID)
        {
            var t = typeof(T);
            var dnAttribute = GetGeneralAttribute(t);
            if (!dnAttribute.Edit)
            {

                TempData["MessageRed"] = "You don't have permissions to edit this element";
                return RedirectToAction("Index");
            }
            var p = t.GetProperties();
            string s = "";
            foreach (var pr in p)
            {
                s = pr.Name;
            }
            // _db.Set<T>().First(x=>x.ID = Id);
            ParameterExpression pe = Expression.Parameter(t, "p");
            Expression left = Expression.Property(pe, "ID");
            Expression right = Expression.Constant(ID);
            Expression e1 = Expression.Equal(left, right);
            var predicate = Expression.Lambda<Func<T, bool>>
     (Expression.Equal(left, right),
     new[] { pe }).Compile();
            var item = _db.Set<T>().FirstOrDefault(predicate);
          if (Session["back_" + t.Name] == null 
              && Request.UrlReferrer != null
            && Request.UrlReferrer.Host == Request.Url.Host
            && Request.UrlReferrer.PathAndQuery != Request.Url.PathAndQuery
            && !Request.UrlReferrer.PathAndQuery.Contains(string.Format("/{0}/Create", t.Name))
             && !Request.UrlReferrer.PathAndQuery.Contains(string.Format("/{0}/Edit", t.Name))

            )
          {
              Session["back_" + t.Name] = Request.UrlReferrer.PathAndQuery;
          }

          if (Session["back_" + t.Name] != null)
          {
              ViewBag.BackUrl = Session["back_" + t.Name];
          }
          if (item == null) {
              TempData["MessageRed"] = string.Format( "Element with ID = {0} dosen`t exist",ID);
              if (ViewBag.BackUrl != null)
                  return Redirect(ViewBag.BackUrl);
              else
                  return RedirectToAction("Index");
          }
          ViewBag.MessageRed = TempData["MessageRed"];
          ViewBag.MessageYellow = TempData["MessageYellow"];
          ViewBag.MessageGreen = TempData["MessageGreen"];

          

            //check Acl
           // AccessTest
          if (dnAttribute.Acl)
          {
              MethodInfo methodList = t.GetMethod("AccessTest", BindingFlags.Public | BindingFlags.Static);
              if (methodList != null)
              {
                  var allow = (bool)methodList.Invoke(null, new object[] { item, dnAttribute });
                  if (!allow)
                  {
                      TempData["MessageRed"] = "You don't have permissions to edit this element";
                      if (ViewBag.BackUrl != null)
                          return Redirect(ViewBag.BackUrl);
                      else
                          return RedirectToAction("Index");
                  }
              }
          }

          var viewfolder = t.Name;
          var area = GetAreaName(RouteData);
          if (!string.IsNullOrEmpty(area))
          {
              area = "/Areas/" + area;
          }
          var view = string.Format("~{2}/Views/{0}/{1}.cshtml", viewfolder, "Edit", area);
          var prev = ControllerContext.RouteData.Values["controller"];
          ControllerContext.RouteData.Values["controller"] = viewfolder;
          var result = ViewEngines.Engines.FindPartialView(this.ControllerContext, view);

          var ex = result.View != null;
          ControllerContext.RouteData.Values["controller"] = prev;
          if (ex)
          {
              ControllerContext.RouteData.Values["controller"] = viewfolder;
              var viewHtml = View(view, item);
              // ControllerContext.RouteData.Values["controller"] = prev;
              return viewHtml;

          }
          return View(item);
        }

        /// <summary>
        /// Edit post
        /// </summary>
        /// <param name="ID">ID of entity</param>
        /// <param name="item">Entity</param>
        /// <returns>Html page or redirect</returns>
        [HttpPost]
        public ActionResult Edit(int ID, T item)
        {
            var dnAttribute = GetGeneralAttribute(typeof(T));
            if (!dnAttribute.Edit)
            {

                TempData["MessageRed"] = "You don't have permissions to edit this element";
                return RedirectToAction("Index");
            }
            var t = typeof(T);
            if (ModelState.IsValid)
            {
                var updateAtrr = item.GetType().GetProperty("UpdateDate");
                if (updateAtrr != null)
                {
                    updateAtrr.SetValue(item, DateTime.UtcNow);
                }
                MethodInfo methodUpdatingList = t.GetMethod("OnUpdating", BindingFlags.Public | BindingFlags.Static);
                if (methodUpdatingList != null)
                {
                    methodUpdatingList.Invoke(null, new object[] { item });
                }
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();
                MethodInfo methodList = t.GetMethod("OnUpdated", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    methodList.Invoke(null, new object[] { item });
                }
                CleanCache.CleanOutputCache();
                TempData["MessageGreen"] = "Save success";
                return RedirectToAction("Edit", new { ID = item.GetType().GetProperty("ID").GetValue(item) });
                //return RedirectToAction("Index");
            }
            var viewfolder = t.Name;
            var area = GetAreaName(RouteData);
            if (!string.IsNullOrEmpty(area))
            {
                area = "/Areas/" + area;
            }
            var view = string.Format("~{2}/Views/{0}/{1}.cshtml", viewfolder, "Edit", area);
            var prev = ControllerContext.RouteData.Values["controller"];
            ControllerContext.RouteData.Values["controller"] = viewfolder;
            var result = ViewEngines.Engines.FindPartialView(this.ControllerContext, view);

            var ex = result.View != null;
            ControllerContext.RouteData.Values["controller"] = prev;
            if (ex)
            {
                ControllerContext.RouteData.Values["controller"] = viewfolder;
                var viewHtml = View(view, item);
                // ControllerContext.RouteData.Values["controller"] = prev;
                return viewHtml;

            }
            return View(item);
        }

        /// <summary>
        /// Ajax delete
        /// </summary>
        /// <param name="item">Entity</param>
        /// <param name="request">kendo datasource request</param>
        /// <returns>Json {Data,Errors:[]}</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxDelete(T item, [DataSourceRequest]DataSourceRequest request)
        {
            var t = typeof(T);
            var dnAttribute = GetGeneralAttribute(typeof(T));
            if (!dnAttribute.Delete)
            {
                ModelState.AddModelError("General", "You don't have permissions to delete this element");
                return Json(new[] { item }.ToDataSourceResult(request, ModelState));
            }
            object Additional = null;
            if (ModelState.IsValid)
            {
                _db.Set<T>().Attach(item);
                _db.Set<T>().Remove(item);

                _db.SaveChanges();
                MethodInfo methodList = t.GetMethod("OnDeleted", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                   Additional= methodList.Invoke(null, new object[] { item });
                }
            }
            var res = new[] { item }.ToDataSourceResult(request, ModelState);

            return Json(new { res.AggregateResults, res.Data, res.Errors, res.Total, Additional });
        }

        /// <summary>
        /// ajax Insert action
        /// </summary>
        /// <param name="request">kendo datasource request</param>
        /// <param name="item">Entity</param>
        /// <returns>Json {Data,Errors:[]}</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxInsert([DataSourceRequest] DataSourceRequest request, T item)
        {
            var t = typeof(T);
            var dnAttribute = GetGeneralAttribute(typeof(T));
            if (!dnAttribute.CreateAjax)
            {
                ModelState.AddModelError("General", "You don't have permissions to add new element");
                return Json(new[] { item }.ToDataSourceResult(request, ModelState));
            }
            object Additional = null;
            if (ModelState.IsValid)
            {
                //item.LangCode = Lang;
                _db.Set<T>().Add(item);
                _db.SaveChanges();
                MethodInfo methodList = t.GetMethod("OnCreated", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                  Additional=  methodList.Invoke(null, new object[] { item });
                }
            }
            var res = new[] { item }.ToDataSourceResult(request, ModelState);
            
            return Json(new { res.AggregateResults,res.Data,res.Errors, res.Total, Additional   });
        }

        /// <summary>
        /// ajax Update action
        /// </summary>
        /// <param name="request">kendo datasource request</param>
        /// <param name="item">Entity</param>
        /// <returns>Json {Data,Errors:[]}</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _AjaxUpdate([DataSourceRequest] DataSourceRequest request, T item)
        {
            var t = typeof(T);
            var dnAttribute = GetGeneralAttribute(typeof(T));
            if (!dnAttribute.AjaxEdit)
            {
                ModelState.AddModelError("General", "You don't have permissions to edit this element");
                return Json(new[] { item }.ToDataSourceResult(request, ModelState));
            }
            object Additional = null;
            if (ModelState.IsValid)
            {
                MethodInfo methodUpdatingList = t.GetMethod("OnUpdating", BindingFlags.Public | BindingFlags.Static);
                if (methodUpdatingList != null)
                {
                    methodUpdatingList.Invoke(null, new object[] { item });
                }
                
                _db.Entry(item).State = EntityState.Modified;
                _db.SaveChanges();
                MethodInfo methodList = t.GetMethod("OnUpdated", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                   Additional = methodList.Invoke(null, new object[] { item });
                }
                CleanCache.CleanOutputCache();

                // return RedirectToAction("Index");
            }

            var res = new[] { item }.ToDataSourceResult(request, ModelState);

            return Json(new { res.AggregateResults, res.Data, res.Errors, res.Total, Additional });
        }
    }
}