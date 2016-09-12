using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{

    [ModelGeneral(Role = "Admin", AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    [ModelGeneral(Role = "Member", Acl = true, Edit = true, EditText = "Attributes", AjaxEdit = true, Create = false, CreateAjax = true, Delete = true, Show = true, DependedShow = true)]
    public partial class ProductShop
    {

        [NotMapped]
        [Model(FilterOnTop = true, Sort = true, IsDropDownName = true)]
        [Display(Name = "Products", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabProduct")]
        [UIHint("GenericDropDownShowOnly")]
        public virtual Product Product { get; set; }

        [NotMapped]
        [Model(Role = "Member", Show = false, AjaxEdit = false, Edit = false)]
        public virtual Shop Shop { get; set; }

        [Display(Name = "CreateDate", Order = 100)]
        [Model(Edit = false, AjaxEdit = false, Show = false)]
        public DateTime CreateDate { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Model(Show = false, Edit = false)]
        [Key]
        public int ID { get; set; }

        [Display(Name = "Product", Order = 100)]
        [Model(Show = false, Edit = false)]
        [Index]
        public int ProductID { get; set; }

        [Display(Name = "ShopID", Order = 100)]
        [Model(Show = false, Edit = false)]
        [Index]
        public int ShopID { get; set; }

        [Display(Name = "OrderPosition", Order = 100)]
        [Model(Show = false, Edit = false)]
        [Index]
        public int OrderPosition { get; set; }

        [Model(Show = false, PreOrder = true, PreOrderDesc = true)]
        [Display(Name = "DontImportPrice", Order = 100, Prompt = "TabProduct")]
        [Index]
        public bool DontImportPrice { get; set; }

        [Display(Name = "Price", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabProduct")]
        [UIHint("Currency")]
        public decimal Price { get; set; }

        //[Display(Name = "OverridenName", Order = 100, Prompt = "TabProduct")]
        // public string OverridenName { get; set; }

        [Display(Name = "PriceByUnit", Order = 100, Prompt = "TabProduct")]
        [UIHint("Currency")]
        [Model(AjaxEdit = true, Edit = true, Show = true)]
        public decimal? PriceByUnit { get; set; }

        [Display(Name = "Quantity", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabProduct")]
        public decimal Quantity { get; set; }

        [Model(Show = false)]
        [Display(Name = "MaxCartQuantity", Order = 100, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "TabProduct")]
        public int? MaxCartQuantity { get; set; }

        [Display(Prompt = "TabProduct")]
        [Model(Show = false)]
        public bool IncludeVat { get; set; }

        [Display(Prompt = "TabProduct")]
        [Model(Show = false, Edit = false)]
        public bool NotInCategory { get; set; }


        [Display(Prompt = "TabProduct")]
        [Model(Show = false, Edit = false)]
        public bool HaveDiscount { get; set; }

        [Display(Prompt = "TabProduct")]
        [Model(Show = false, Edit = false)]
        public int SellCount { get; set; }

        [Display(Prompt = "TabProduct")]
        [Model(Show = false)]
        public bool IncludeInShipPrice { get; set; }

        [Display(Prompt = "TabProduct")]
        [Model(Show = false)]
        public ProductQuantityType QuantityType { get; set; }



        [NotMapped]
        [Display(Name = "Attributes", Order = 200, ResourceType = typeof(Uco.Models.Resources.Models), Prompt = "Attributes")]
        public List<ProductAttributeOption> ProductAttributeOptions { get; set; }

        #region ACL
        public static bool AccessTest(ProductShop productShop, ModelGeneralAttribute attr)
        {
            if (attr.Acl)
            {
                if (LS.CurrentShop != null)
                {
                    return productShop.ShopID == LS.CurrentShop.ID;
                }
                return false;
            };
            return true;
        }
        public static IQueryable<ProductShop> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            int ShopID = 0;
            int CategoryID = 0;
            string productName = null;
            string productSku = null;
            var query = from oi in LS.CurrentEntityContext.ProductShopMap
                        select oi;
            if (param != null && param.ContainsKey("ShopID"))
            {
                ShopID = int.Parse(param["ShopID"].ToString());
            }
            else if (attr.Acl)
            {
                ShopID = LS.CurrentShop.ID;
            }
            //sort
            bool sortCategory = false;
            if (param != null && param.ContainsKey("_SortVariants"))
            {
                var sortVariants = param["_SortVariants"] as List<JsonKeyValue>;
                if (sortVariants.Count > 0 && sortVariants.FirstOrDefault().Name == "Product.CategoryID")
                {
                    sortCategory = true;
                }
            }



            if (ShopID > 0)
            {
                if (sortCategory)
                {
                    query = (
                       from oi in LS.CurrentEntityContext.ProductShopMap
                       join p in LS.CurrentEntityContext.Products.AsQueryable()
                       on oi.ProductID equals p.ID
                       where oi.ShopID == ShopID
                       orderby p.CategoryID
                       select oi);
                }
                else
                {
                    query = (
                       from oi in LS.CurrentEntityContext.ProductShopMap
                       where oi.ShopID == ShopID
                       select oi);
                }
            }
            if (param != null)
            {
                if (param.ContainsKey("Product.CategoryID"))
                {
                    CategoryID = int.Parse(param["Product.CategoryID"].ToString());
                }
                if (param.ContainsKey("Product.Name"))
                {
                    productName = param["Product.Name"].ToString();
                }
                if (param.ContainsKey("Product.SKU"))
                {
                    productSku = param["Product.SKU"].ToString();
                }
            }
            if (CategoryID > 0 || !string.IsNullOrEmpty(productName) || !string.IsNullOrEmpty(productSku))
            {
                var productQuery = LS.CurrentEntityContext.Products.AsQueryable();
                if (CategoryID > 0)
                {
                    productQuery = productQuery.Where(x => x.CategoryID == CategoryID);
                }
                if (!string.IsNullOrEmpty(productName))
                {
                    productQuery = productQuery.Where(x => x.Name.Contains(productName));
                }
                if (!string.IsNullOrEmpty(productSku))
                {
                    productQuery = productQuery.Where(x => x.SKU.Contains(productSku));
                }
                if (sortCategory)
                {
                    query = from ps in query
                            join p in productQuery
                            on ps.ProductID equals p.ID
                            orderby p.CategoryID
                            select ps;
                }
                else
                {
                    query = from ps in query
                            join p in productQuery
                            on ps.ProductID equals p.ID
                            select ps;
                }
            }
            else
            {
                if (sortCategory)
                {
                    query = (
                       from oi in query
                       join p in LS.CurrentEntityContext.Products.AsQueryable()
                       on oi.ProductID equals p.ID
                       orderby p.CategoryID
                       select oi);

                }
            }


            return query;
        }
        #endregion
    }
    public enum ProductQuantityType
    {
        NotCheck = 0,
        CheckByProduct = 1,
        CheckByProductOptions = 2
    }
}
