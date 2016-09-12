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
    public partial class ProductFilter
    {
        public bool? WithPictures { get; set; }
        public string WithoutPictures { get; set; }
        public bool? WithCategory { get; set; }
        public string WithoutCategory { get; set; }

        public bool? ActiveShop { get; set; }
        public int ShopName { get; set; }
    }
    public partial class ProductSmall
    {
        public int ID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
    public partial class ProductReport
    {
        public int ID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string SuggestedSKU { get; set; }
        public bool InvalidCheckSum { get; set; }
    }
    public class ProductVariable<T> where T : class
    {
        public ProductVariable()
        {
            Fields = new List<string>();
        }
        public T Entity { get; set; }
        public List<string> Fields { get; set; }
    }
    [ModelGeneral(Role = "Admin", AjaxEdit = false, Edit = true, Create = true, CreateAjax = false)]
    [ModelGeneral(Role = "Member", AjaxEdit = false, Edit = false, Create = false, CreateAjax = false, Delete = false)]
    public partial class Product
    {

        [HiddenInput(DisplayValue = false)]
        [Key]
        [Display(Prompt = "Main", Order = 1)]
        public int ID { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Index]
        public bool Deleted { get; set; }

        [Display(Prompt = "Main")]
        [Model(FilterOnTop = true)]
        [Index("sku_uniq", IsUnique = true), StringLength(400)]
        public string SKU { get; set; }

        [Model(ShowInParentGrid = true, IsDropDownName = true, FilterOnTop = true)]
        [Display(Prompt = "Main")]
        public string Name { get; set; }

        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        public string SeoDescription { get; set; }

        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000)]
        [Model(Role = "Admin", Show = false, AjaxEdit = false)]
        [Model(Show = false, ShowInEdit = true, Edit = false)]
        public string SeoKeywords { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public bool IsKosher { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public string KosherType { get; set; }


        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public string MadeCoutry { get; set; }


        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public string Capacity { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public bool SoldByWeight { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public bool IgnoreOnImport { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public string MeasureUnit { get; set; }

        [Model(Show = false, Edit = false)]
        [Display(Prompt = "Main")]
        public int ProductMeasureID { get; set; }

        [NotMapped]
        [Model(Sort = true, ShowInParentGrid = true, FilterOnTop = true)]
        [Display(Prompt = "Main")]
        public virtual Measure ProductMeasure { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public decimal? MeasureUnitStep { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public decimal? UnitsPerPackage { get; set; }

        //[Display(Prompt = "Main")]
        //  [Model(Show = false,Edit=false)]
        // public decimal RecomendedPrice { get; set; }

        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Model(Show = false)]
        public string ShortDescription { get; set; }

        [Display(Prompt = "ProductShopOptions")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Model(Show = false)]
        public string ProductShopOptions { get; set; }

        [Display(Prompt = "Main")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Model(Show = false)]
        public string Components { get; set; }

        [UIHint("Image")]
        [Display(Prompt = "Main")]
        // [Model(ShowInParentGrid = true)]
        public string Image { get; set; }


        [Display(Prompt = "Main")]
        [NotMapped]
        [Model(Show = true, Edit = false, AjaxEdit = false, ShowInParentGrid = true)]
        [UIHint("Image")]
        public string SmallGridImage { get; set; }

        [Index]
        [Model(Show = false, Edit = false)]
        public bool HasImage { get; set; }



        //public string FullImage { get; set; }


        //Rating Functional

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int CategoryID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        public int ProductManufacturerID { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false, Edit = false)]
        [Index]
        public int DisplayOrder { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public bool NoTax { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public bool IsFeaturedTop { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public bool IsFeaturedLeft { get; set; }

        //[Model(FilterOnTop=true)]
        [NotMapped]
        [Model(Show = false, Edit = false)]
        [Display(Prompt = "Main")]
        public string Manufacturer { get; set; }

        [NotMapped]
        [Model(Sort = true, ShowInParentGrid = true, FilterOnTop = true)]
        [Display(Prompt = "Main")]
        public virtual Manufacturer ProductManufacturer { get; set; }




        [NotMapped]
        [Model(Sort = true, ShowInParentGrid = true, FilterOnTop = true)]
        [Display(Prompt = "Main")]
        public virtual Category Category { get; set; }


        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public decimal Rate { get; set; }

        [Display(Prompt = "Main")]
        [Model(Show = false)]
        public int RateCount { get; set; }

        [Display(Prompt = "Main")]
        [UIHint("Html")]
        [AllowHtml]
        [Model(Show = false)]
        public string FullDescription { get; set; }

        [Display(Prompt = "ContentUnitMeasure")]
        public int ContentUnitMeasureID { get; set; }
        [Display(Prompt = "ContentUnitMeasure")]
        public decimal ContentUnitPriceMultiplicator { get; set; }
        //  public decimal MinPrice { get; set; }
        //   public decimal MaxPrice { get; set; }
        //  public decimal MiddlePrice { get; set; }



        [NotMapped]
        [Display(Name = "Rates", Order = 270, Prompt = "Rates")]
        public List<ProductRate> ProductRates { get; set; }

        [NotMapped]
        [Display(Name = "Comments", Order = 270, Prompt = "Comments")]
        public List<ProductComment> ProductComments { get; set; }

        [NotMapped]
        [Display(Name = "Personal Notes", Order = 270, Prompt = "Personal Notes")]
        public List<ProductNote> ProductNotes { get; set; }

        [NotMapped]
        [Display(Name = "Prices", Order = 270, Prompt = "Prices")]
        public List<ProductShop> ProductShops { get; set; }

        [NotMapped]
        [Display(Name = "Specifications", Order = 270, Prompt = "Specifications")]
        public List<ProductSpecificationAttributeOption> ProductSpecificationAttributeOptions { get; set; }
        #region Events
        public static void OnUpdating(Product product)
        {
            product.HasImage = !string.IsNullOrEmpty(product.Image);
            product.DisplayOrder = 0;
            if (product.ProductManufacturerID > 0)
            {
                var m = LS.Get<Manufacturer>().FirstOrDefault(x => x.ID == product.ProductManufacturerID);
                if (m != null)
                {
                    product.DisplayOrder = m.DisplayOrder;
                }
            }
        }
        public static void OnCreating(Product product)
        {
            product.HasImage = !string.IsNullOrEmpty(product.Image);
            product.DisplayOrder = 0;
            if (product.ProductManufacturerID > 0)
            {
                var m = LS.Get<Manufacturer>().FirstOrDefault(x => x.ID == product.ProductManufacturerID);
                if (m != null)
                {
                    product.DisplayOrder = m.DisplayOrder;
                }
            }
        }

        public static void OnDeleted(Product product)
        {
            var pshopsRefDelete = LS.CurrentEntityContext
                .ProductShopMap.Where(x => x.ProductID == product.ID)
                .ToList();
            LS.CurrentEntityContext
               .ProductShopMap.RemoveRange(pshopsRefDelete);
            LS.CurrentEntityContext.SaveChanges();

        }
        #endregion

        public static IQueryable<Product> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
        {
            if (param != null)
            {
                if (param.ContainsKey("ShopID"))
                {
                    var shopID = Convert.ToInt32(param["ShopID"]);
                    return (from p in LS.CurrentEntityContext.Products
                            join ps in LS.CurrentEntityContext.ProductShopMap
                                on p.ID equals ps.ProductID
                            where !p.Deleted && ps.ShopID == shopID
                            select p).Distinct();

                }
            }
            return LS.CurrentEntityContext.Products.Where(x => !x.Deleted).AsQueryable();
        }
    }

    // [ModelGeneral(Role = "Admin", AjaxEdit = true, Edit = false, Create = false, CreateAjax = true)]
    //   public partial class FeaturedProduct
    //{

    //      [HiddenInput(DisplayValue = false)]
    //    [Key]
    //    [Display(Prompt = "Main", Order = 1)]
    //    public int ID { get; set; }

    //      [Display(Prompt = "Main")]
    //      [Model(Show = false, Edit = false)]
    //      public int Product1ID { get; set; }

    //      [Display(Prompt = "Main")]
    //      [Model(Show = false, Edit = false)]
    //      public int Product2ID { get; set; }


    //      [NotMapped]
    //      [Model( Sort = true)]
    //      [Display(Prompt = "Main")]
    //      public virtual Product Product1 { get; set; }

    //      [NotMapped]
    //      [Model(Sort = true)]
    //      [Display(Prompt = "Main")]
    //      public virtual Product Product2 { get; set; }
    //}
}
