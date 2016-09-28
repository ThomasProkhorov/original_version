using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using Uco.Models.Overview;
using Uco.Infrastructure.EntityExtensions;
using Uco.Infrastructure.Services;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Caching;
using System.Configuration;
using Uco.Infrastructure.Helpers;
namespace Uco.Infrastructure.Livecycle
{
    public static partial class LS
    {
        public static System.Web.Caching.Cache Cache
        {
            get
            {
                return HttpRuntime.Cache;
            }
        }
        public static Random rnd = new Random();
        public static int GetRandom()
        {

            int month = rnd.Next(4500241, 187008221);
            return month;
        }
        public static List<T> GetRefList<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return CurrentEntityContext.Set<T>().Where(predicate).ToList();
        }
        public static List<T> GetAllRefList<T>() where T : class
        {
            return CurrentEntityContext.Set<T>().ToList();
        }
        public static T GetFirst<T>(Expression<Func<T, bool>> predicate) where T : class
        {

            return CurrentEntityContext.Set<T>().FirstOrDefault(predicate);
        }
        public static IList<T> GetForTest<T>(int ShopID, int limit = 20) where T : class
        {
            if (limit < 1) { limit = 1; }
            if (limit > 100) { limit = 100; }
            return CurrentEntityContext.Set<T>().AsQueryable().Take(limit).ToList();
        }





        public static IEnumerable<ProductOverviewModel> GetProductByName(int shopID, string productName)
        {
            var list = (from ps in LS.CurrentEntityContext.ProductShopMap
                        join p in LS.CurrentEntityContext.Products
                        on ps.ProductID equals p.ID
                        where ps.ShopID == shopID && p.Name.Contains(productName)
                        select new ProductOverviewModel()
                        {
                            FullDescription = p.FullDescription,
                            ID = ps.ID,
                            Image = p.Image,
                            Name = p.Name,
                            Price = ps.Price,
                            PriceByUnit = ps.PriceByUnit,
                            ProductID = p.ID,
                            ProductShopID = ps.ID,
                            Quantity = ps.Quantity,
                            Rate = p.Rate,
                            RateCount = p.RateCount,
                            ShopID = ps.ShopID,
                            ShortDescription = p.ShortDescription,
                            SKU = p.SKU,
                            ContentUnitMeasureID = p.ContentUnitMeasureID,
                            ContentUnitPriceMultiplicator = p.ContentUnitPriceMultiplicator

                        }).Distinct().ToList();

            list.PrepareContentUnitMeasures();
            return list;
        }

        public static IEnumerable<ProductOverviewModel> GetProductByCategory(int shopID, string categoryName, int categoryID) //or categoryID
        {
            if (categoryID == 0 && !string.IsNullOrEmpty(categoryName))
            {
                var cat = LS.CurrentEntityContext.Categories.FirstOrDefault(x => x.Name == categoryName);
                if (cat != null)
                {
                    categoryID = cat.ID;
                }
            }
            var list = (from ps in LS.CurrentEntityContext.ProductShopMap
                        join p in LS.CurrentEntityContext.Products
                        on ps.ProductID equals p.ID
                        where ps.ShopID == shopID
                        && p.CategoryID == categoryID
                        select new ProductOverviewModel()
                        {
                            FullDescription = p.FullDescription,
                            ID = ps.ID,
                            Image = p.Image,
                            Name = p.Name,
                            Price = ps.Price,
                            PriceByUnit = ps.PriceByUnit,
                            ProductID = p.ID,
                            ProductShopID = ps.ID,
                            Quantity = ps.Quantity,
                            Rate = p.Rate,
                            RateCount = p.RateCount,
                            ShopID = ps.ShopID,
                            ShortDescription = p.ShortDescription,
                            SKU = p.SKU,
                            ContentUnitMeasureID = p.ContentUnitMeasureID,
                            ContentUnitPriceMultiplicator = p.ContentUnitPriceMultiplicator

                        }).Distinct().ToList();
            list.PrepareContentUnitMeasures();
            return list;
        }

