using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;


namespace Uco.Models
{
    [ModelGeneral(AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    [ModelGeneral(Role = "Member", Acl = true, AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]

    // [ModelGeneral(Role = "Member", AjaxEdit = false, Edit = false, Create = false, CreateAjax = false,Delete=false)]
    public partial class ProductSkuMap
    {

        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }


        [Model(Show = false, Edit = false)]
        [Index]
        public int ShopID { get; set; }


        [Model(Show = false, Edit = false)]
        public bool Imported { get; set; }

        [NotMapped]

        [Model(Role = "Admin", Show = true, Edit = true, AjaxEdit = false, FilterOnTop = true)]
        [Model(Show = false, Edit = false)]
        public Shop Shop { get; set; }

        [Display(Prompt = "Main")]
        [Model(ShowInParentGrid = true, FilterOnTop = true, Sort = true)]
        [Index("shortsku"), StringLength(400)]
        public string ShortSKU { get; set; }


        [Display(Prompt = "Main")]
        [UIHint("Currency")]
        [Model(FilterOnTop = false, Sort = true)]
        public decimal Price { get; set; }

        [Display(Prompt = "Main")]
        [Model(FilterOnTop = false, Sort = true)]
        public decimal Quantity { get; set; }

        [Display(Prompt = "Main")]
        // [NotMapped]
        [Model(FilterOnTop = true, Sort = true)]
        public string ImportProductName { get; set; }

        [Display(Prompt = "Main")]
        [Model(ShowInParentGrid = true, FilterOnTop = true, Sort = true)]
        [Index("productsku"), StringLength(400)]
        [UIHint("ProductDetailAutoComplite")]
        public string ProductSKU { get; set; }

        [NotMapped]
        [Model(Sort = true, ShowInParentGrid = true, FilterOnTop = false, KeyField = "ProductSKU", ForeignKey = "SKU")]
        [Display(Prompt = "Main")]
        public virtual Product Product { get; set; }

        #region ACL

        public static ProductSkuMap OnUpdating(ProductSkuMap item)
        {
            if (!string.IsNullOrEmpty(item.ProductSKU))
            {
                //check if shop product in DB

                var sid = item.ShopID;
                var sku = item.ProductSKU;
                var product = LS.CurrentEntityContext.Products.AsNoTracking().FirstOrDefault(x => x.SKU == sku);
                int productCategoryID = 0;
                if (product != null)
                {
                    var pid = product.ID;
                    var productShop = LS.CurrentEntityContext.ProductShopMap.AsNoTracking().FirstOrDefault(x => x.ProductID == pid && x.ShopID == sid);

                        
                    if (productShop == null)
                    {
                        //import
                        DeleteOldProductShop(item.ID, item.ShopID);
                        var newProductShop = new ProductShop();
                        newProductShop.CreateDate = DateTime.Now;
                        newProductShop.Price = item.Price;
                        if (product.UnitsPerPackage.HasValue && product.UnitsPerPackage.Value > 0)
                        {
                            newProductShop.PriceByUnit = newProductShop.Price / product.UnitsPerPackage.Value;
                        }
                        newProductShop.ProductID = product.ID;
                        newProductShop.Quantity = item.Quantity;
                        newProductShop.QuantityType = ProductQuantityType.NotCheck;
                        newProductShop.ShopID = sid;


                        bool needAddCategory = true;
                        productCategoryID = product.CategoryID;
                        //category check
                        if (productCategoryID > 0)
                        {
                            var shopCategories = LS.CurrentEntityContext.ShopCategories.AsNoTracking().Where(x => x.ShopID == sid);
                            var categories = LS.CurrentEntityContext.Categories.AsNoTracking();

                            if (!shopCategories.Any(x => x.CategoryID == productCategoryID))
                            {
                                //create and add
                                var shopCat = new ShopCategory()
                                {
                                    CategoryID = productCategoryID,
                                    DisplayOrder = 1000,
                                    Published = true,
                                    ShopID = sid
                                };
                                LS.CurrentEntityContext.ShopCategories.Add(shopCat);
                                //check if parent cat in shop map
                                var cat = categories.FirstOrDefault(x => x.ID == productCategoryID);
                                if (cat != null && cat.ParentCategoryID > 0)
                                {

                                    int parentCategoryID = cat.ParentCategoryID;
                                    if (!shopCategories.Any(x => x.CategoryID == parentCategoryID))
                                    {
                                        //create and add
                                        var shopCatParent = new ShopCategory()
                                        {
                                            CategoryID = parentCategoryID,
                                            DisplayOrder = 1000,
                                            Published = true,
                                            ShopID = sid
                                        };
                                        LS.CurrentEntityContext.ShopCategories.Add(shopCatParent);

                                    }

                                }

                            }
                            else
                            {
                                newProductShop.NotInCategory = true;
                                if (needAddCategory)//run only one time
                                {
                                    var otherCategory = categories.FirstOrDefault(x => x.Name == "שונות" && x.ParentCategoryID == 0);
                                    if (otherCategory == null)
                                    {
                                        otherCategory = new Category()
                                        {
                                            DisplayOrder = 1000000,
                                            Name = "שונות",
                                            Published = true,

                                        };
                                        LS.CurrentEntityContext.Categories.Add(otherCategory);


                                    }
                                    var catshopmap = shopCategories.FirstOrDefault(x => x.CategoryID == otherCategory.ID);
                                    if (catshopmap == null)
                                    {
                                        var otherShopCategory = new ShopCategory()
                                        {
                                            CategoryID = otherCategory.ID,
                                            DisplayOrder = 1000000,
                                            Published = true,
                                            ShopID = sid
                                        };
                                        LS.CurrentEntityContext.ShopCategories.Add(otherShopCategory);

                                    }
                                    else
                                    {
                                        if (!catshopmap.Published)
                                        {
                                            var catshopMapAttached = LS.CurrentEntityContext.ShopCategories.FirstOrDefault(x => x.ID == catshopmap.ID);
                                            if (catshopMapAttached != null)
                                            {
                                                catshopMapAttached.Published = true;

                                            }
                                        }
                                    }
                                    needAddCategory = false;
                                }
                            }
                        }




                        LS.CurrentEntityContext.ProductShopMap.Add(newProductShop);
                        item.Imported = true;
                    }
                    else
                    {
                        item.Imported = true;
                    }
                }
            }
            //delete product from shop
            if (string.IsNullOrEmpty(item.ProductSKU))
            {
                item.Imported = false;
                DeleteOldProductShop(item.ID, item.ShopID);
            }
            return item;
        }

        private static void DeleteOldProductShop(int productSkuMapId,int shopID)
        {
            //getting old product sku (currently item have null)
            var productSkuForDelete = LS.CurrentEntityContext.ProductSkuMaps
                .Where(x => x.ID == productSkuMapId)
                .Select(x => x.ProductSKU)
                .DefaultIfEmpty()
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(productSkuForDelete))
            {
                var Pid = LS.CurrentEntityContext.Products.Where(x => x.SKU == productSkuForDelete)
                    .Select(x => x.ID).DefaultIfEmpty(0).FirstOrDefault();
                if (Pid > 0)
                {
                    var sid = shopID;
                    var productShopsForRemove = LS.CurrentEntityContext.ProductShopMap
                        .Where(x => x.ShopID == sid && x.ProductID == Pid).ToList();
                    foreach (var ps in productShopsForRemove)
                    {
                        LS.CurrentEntityContext.ProductShopMap.Remove(ps);
                    }
                    //not need to do SaveChanges, after return its will be called
                }
            }
        }

        public static bool AccessTest(ProductSkuMap discount, ModelGeneralAttribute attr)
        {
            if (attr.Acl)
            {
                if (LS.CurrentShop != null)
                {
                    return discount.ShopID == LS.CurrentShop.ID;
                }
                return false;
            };
            return true;
        }
        public static IQueryable<ProductSkuMap> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            int ShopID = 0;
            if (param != null && param.ContainsKey("ShopID"))
            {
                ShopID = int.Parse(param["ShopID"].ToString());
            }
            else if (attr.Acl)
            {
                ShopID = LS.CurrentShop.ID;
            }
            else
            {
                if (attr.Acl && ShopID == 0)
                {
                    return (
                   from oi in LS.CurrentEntityContext.ProductSkuMaps
                   where false
                   select oi);
                }
                return (
                   from oi in LS.CurrentEntityContext.ProductSkuMaps
                   select oi);
            }
            if (attr.Acl && ShopID == 0)
            {
                return (
               from oi in LS.CurrentEntityContext.ProductSkuMaps
               where false
               select oi);
            }
            return (
               from oi in LS.CurrentEntityContext.ProductSkuMaps
               where oi.ShopID == ShopID
               select oi);
        }
        #endregion


    }

}
