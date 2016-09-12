using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uco.Models.Overview
{
    public class ProductOverviewModel
    {
        public ProductOverviewModel()
        {
            ProductComments = new List<ProductComment>();
            Attributes = new List<ProductAttributeOptionModel>();
            Specifications = new List<SpecificationOptionModel>();
            RelatedProductShops = new List<RelatedProductShop>();
            QuantityToBuy = 1;
        }
        public int ID { get; set; }

        public int ProductShopID { get; set; }

        public int ProductID { get; set; }

        public string Name { get; set; }

        public bool NoTax { get; set; }

        public int ShopID { get; set; }
        public bool isInShoppingCart { get; set; }
        public decimal QuantityToBuy { get; set; }
        public QuantityType QuantityType { get; set; }
        public int CategoryID { get; set; }
        public int ProductManufacturerID { get; set; }
        public int DisplayOrder { get; set; }
        public int OrderPosition { get; set; }
        public decimal Price { get; set; }
        public decimal? PriceByUnit { get; set; }
        public decimal Quantity { get; set; }

        public string SKU { get; set; }

        public bool SoldByWeight { get; set; }
        public string MeasureUnit { get; set; }

        public int ProductMeasureID { get; set; }

        public virtual Measure ProductMeasure { get; set; }

        public decimal? MeasureUnitStep { get; set; }
        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string Image { get; set; }
        public bool HasImage { get; set; }
        public int SellCount { get; set; }
        public decimal Rate { get; set; }

        public int RateCount { get; set; }
        public string Manufacturer { get; set; }

        public bool HaveDiscount { get; set; }
        public string DiscountDescription { get; set; }
        public List<ProductComment> ProductComments { get; set; }
        public List<ProductAttributeOptionModel> Attributes { get; set; }
        public List<SpecificationOptionModel> Specifications { get; set; }
        public List<RelatedProductShop> RelatedProductShops { get; set; }
        public List<ProductOverviewModel> RelatedProducts { get; set; }
        public string ProductNoteText { get; set; }



        public string SeoDescription { get; set; }

        public string SeoKeywords { get; set; }


        public int ContentUnitMeasureID { get; set; }
        public decimal ContentUnitPriceMultiplicator { get; set; }
        public decimal ContentUnitPrice { get; set; }
        public string ContentUnitName { get; set; }
    }

    public class ProductShortModel
    {

        public int ID { get; set; }
        public string Name { get; set; }
    }
    public class RelatedProductShop
    {
        public int ID { get; set; }

        public int ShopID { get; set; }
        public int ProductShopID { get; set; }
        public int ProductID { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public string PriceStr { get; set; }
    }
    public class SpecificationOptionModel
    {

        public int ID { get; set; }

        public int SpecificationAttributeID { get; set; }

        public virtual string Attribute { get; set; }
        public virtual string AttributeType { get; set; }

        public virtual string CustomValue { get; set; }

        public string Name { get; set; }
    }
}