        public static IEnumerable<ProductOverviewModel> SearchProducts(int shopID, out List<SpecificationOptionModel> options
            , int page = 1, int limit = 20,
            int? categoryID = null, IList<int> filters = null
            , bool loadSpecifications = false // + filter params
            , bool loadAttributes = true  // load ProductShopAttributes 
            , string keywords = null
            , bool isBestSelling = false
            , bool featuredLeft = false//featureleft
            , bool featuredTop = false//featuretop
            , bool favorite = false,
            bool allOrderedProducts = false,
            bool showDiscounts = false
            , bool discountedProducts = false
            , string productName = null
            , List<int> inCategories = null
            , List<int> excludeProductShopList = null
            // , bool orderByManufacturer=false
            )
        {
            //save activity
            UserActivityService.InsertProductSearch(LS.CurrentUser.ID
               , HttpContext.Current.Request.RawUrl,
               HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null
               , LS.GetUser_IP(HttpContext.Current.Request)
               , shopID, page, limit, categoryID, filters, keywords, productName

               );
            if (loadSpecifications)
            {
                if (ConfigurationManager.AppSettings["DisableFilters"] == "true")
                {
                    loadSpecifications = false;
                }
            }

            options = null;
            //fix page
            if (page < 1)
            {
                page = 1;
            }

            if (limit < 0) // to get all
            {
                limit = 0;
            }

            if (isBestSelling)
            {
                isBestSelling = false;
                featuredTop = true;
            }
            // options = new List<SpecificationOptionModel>();
            //set product source
            var source = LS.CurrentEntityContext.Products.Where(x => !x.Deleted).AsQueryable();
            //filter by category and his childrens
            bool isNotInCategory = false;
            if (categoryID.HasValue)
            {
                int OtherCatID = LS.GetCachedFunc<int>(() =>
                {
                    var cat = LS.Get<Category>().FirstOrDefault(x =>
                        (x.Name == "מוצרים נוספים" || x.Name.ToLower() == "other")
                        && x.ParentCategoryID == 0
                        );
                    if (cat != null)
                    {
                        return cat.ID;
                    }
                    return 0;
                }, "OtherCategoryID", 10);
                if (OtherCatID == categoryID.Value)
                {
                    isNotInCategory = true;
                }
                else
                {
                    List<int> categories = new List<int>();

                    categories.Add(categoryID.Value);

                     var shopMenuCategory = LS.Get<ShopCategoryMenu>().Where(x => x.Published && x.ShopID == shopID).ToList();
                     if (shopMenuCategory.Count > 0)
                     {
                         var parent = shopMenuCategory.FirstOrDefault(x=>x.CategoryID == categoryID.Value && x.Level == 0);
                         if(parent!=null)
                         {
                             var childs = shopMenuCategory.Where(x => x.GroupNumber == parent.GroupNumber).Select(x => x.CategoryID).ToList();
                             categories.AddRange(childs);
                         }
                     }
                     else
                     {

                         var childs = LS.CurrentEntityContext.Categories
                             .Where(x => x.ParentCategoryID == categoryID.Value).Select(x => x.ID).ToList();
                         childs.AddRange(LS.CurrentEntityContext.Categories
                            .Where(x => childs.Contains(x.ParentCategoryID)).Select(x => x.ID).ToList());
                         categories.AddRange(childs);
                     }
                    source = source.Where(x => categories.Contains(x.CategoryID));
                }
            }
            else if (inCategories != null && inCategories.Count > 0)
            {
                source = source.Where(x => inCategories.Contains(x.CategoryID));

            }
            if(!string.IsNullOrEmpty(productName))
            {
                if(string.IsNullOrEmpty(keywords))
                {
                    keywords = productName;
                }
                else
                {
                    keywords += " " + productName;
                }
                productName = "";
            }
            if (!string.IsNullOrEmpty(keywords))
            {
                //OK, search algorythm
                // text[word word2 \n
                //      word2 word3 [...]  ]

                var lines = keywords.Split(new char[] { '\n','\r',',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                Expression<Func<Product, bool>> mainPredicate = null;
                foreach (var line in lines)
                {
                    List<List<string>> searchAndOrList = new List<List<string>>();
                    var words = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var countWords = words.Count;
                    for (var i = 0; i < countWords; i++)
                    {
                        var listor = new List<string>();
                        listor.Add(words[i]);
                        searchAndOrList.Add(listor);
                    }

                    for (var i = 0; i < countWords; i++)
                    {
                        var word = words[i];
                        //1)
                        #region woman
                        if (word.EndsWith("ת"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "ה";
                            var variant2 = word.Remove(word.Length - 1, 1) + "ות";
                            searchAndOrList[i].Add(variant1);
                            searchAndOrList[i].Add(variant2);
                            continue;
                        }
                        else if (word.EndsWith("ה"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "ת";
                            var variant2 = word.Remove(word.Length - 1, 1) + "ות";
                            searchAndOrList[i].Add(variant1);
                            searchAndOrList[i].Add(variant2);
                            continue;
                        }
                        else if (word.EndsWith("ות"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "ה";
                            var variant2 = word.Remove(word.Length - 2, 2) + "ת";
                            searchAndOrList[i].Add(variant1);
                            searchAndOrList[i].Add(variant2);
                            continue;
                        }
                        #endregion

                        #region man couple
                        // "ים"
                        if (word.EndsWith("ים"))
                        {
                            var variant1 = word.Remove(word.Length - 2, 2);
                            //replace
                            //"פ" - "ף"
                            //"נ" - "ן"
                            //"מ" - "ם"
                            // "כ" - "ך"
                            // "צ" - "ץ"
                            if (variant1.EndsWith("פ"))
                            {
                                variant1 = variant1.Remove(variant1.Length - 1, 1) + "ף";
                            }
                            else if (variant1.EndsWith("נ"))
                            {
                                variant1 = variant1.Remove(variant1.Length - 1, 1) + "ן";
                            }
                            else if (variant1.EndsWith("מ"))
                            {
                                variant1 = variant1.Remove(variant1.Length - 1, 1) + "ם";
                            }
                            else if (variant1.EndsWith("כ"))
                            {
                                variant1 = variant1.Remove(variant1.Length - 1, 1) + "ך";
                            }
                            else if (variant1.EndsWith("צ"))
                            {
                                variant1 = variant1.Remove(variant1.Length - 1, 1) + "ץ";
                            }

                            searchAndOrList[i].Add(variant1);
                            continue;
                        }
                        #endregion

                        #region man couple 2

                        //"פ" - "ף"
                        //"נ" - "ן"
                        //"מ" - "ם"
                        // "כ" - "ך"
                        // "צ" - "ץ"
                        if (word.EndsWith("ף"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "פ";
                            variant1 += "ים";
                            searchAndOrList[i].Add(variant1);
                            continue;
                        }
                        else if (word.EndsWith("ן"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "נ";
                            variant1 += "ים";
                            searchAndOrList[i].Add(variant1);
                            continue;
                        }
                        else if (word.EndsWith("ם"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "מ";
                            variant1 += "ים";
                            searchAndOrList[i].Add(variant1);
                            continue;
                        }
                        else if (word.EndsWith("ך"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "כ";
                            variant1 += "ים";
                            searchAndOrList[i].Add(variant1);
                            continue;
                        }
                        else if (word.EndsWith("ץ"))
                        {
                            var variant1 = word.Remove(word.Length - 1, 1) + "צ";
                            variant1 += "ים";
                            searchAndOrList[i].Add(variant1);
                            continue;
                        }

                        #endregion
                    }
                    //   var orTextPredicate = words.Where(x => x.Length > 3).ToList(); // select only length > 3, keywords
                    //if (orTextPredicate.Count > 0)
                    int whatPosition = 0;


                    Expression<Func<Product, bool>> predicateAnd = null;
                    foreach (var orTextPredicate in searchAndOrList)
                    {
                        Expression<Func<Product, bool>> predicateOr = null;
                        foreach (var s in orTextPredicate)
                        {
                            if (false && whatPosition == 0)
                            {
                                if (predicateOr != null)
                                {
                                    predicateOr = predicateOr.MultiSearchOrSql(x => x.Name.StartsWith(s + " ")
                                        || x.SKU.StartsWith(s + " ")
                                        || x.ShortDescription.StartsWith(s + " ")
                                        || x.FullDescription.StartsWith(s + " ")
                                        //     || x.Manufacturer.Contains(s)
                                        );
                                }
                                else
                                {
                                    predicateOr = x => (x.Name.StartsWith(s)
                                         || x.SKU.StartsWith(s)
                                        || x.ShortDescription.StartsWith(s)
                                        || x.FullDescription.StartsWith(s)
                                        //      || x.Manufacturer.Contains(s)
                                        );
                                }
                            }
                            else
                            {
                                if (predicateOr != null)
                                {
                                    predicateOr = predicateOr.MultiSearchOrSql(x => x.Name.Contains(s)
                                        || x.SKU.Contains(s)
                                        || x.ShortDescription.Contains(s)
                                        || x.FullDescription.Contains(s)
                                        //     || x.Manufacturer.Contains(s)
                                        );
                                }
                                else
                                {
                                    predicateOr = x => (x.Name.Contains(s)
                                         || x.SKU.Contains(s)
                                        || x.ShortDescription.Contains(s)
                                        || x.FullDescription.Contains(s)
                                        //      || x.Manufacturer.Contains(s)
                                        );
                                }
                            }
                        }
                       if(predicateOr!=null)
                       {
                           if(predicateAnd == null)
                           {
                               predicateAnd = predicateOr;
                           }
                           else
                           {
                              predicateAnd = predicateAnd.MultiSearchAndSql(predicateOr);
                           }
                       }
                        whatPosition++;
                    }
                    if (predicateAnd != null)
                    {
                        if (mainPredicate == null)
                        {
                            mainPredicate = predicateAnd;
                        }
                        else
                        {
                            mainPredicate = mainPredicate.MultiSearchOrSql(predicateAnd);
                        }

                    }
                    
                }
                if (mainPredicate != null)
                {
                    source = source.Where(mainPredicate);
                }
            }

            if (!string.IsNullOrEmpty(productName))
            {

                source = source.Where(x => x.Name.Contains(productName));

            }
            if (featuredLeft)
            {
                source = source.Where(x => x.IsFeaturedLeft);
            }
            if (featuredTop)
            {
                source = source.Where(x => x.IsFeaturedTop);
            }

            //set filtersource
            var filterSource = LS.CurrentEntityContext.ProductSpecificationAttributeOptions.AsQueryable();
            if (filters != null)
            {
                var FilterList = LS.CurrentEntityContext.SpecificationAttributeOptions
                    .Where(x => filters.Contains(x.ID)).OrderBy(x => x.SpecificationAttributeID).ToList();
                int lastAttr = -1;
                Expression<Func<ProductSpecificationAttributeOption, bool>> predicate = null;
                foreach (var f in FilterList)
                {
                    //if (f.SpecificationAttributeID != lastAttr)
                    //{
                    //    lastAttr = f.SpecificationAttributeID;
                    //    if (predicate != null)
                    //    {
                    //        filterSource = filterSource.Where(predicate);
                    //    }
                    //    predicate = null;
                    //}
                    if (predicate != null)
                    {
                        predicate = predicate.MultiSearchOrSql(x => x.SpecificationAttributeOptionID == f.ID);
                    }
                    else
                    {
                        predicate = x => x.SpecificationAttributeOptionID == f.ID;
                    }


                }
                if (predicate != null)
                {
                    filterSource = filterSource.Where(predicate);
                }

            }


            IQueryable<ProductOverviewModel> list = null;
            //select products by filter
            var pshopmapSource = LS.CurrentEntityContext.ProductShopMap.AsQueryable();
            if (isNotInCategory)
            {
                pshopmapSource = pshopmapSource.Where(x => x.NotInCategory);
            }
            if (discountedProducts)
            {
                pshopmapSource = pshopmapSource.Where(x => x.HaveDiscount);
            }
            if (excludeProductShopList != null && excludeProductShopList.Count > 0)
            {
                pshopmapSource = pshopmapSource.Where(x => !excludeProductShopList.Contains(x.ID));
            }
            var curUserId = LS.CurrentUser.ID;
            if (allOrderedProducts)
            {
                pshopmapSource = (from ps in pshopmapSource
                                  join oi in LS.CurrentEntityContext.OrderItems
                                  on ps.ID equals oi.ProductShopID
                                  join o in LS.CurrentEntityContext.Orders
                                  on oi.OrderID equals o.ID
                                  where o.UserID == curUserId && o.ShopID == shopID
                                  select ps);
            }
            if (favorite)
            {
                source = (from src in source
                          join ps in pshopmapSource
                              on src.ID equals ps.ProductID
                          join fav in LS.CurrentEntityContext.ProductFavorites
                              on ps.ID equals fav.ProductShopID
                          where fav.UserID == curUserId && shopID == ps.ShopID
                          select src);
            }
            if (filters != null)
            {
                list = (from ps in pshopmapSource
                        join p in source
                        on ps.ProductID equals p.ID
                        join pso in filterSource
                        on p.ID equals pso.ProductID
                        where ps.ShopID == shopID


                        select new ProductOverviewModel()
                        {
                            FullDescription = p.FullDescription,
                            ID = ps.ID,
                            Image = p.Image,
                            Name = p.Name,
                            Price = ps.Price,
                            PriceByUnit = ps.PriceByUnit,
                            ProductID = p.ID,
                            ProductShopID = ps.ID,
                            Quantity = ps.Quantity,
                            Rate = p.Rate,
                            RateCount = p.RateCount,
                            ShopID = ps.ShopID,
                            ShortDescription = p.ShortDescription,
                            SKU = p.SKU,
                            SellCount = ps.SellCount,
                            //  Manufacturer = p.Manufacturer,
                            NoTax = p.NoTax,
                            MeasureUnit = p.MeasureUnit,
                            MeasureUnitStep = p.MeasureUnitStep,
                            ProductMeasureID = p.ProductMeasureID,
                            SoldByWeight = p.SoldByWeight,
                            CategoryID = p.CategoryID,
                            HasImage = p.HasImage,
                            ProductManufacturerID = p.ProductManufacturerID,
                            DisplayOrder = p.DisplayOrder,
                            OrderPosition = ps.OrderPosition,
                            ContentUnitMeasureID = p.ContentUnitMeasureID,
                            ContentUnitPriceMultiplicator = p.ContentUnitPriceMultiplicator
                        });
                if (false && loadSpecifications)
                {
                    //select specification attribute for filtering
                    options = (from ps in pshopmapSource
                               join p in source
                               on ps.ProductID equals p.ID
                               join pso in filterSource
                               on p.ID equals pso.ProductID
                               join sao in LS.CurrentEntityContext.SpecificationAttributeOptions
                               on pso.SpecificationAttributeOptionID equals sao.ID
                               join sa in LS.CurrentEntityContext.SpecificationAttributes
                               on sao.SpecificationAttributeID equals sa.ID
                               where ps.ShopID == shopID
                               select new SpecificationOptionModel()
                               {
                                   ID = sao.ID,
                                   Name = sao.Name,
                                   SpecificationAttributeID = sao.SpecificationAttributeID,
                                   Attribute = sa.Name

                               }).Distinct().ToList();
                }

            }
            else
            {
                //select products not filtered
                list = (from ps in pshopmapSource
                        join p in source
                        on ps.ProductID equals p.ID
                        //join pso in filterSource
                        //  on p.ID equals pso.ProductID
                        where ps.ShopID == shopID
                        select new ProductOverviewModel()
                         {
                             FullDescription = p.FullDescription,
                             ID = ps.ID,
                             Image = p.Image,
                             Name = p.Name,
                             Price = ps.Price,
                             PriceByUnit = ps.PriceByUnit,
                             ProductID = p.ID,
                             ProductShopID = ps.ID,
                             Quantity = ps.Quantity,
                             Rate = p.Rate,
                             RateCount = p.RateCount,
                             ShopID = ps.ShopID,
                             ShortDescription = p.ShortDescription,
                             SKU = p.SKU,
                             SellCount = ps.SellCount,
                             //Manufacturer = p.Manufacturer,
                             MeasureUnit = p.MeasureUnit,
                             SoldByWeight = p.SoldByWeight,
                             MeasureUnitStep = p.MeasureUnitStep,
                             ProductMeasureID = p.ProductMeasureID,
                             CategoryID = p.CategoryID,
                             HasImage = p.HasImage,
                             ProductManufacturerID = p.ProductManufacturerID,
                             DisplayOrder = p.DisplayOrder,
                             OrderPosition = ps.OrderPosition,
                             ContentUnitMeasureID = p.ContentUnitMeasureID,
                             ContentUnitPriceMultiplicator = p.ContentUnitPriceMultiplicator
                         });
                if (false && loadSpecifications)
                {
                    //select current filters
                    options = (from ps in pshopmapSource
                               join p in source
                               on ps.ProductID equals p.ID
                               join pso in LS.CurrentEntityContext.ProductSpecificationAttributeOptions
                               on p.ID equals pso.ProductID
                               join sao in LS.CurrentEntityContext.SpecificationAttributeOptions
                               on pso.SpecificationAttributeOptionID equals sao.ID
                               join sa in LS.CurrentEntityContext.SpecificationAttributes
                               on sao.SpecificationAttributeID equals sa.ID
                               where ps.ShopID == shopID
                               select new SpecificationOptionModel()
                               {
                                   ID = sao.ID,
                                   Name = sao.Name,
                                   SpecificationAttributeID = sao.SpecificationAttributeID,
                                   Attribute = sa.Name

                               }).Distinct().ToList();
                }
            }

            if (discountedProducts)
            {
                list = list.Distinct()
                   .OrderBy(x => x.OrderPosition)
                   .ThenBy(x => x.CategoryID)                   
                    .ThenByDescending(x => x.HasImage)
                    .ThenByDescending(x => x.SellCount);
            }
            else
            {
                //order and paging
                list = list.Distinct()
                   .OrderBy(x => x.CategoryID)
                   .ThenBy(x => x.OrderPosition)
                    .ThenByDescending(x => x.HasImage)
                    .ThenByDescending(x => x.SellCount);
            }

            if (limit > 0)
            {
                list = list
                   .Skip((page - 1) * limit).Take(limit);
            }

            var products = list.ToList();

            if (!string.IsNullOrEmpty(keywords) && page == 1)
            {
                if (products.Count == 0)
                {
                    //no result
                    UserActivityService.InsertSearchNotFound(keywords, shopID, LS.CurrentUser.ID
                        , HttpContext.Current.Request.RawUrl,
                        HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.OriginalString : null
                         , LS.GetUser_IP(HttpContext.Current.Request));
                }
                
            }


            //load attribute options (overriden sku, overriden price)
            if (false && loadAttributes)
            {
                foreach (var item in products)
                {
                    var attributes = (from pao in LS.CurrentEntityContext.ProductAttributeOptions
                                      // join pa in LS.CurrentEntityContext.ProductAttributes
                                      //on pao.ProductAttributeID equals pa.ID
                                      where pao.ProductShopID == item.ProductShopID
                                      select new ProductAttributeOptionModel()
                                      {
                                          ID = pao.ID,
                                          Name = pao.Name,
                                          OverridenPrice = pao.OverridenPrice,
                                          // OverridenPriceStr = pao.OverridenPrice
                                          OverridenSku = pao.OverridenSku,
                                          //ProductAttributeID = pao.ProductAttributeID,
                                          ProductShopID = pao.ProductShopID,
                                          Quantity = pao.Quantity,
                                          // ProductAttribute = pa.Name

                                      }).ToList();
                    foreach (var a in attributes)
                    {
                        if (!a.OverridenPrice.HasValue)
                        {
                            a.OverridenPrice = item.Price;
                        }
                        a.OverridenPriceStr = ShoppingService.FormatPrice(a.OverridenPrice.Value);
                    }
                    item.Attributes = attributes;

                }
            }
            var cart = LS.CurrentEntityContext.ShoppingCartItems.Where(x => x.ShopID == shopID
                && x.UserID == curUserId)
                .ToList();
            foreach (var item in products)
            {
                if (showDiscounts)
                {
                    //get from cache
                    string discountInfo = LS.GetCachedFunc<string>(() =>
                    {
                        DateTime now = DateTime.Now;
                        var discounts = LS.Get<Discount>(item.ShopID.ToString() + "_" + DiscountType.ForCartItem.ToString())
                            .Where(x => x.ProductShopIDs != null
                                && (x.StartDate == null || x.StartDate < now)
                                && (x.EndDate == null || x.EndDate > now)
                                && x.ProductShopIDs.Contains("," + item.ProductShopID + ",")).ToList();
                        ;
                        if (discounts.Count > 0)
                        {
                            return string.Join(", ", discounts.Select(x => x.Name).ToArray());
                        }
                        return "";//null not allowed
                    }, "discount_info_" + item.ProductShopID, 30);//30 minute cache
                    item.DiscountDescription = discountInfo;
                    item.HaveDiscount = !string.IsNullOrEmpty(discountInfo);

                }
                var curCartItem = cart.FirstOrDefault(x => x.ProductShopID == item.ProductShopID);
                if (curCartItem != null)
                {
                    item.isInShoppingCart = true;
                    item.QuantityToBuy = curCartItem.Quantity;
                    item.QuantityType = curCartItem.QuantityType;
                }
                if (item.ProductManufacturerID > 0)
                {
                    var m = LS.Get<Manufacturer>().FirstOrDefault(x => x.ID == item.ProductManufacturerID);
                    item.Manufacturer = m != null ? m.Name : "";
                }
                if(item.ProductMeasureID > 0)
                {
                    item.ProductMeasure = LS.Get<Measure>().FirstOrDefault(x=>x.ID == item.ProductMeasureID);
                    if(item.ProductMeasure!=null)
                    {
                        item.MeasureUnit = item.ProductMeasure.NameForUser;
                    }
                }
            }
            if (isBestSelling)
            {
                return products.OrderBy(x => x.Price); //to do: load best selling products
            }
            products.PrepareContentUnitMeasures();

            //end attributes
            return products;
        }
        public static string GetArea()
        {
            var context = new HttpContextWrapper(System.Web.HttpContext.Current);
            var routeData = RouteTable.Routes.GetRouteData(context);
            return GetAreaName(routeData);
        }
        private static string GetAreaName(RouteData routeData)
        {
            object area;

            if (routeData.DataTokens.TryGetValue("area", out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }
        private static string GetAreaName(RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;

            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            Route castRoute = route as Route;

            if (castRoute != null && castRoute.DataTokens != null)
            {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }
        #region ModelAttribute
        //Role depended settings
        private static Dictionary<string, ModelGeneralAttribute> _GeneralRole = new Dictionary<string, ModelGeneralAttribute>() 
        {
        {"Admin" , new ModelGeneralAttribute() },
        {"Member", new ModelGeneralAttribute(){ AjaxEdit=false,Create=false,CreateAjax=false,Delete=false,Edit=false,Show=false,DependedShow=false } }
        };

        public static ModelGeneralAttribute GetModelGeneral(Type type, string role)
        {
            var dnAttribute = type.GetCustomAttributes<ModelGeneralAttribute>(true).Where(x => x.Role == role || x.Role == null).OrderByDescending(x => x.Role).ToList().FirstOrDefault() as ModelGeneralAttribute;
            if (dnAttribute == null)
            {
                if (!string.IsNullOrEmpty(role) && _GeneralRole.ContainsKey(role))
                {
                    dnAttribute = _GeneralRole[role];
                }
                else
                {
                    dnAttribute = new ModelGeneralAttribute();
                }
            }
            return dnAttribute;
        }
        #endregion
        #region caching entity model
        public static List<T> GetCustom<T>(string key, Func<object> func) where T : class
        {
            return Get<T>(key, false, func);
        }
        #endregion
        #region SeoUrl
        /// <summary>
        /// Convert a name into a string that can be appended to a Uri.
        /// </summary>
        private static string EscapeName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                name = NormalizeString(name);

                // Replaces all non-alphanumeric character by a space
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < name.Length; i++)
                {
                    builder.Append(char.IsLetterOrDigit(name[i]) ? name[i] : ' ');
                }

                name = builder.ToString();

                // Replace multiple spaces into a single dash
                name = Regex.Replace(name, @"[ ]{1,}", @"-", RegexOptions.None);
            }

            return name;
        }

        /// <summary>
        /// Strips the value from any non english character by replacing thoses with their english equivalent.
        /// </summary>
        /// <param name="value">The string to normalize.</param>
        /// <returns>A string where all characters are part of the basic english ANSI encoding.</returns>
        /// <seealso cref="http://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net"/>
        private static string NormalizeString(string value)
        {
            string normalizedFormD = value.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < normalizedFormD.Length; i++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(normalizedFormD[i]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(normalizedFormD[i]);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
        #endregion
        public static void SetSingle<Tent>(Tent o, Type t) where Tent : class
        {
            if (Cache[t.Name] != null)
            {
                var p = t.GetProperty("ID");
                if (p != null)
                {
                    foreach (var c in (Dictionary<string, List<Tent>>)Cache[t.Name])
                    {
                        var ID = p.GetValue(o);
                        ParameterExpression pe = Expression.Parameter(t, "p");
                        Expression left = Expression.Property(pe, "ID");
                        Expression right = Expression.Constant(ID);
                        Expression e1 = Expression.Equal(left, right);
                        var predicate = Expression.Lambda<Func<Tent, bool>>
                 (Expression.Equal(left, right),
                 new[] { pe }).Compile();
                        var fd = c.Value.FirstOrDefault(predicate);
                        if (fd != null)
                        {
                            var index = c.Value.IndexOf(fd);
                            if (index > -1)
                            {
                                c.Value[index] = o;
                                //  c.Value.Insert(index, o);

                            }
                        }
                    }
                }
                MethodInfo methodList = t.GetMethod("CacheItem", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    methodList.Invoke(null, new object[] { o, true, false, false });
                }
            }
            var property = t.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<SeoUrlAttribute>() != null);
            if (property != null)
            {
                var seoattribute = property.GetCustomAttribute<SeoUrlAttribute>();
                if (seoattribute != null)
                {

                    string seoUrl = (string)property.GetValue(o);
                    if (string.IsNullOrEmpty(seoUrl))
                    {
                        //get new seo name
                        var nameprop = t.GetProperty(seoattribute.NameField);
                        if (nameprop == null)
                        {
                            nameprop = t.GetProperty(seoattribute.TitleField);
                        }
                        if (nameprop != null)
                        {

                            seoUrl = (string)nameprop.GetValue(o);
                            if (seoUrl != null)
                            {
                                seoUrl = EscapeName(NormalizeString(seoUrl));
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(seoUrl))
                    {
                        var p = t.GetProperty("ID");
                        if (p != null)
                        {
                            var IDint = p.GetValue(o);
                            if (IDint is int)
                            {
                                //do insert update delete seo url record
                                int ID = (int)IDint;
                                seoUrl = seoUrl.ToLower();
                                var exists = LS.Get<UrlRecord>().Where(x => x.Slug.StartsWith(seoUrl)).ToList();
                                int nextNum = 1;
                                string nextPlus = "-" + nextNum.ToString();
                                string baseUrl = seoUrl;
                                while (exists.Any(x => x.Slug == seoUrl && (
                                    x.EntityID != ID || x.EntityName != t.Name)))
                                {
                                    seoUrl = baseUrl + nextPlus;
                                    nextNum++;
                                    nextPlus = "-" + nextNum.ToString();

                                }
                                //now seoUrl is clean and unique

                                //update current
                                //get from DB, must be real data, assigned to context
                                var thisUrlRecord = LS.CurrentEntityContext.UrlRecords
                                              .FirstOrDefault(x => x.EntityID == ID && x.EntityName == t.Name);
                                if (thisUrlRecord != null)
                                {
                                    if (thisUrlRecord.Slug != seoUrl) //if only text changed
                                    {


                                        thisUrlRecord.Slug = seoUrl;
                                        thisUrlRecord.IsActive = true;

                                        //don`t use save change there, make cycle and overflow
                                        //LS.CurrentEntityContext.SaveChanges();
                                        var list = new List<UrlRecord>() { thisUrlRecord };
                                        list.SqlUpdateById();
                                        list.Update();

                                    }
                                }
                                else
                                {
                                    //insert new to DB
                                    var newUrlRecord = new UrlRecord()
                                    {
                                        EntityID = ID,
                                        EntityName = t.Name,
                                        IsActive = true,
                                        Slug = seoUrl
                                    };
                                    var list = new List<UrlRecord>() { newUrlRecord };
                                    list.SqlInsert();
                                    list.Insert();

                                    //don`t use save change there, make cycle and overflow
                                    //  LS.CurrentEntityContext.SaveChanges();

                                }
                                property.SetValue(o, seoUrl);
                            }
                        }

                    }
                }

            }
        }
        public static void RemoveSingle<Tent>(Tent o, Type t) where Tent : class
        {
            if (Cache[t.Name] != null)
            {
                var p = t.GetProperty("ID");
                if (p != null)
                {
                    foreach (var c in (Dictionary<string, List<Tent>>)Cache[t.Name])
                    {
                        var ID = p.GetValue(o);
                        ParameterExpression pe = Expression.Parameter(t, "p");
                        Expression left = Expression.Property(pe, "ID");
                        Expression right = Expression.Constant(ID);
                        Expression e1 = Expression.Equal(left, right);
                        var predicate = Expression.Lambda<Func<Tent, bool>>
                 (Expression.Equal(left, right),
                 new[] { pe }).Compile();
                        var fd = c.Value.FirstOrDefault(predicate);
                        if (fd != null)
                        {
                            var index = c.Value.IndexOf(fd);
                            if (index > -1)
                            {
                                c.Value.RemoveAt(index);// = o;
                                //  c.Value.Insert(index, o);

                            }
                        }
                    }
                }
                MethodInfo methodList = t.GetMethod("CacheItem", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    methodList.Invoke(null, new object[] { o, false, false, true });
                }
            }
        }
        public static void AddSingle<Tent>(Tent o, Type t) where Tent : class
        {
            if (Cache[t.Name] != null)
            {
                var p = t.GetProperty("ID");
                if (p != null)
                {
                    foreach (var c in (Dictionary<string, List<Tent>>)Cache[t.Name])
                    {

                        c.Value.Add(o);
                        //  c.Value.Insert(index, o);

                    }
                }
                MethodInfo methodList = t.GetMethod("CacheItem", BindingFlags.Public | BindingFlags.Static);
                if (methodList != null)
                {
                    methodList.Invoke(null, new object[] { o, false, true, false });
                }
            }
        }
        public static void Update<T>(this IEnumerable<T> list) where T : class
        {
            foreach (var e in list)
            {
                try
                {
                    var t = e.GetType();
                    // var o = Activator.CreateInstance(t);
                    MethodInfo method = typeof(LS).GetMethod("SetSingle", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo genericMethod = method.MakeGenericMethod(t);
                    genericMethod.Invoke(null, new object[] { e, t });
                    // SetSingle(e, t);
                }
                catch { }
            }
        }
        public static void Insert<T>(this IEnumerable<T> list) where T : class
        {
            foreach (var e in list)
            {
                try
                {
                    var t = e.GetType();
                    // var o = Activator.CreateInstance(t);
                    MethodInfo method = typeof(LS).GetMethod("AddSingle", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo genericMethod = method.MakeGenericMethod(t);
                    genericMethod.Invoke(null, new object[] { e, t });
                    // SetSingle(e, t);
                }
                catch { }
            }
        }
        public static void Delete<T>(this IEnumerable<T> list) where T : class
        {
            foreach (var e in list)
            {
                try
                {
                    var t = e.GetType();
                    // var o = Activator.CreateInstance(t);
                    MethodInfo method = typeof(LS).GetMethod("RemoveSingle", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo genericMethod = method.MakeGenericMethod(t);
                    genericMethod.Invoke(null, new object[] { e, t });
                    // SetSingle(e, t);
                }
                catch { }
            }
        }
        public static List<SelectListItem> GetSelectList(Type type)
        {
            MethodInfo methodList = typeof(LS).GetMethod("GetSelectListGen", BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericMethodList = methodList.MakeGenericMethod(type);

            var list = (List<SelectListItem>)genericMethodList.Invoke(null, new object[] { });
            return list;
        }
        public static List<SelectListItem> GetSelectListGen<T>() where T : class
        {
            var list = Get<T>();
            return list.Select(x => new SelectListItem()
            {
                Text = typeof(T).GetProperty("Name").GetValue(x).ToString(),
                Value = typeof(T).GetProperty("ID").GetValue(x).ToString(),
            }).ToList();
        }
        public static void SetMeToCache<T>(this T o, string key)
        {
            lock (_lock)
            {
                LS.Cache[key] = o;
            }
        }
        public static bool IsExistInCache(string key)
        {
            return LS.Cache[key] != null;
        }
        public static void SetToCache<T>(T o, string key, int Minutes = 0)
        {
            lock (_lock)
            {
                if (Minutes == 0)
                {
                    LS.Cache[key] = o;
                }
                else
                {
                    LS.Cache.Add(key, o, null, DateTime.Now.AddMinutes(Minutes), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
            }
        }
        public static void RemoveFromCache(string key)
        {
            LS.Cache.Remove(key);
        }
        public static T GetCachedFunc<T>(Func<T> o, string key, int Minutes = 0, bool replaceNew = false)
        {
            lock (_lock)
            {
                if (replaceNew)
                {
                    LS.Cache.Remove(key);
                }
                if (LS.Cache[key] == null)
                {
                    if (Minutes == 0)
                    {
                        LS.Cache[key] = o();
                    }
                    else
                    {
                        LS.Cache.Add(key, o(), null, DateTime.Now.AddMinutes(Minutes), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }
                }
            }
            return (T)LS.Cache[key];
        }
        public static T GetFromCache<T>(string key)
        {
            if (LS.Cache[key] == null)
            {
                return default(T);
            }
            return (T)LS.Cache[key];
        }
        public static object _lock = new object();


        public static void Clear<T>() where T : class
        {
            var t = typeof(T);
            string key = t.Name;
            string master = key;
            if (LS.Cache[master] != null)
            {
                LS.Cache.Remove(master);
            }
        }

        public static void Clear(string key)
        {
            if (LS.Cache[key] != null)
            {
                LS.Cache.Remove(key);
            }
        }


        public static List<T> CleanGet<T>() where T : class
        {
            Clear<T>();
            return Get<T>();
        }
        public static List<T> Get<T>(string AdditionalKey = "", bool cacheByDomainAndLang = true
            , Func<object> func = null
            , string masterKey = "") where T : class
        {
            var t = typeof(T);
            int DomainID = 0;
            bool islang = false;
            string LanguageCode = "";
            string key = t.Name;
            string master = key;
            if (!string.IsNullOrEmpty(masterKey))
            {
                master = masterKey;
                key = masterKey;
            }
            if (cacheByDomainAndLang && t.GetProperty("LangCode") != null)
            {
                LanguageCode = SF.GetLangCodeThreading();
                key += "_" + LanguageCode;
                islang = true;
            }
            if (cacheByDomainAndLang && t.GetProperty("DomainID") != null)
            {
                DomainID = RP.GetCurrentSettings().ID;
                key += "_" + DomainID.ToString();
            }
            var mainListKey = key;
            key += AdditionalKey;
            if (LS.Cache[master] == null)
            {
                LS.Cache[master] = new Dictionary<string, List<T>>();
            }
            lock (_lock)
            {
                if (!((Dictionary<string, List<T>>)LS.Cache[master]).ContainsKey(key))
                {
                    if (func != null)
                    {
                        var l = (List<T>)func();

                        ((Dictionary<string, List<T>>)LS.Cache[master]).Add(key, l);
                    }
                    else if (!string.IsNullOrEmpty(AdditionalKey) && ((Dictionary<string, List<T>>)LS.Cache[master]).ContainsKey(mainListKey))
                    {
                        var l = new List<T>();

                        ((Dictionary<string, List<T>>)LS.Cache[master]).Add(key, l);
                    }
                    else
                    {

                        var query = new DBContextService().EntityContext.Set<T>().AsQueryable();
                        if (DomainID > 0)
                        {
                            ParameterExpression pe = Expression.Parameter(t, "p");
                            Expression left = Expression.Property(pe, "DomainID");
                            Expression right = Expression.Constant(DomainID);
                            Expression e1 = Expression.Equal(left, right);
                            var predicate = Expression.Lambda<Func<T, bool>>
                     (Expression.Equal(left, right),
                     new[] { pe }).Compile();
                            query = query.Where(predicate).AsQueryable();
                        }
                        if (islang)
                        {
                            ParameterExpression pe = Expression.Parameter(t, "p"); // p=>p.field == value
                            Expression left = Expression.Property(pe, "LangCode");
                            Expression right = Expression.Constant(LanguageCode);
                            Expression e1 = Expression.Equal(left, right);
                            var predicate = Expression.Lambda<Func<T, bool>>
                     (Expression.Equal(left, right),
                     new[] { pe }).Compile();
                            query = query.Where(predicate).AsQueryable();
                        }
                        // Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T,bool>>();
                        var l = query.ToList();

                        ((Dictionary<string, List<T>>)LS.Cache[master]).Add(mainListKey, l);
                        if (mainListKey != key)
                        {
                            ((Dictionary<string, List<T>>)LS.Cache[master]).Add(key, new List<T>());
                        }
                        MethodInfo methodList = t.GetMethod("CacheList", BindingFlags.Public | BindingFlags.Static);
                        if (methodList != null)
                        {
                            methodList.Invoke(null, new object[] { l });
                        }
                    }
                    // LS.Cache[key] = l;
                }
            }
            return ((Dictionary<string, List<T>>)LS.Cache[master])[key];
            //  return new List<T>();
        }

    }
}