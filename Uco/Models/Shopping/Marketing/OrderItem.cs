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
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{
    #region Html table list
    public partial class OrderChangedProductTable:OrderProductTable
    {
        public OrderChangedProductTable(List<OrderItem> items):base(items)
        {
        }
        public OrderChangedProductTable(int orderID)
            : base(orderID)
        {
        }
    }
    public partial class OrderRemovedProductTable : OrderProductTable
    {
         public OrderRemovedProductTable(List<OrderItem> items):base(items)
        {
        }
         public OrderRemovedProductTable(int orderID)
            : base(orderID)
        {
        }
    }
    public partial class OrderProductTable
    {
        private List<OrderItem> _items;
        public OrderProductTable(List<OrderItem> items)
        {
            

            var shopproductIds = items.Select(x => x.ProductShopID).ToList();
            var shpproducts = LS.CurrentEntityContext.ProductShopMap.Where(x => shopproductIds.Contains(x.ID)).ToList();

            var productIds = shpproducts.Select(x => x.ProductID).ToList();
            var products = LS.CurrentEntityContext.Products.Where(x => productIds.Contains(x.ID)).ToList();
            foreach (var item in items)
            {
                if (shpproducts.Any(x => x.ID == item.ProductShopID))
                {
                    item.ProductShop = shpproducts.FirstOrDefault(x => x.ID == item.ProductShopID);
                    if (products.Any(x => x.ID == item.ProductShop.ProductID))
                    {
                        item.ProductShop.Product = products.FirstOrDefault(x => x.ID == item.ProductShop.ProductID);
                    }
                }
            }
            this._items = items;
            if(_items== null)
            {
                _items = new List<OrderItem>();
            }
        }
         public OrderProductTable(int orderID)
        {
            if (orderID <= 0 || LS.CurrentEntityContext == null)
             {
                 return;
             }
            var items = LS.CurrentEntityContext.OrderItems.Where(x => x.OrderID == orderID).ToList();

            var shopproductIds = items.Select(x => x.ProductShopID).ToList();
            var shpproducts = LS.CurrentEntityContext.ProductShopMap.Where(x => shopproductIds.Contains(x.ID)).ToList();

            var productIds = shpproducts.Select(x => x.ProductID).ToList();
            var products = LS.CurrentEntityContext.Products.Where(x => productIds.Contains(x.ID)).ToList();
             foreach(var item in items)
             {
                 if(shpproducts.Any(x=>x.ID == item.ProductShopID))
                 {
                     item.ProductShop = shpproducts.FirstOrDefault(x => x.ID == item.ProductShopID);
                     if(products.Any(x=>x.ID == item.ProductShop.ProductID))
                     {
                         item.ProductShop.Product = products.FirstOrDefault(x => x.ID == item.ProductShop.ProductID);
                     }
                 }
             }
            this._items = items;
        }
        private string _fullinfo;
        private string _shortnfo;
        private string _infowithotattributanddiscount;
        public string FullInfoWithoutDiscountAndAttributes
        {
            get
            {
                if (_infowithotattributanddiscount == null)
                {
                    var domain = LS.CurrentEntityContext.SettingsAll.Select(x => x.Domain).FirstOrDefault();
                    var urlbase = string.Format("{0}://{1}",
                        "http",
                        domain);

                    StringBuilder table = new StringBuilder();
                    table.AppendLine("<table dir=\"rtl\" style=\"text-align:right\">");
                    table.Append("<tr>");
                    table.Append("<td>" + RP.M("OrderItem", "ProductShop") + "</td>");
                    table.Append("<td>" + RP.M("Product", "SKU") + "</td>");
                    table.Append("<td>" + RP.M("Product", "Image") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "Price") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "Quantity") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "UnitPrice") + "</td>");
                   table.Append("</tr>");
                    foreach (var item in _items)
                    {
                        table.AppendLine("<tr>");

                        table.Append("<td>" + (item.ProductShop != null
                            && item.ProductShop.Product != null ? item.ProductShop.Product.Name : "") + "</td>");
                        table.Append("<td>" + item.SKU + "</td>");
                        table.Append("<td>" + (item.ProductShop != null
                            && item.ProductShop.Product != null
                            ? "<img height=\"30px\" src=\"" + urlbase + SF.GetImage( item.ProductShop.Product.Image,200,200,true,true) + "\" alt=\"alt\" />" : "") + "</td>");
                        table.Append("<td>" + item.PriceStr + (item.QuantityType == QuantityType.Default
                            // && item.MeasureUnitStep.HasValue
                            && item.MeasureUnit != null ?
                             (" / " + item.MeasureUnit)
                            : " / " + RP.S("View.Shop.ProductItem.PriceForItem")
                            ) + "</td>");
                        table.Append("<td>" + item.Quantity.ToString("0.00") + "</td>");
                        table.Append("<td>" + item.UnitPriceStr + "</td>");
                     
                        table.Append("</tr>");
                    }

                    table.AppendLine("</table>");
                    _infowithotattributanddiscount = table.ToString();
                }
                return _infowithotattributanddiscount;// "<table><tr><td>" + RP.M("OrderItem", "SKU") + "</td></tr></table>";
            }
        }
        public string ShortInfo
        {
            get
            {
                if (_shortnfo == null)
                {
                    var domain = LS.CurrentEntityContext.SettingsAll.Select(x => x.Domain).FirstOrDefault();
                    var urlbase = string.Format("{0}://{1}",
                        "http",
                        domain);

                    StringBuilder table = new StringBuilder();
                    table.AppendLine("<table dir=\"rtl\" style=\"text-align:right\">");
                    table.Append("<tr>");
                    table.Append("<td>" + RP.M("OrderItem", "ProductShop") + "</td>");
                    table.Append("<td>" + RP.M("Product", "SKU") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "Price") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "Quantity") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "UnitPrice") + "</td>");
                    table.Append("</tr>");
                    foreach (var item in _items)
                    {
                        table.AppendLine("<tr>");

                        table.Append("<td>" + (item.ProductShop != null
                            && item.ProductShop.Product != null ? item.ProductShop.Product.Name : "") + "</td>");
                        table.Append("<td>" + item.SKU + "</td>");
                        table.Append("<td>" + item.PriceStr + (item.QuantityType == QuantityType.Default
                            // && item.MeasureUnitStep.HasValue
                            && item.MeasureUnit != null ?
                             (" / " + item.MeasureUnit)
                            : " / " + RP.S("View.Shop.ProductItem.PriceForItem")
                            ) + "</td>");
                       table.Append("<td>" + item.Quantity.ToString("0.00") + "</td>");
                        table.Append("<td>" + item.UnitPriceStr + "</td>");
                      
                        table.Append("</tr>");
                    }

                    table.AppendLine("</table>");
                    _shortnfo = table.ToString();
                }
                return _shortnfo;// "<table><tr><td>" + RP.M("OrderItem", "SKU") + "</td></tr></table>";
            }
        }
        public string FullInfo
        {
            get
            {
                if(_fullinfo == null)
                {
                    var domain = LS.CurrentEntityContext.SettingsAll.Select(x => x.Domain).FirstOrDefault();
                    var urlbase = string.Format("{0}://{1}",
                        "http",
                        domain);

                    StringBuilder table = new StringBuilder();
                    table.AppendLine("<table dir=\"rtl\" style=\"text-align:right\">");
                    table.Append("<tr>");
                    table.Append("<td>" + RP.M("OrderItem", "ProductShop") + "</td>");
                    table.Append("<td>" + RP.M("Product", "SKU") + "</td>");
                    table.Append("<td>" + RP.M("Product", "Image") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "Price") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "Quantity") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "UnitPrice") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "DiscountDescription") + "</td>");
                    table.Append("<td>" + RP.M("OrderItem", "AttributeDescription") + "</td>");
                    table.Append("</tr>");
                    foreach(var item in _items)
                    {
                        table.AppendLine("<tr>");

                        table.Append("<td>" +(item.ProductShop!=null 
                            && item.ProductShop.Product!=null ? item.ProductShop.Product.Name : "")+ "</td>");
                        table.Append("<td>" + item.SKU + "</td>");
                        table.Append("<td>" + (item.ProductShop != null
                            && item.ProductShop.Product != null
                            ? "<img height=\"30px\" src=\"" + urlbase + SF.GetImage(item.ProductShop.Product.Image, 200, 200, true, true) + "\" alt=\"alt\" />" : "") + "</td>");
                        table.Append("<td>" + item.PriceStr + (item.QuantityType == QuantityType.Default
                           // && item.MeasureUnitStep.HasValue
                            && item.MeasureUnit != null ?
                             (" / " +  item.MeasureUnit)
                            : " / " + RP.S("View.Shop.ProductItem.PriceForItem")
                            ) + "</td>");
                        table.Append("<td>" + item.Quantity.ToString("0.00") + "</td>");
                        table.Append("<td>" + item.UnitPriceStr + "</td>");
                        table.Append("<td>" + item.DiscountDescription + "</td>");
                        table.Append("<td>" + item.AttributeDescription + "</td>");

                        table.Append("</tr>");
                    }

                    table.AppendLine("</table>");
                    _fullinfo = table.ToString();
                }
                return _fullinfo;// "<table><tr><td>" + RP.M("OrderItem", "SKU") + "</td></tr></table>";
            }
        }
    }
    #endregion

    [ModelGeneral(
       Role = "Admin", AjaxEdit = true, Edit = true, Create = true, CreateAjax = true)]
    [ModelGeneralAttribute(
        Role = "Member", Acl = true, Edit = true, AjaxEdit = true, CreateAjax = true, Delete = true, Show = true, DependedShow = true)]
  
   public partial class OrderItem
    {
       
        
       [HiddenInput(DisplayValue = false)]
       [Key]
       public int ID { get; set; }
       
        [Model(Show = false, Edit = false)]
        public int OrderID { get; set; }

       [Model(Show = false, Edit = false)]
       public int ProductShopID { get; set; }

          [Model(Filter = true)]
       public string AttributeDescription { get; set; }
          [Model(Filter = true)]
       public string SKU { get; set; }

       [NotMapped]
       [Model(Show = true, Edit = false, AjaxEdit = true)]
       public virtual ProductShop ProductShop { get; set; }

      // public int SelectedAttributeID { get; set; }
        [Model(Show = false, Edit = false)]
        public int ProductAttributeOptionID { get; set; }

        [NotMapped]
        [Model(Show = true, Edit = false, AjaxEdit = true)]
        public virtual ProductAttributeOption ProductAttributeOption { get; set; }
        

       public decimal Price { get;set; }
       [NotMapped] // for showing in view
       [Model(Show = false, Edit = false)]
       public string PriceStr
       {
           get
           {
               return ShoppingService.FormatPrice(Price);
           }
       }
       public decimal DiscountAmount { get; set; }

       public string DiscountAmountStr
       {
           get
           {
               return ShoppingService.FormatPrice(DiscountAmount);
           }
       }
       public string DiscountDescription { get; set; }
       public decimal Quantity { get; set; }
       public QuantityType QuantityType { get; set; }
         [Model(Show = false, Edit = true)]
       public string MeasureUnit { get; set; }
         [Model(Show = false, Edit = true)]
       public decimal? MeasureUnitStep { get; set; }
       public decimal UnitPrice { get; set; }
       [NotMapped] // for showing in view
       [Model(Show = false, Edit = false)]
       public string UnitPriceStr
       {
           get
           {
               return ShoppingService.FormatPrice(UnitPrice);
           }
       }
       [NotMapped]
       [Model(Show = false, Edit = false, AjaxEdit = false)]
       public virtual int VirtualOrder { get; set; }
       //[Display(Prompt = "Main")]
       //[Model(Show = false, Edit = false)]
       //public int OrderItemStatusID { get; set; }



       //[NotMapped]
       [Model(Show = true, Edit = true)]
       [Display(Prompt = "Main")]
       public OrderItemStatus OrderItemStatus { get; set; }

       #region Events
        public static Order FixOrderTotals(OrderItem item)
       {
           if (item.ID > 0)// && item.Price == 0 && item.UnitPrice == 0) _ DEPRECATED
           {
               var ps = LS.CurrentEntityContext.ProductShopMap.Where(x => x.ID == item.ProductShopID)
                 .Select(x => new { x.Price, x.ProductID })
                 .FirstOrDefault();
               if (ps != null)
               {
                   if (item.Price == 0)
                   {
                       item.Price = ps.Price;
                   }
                   var p = LS.CurrentEntityContext.Products.Where(x => x.ID == ps.ProductID).Select(x => x.SKU).FirstOrDefault();
                   item.SKU = p;
                   if (item.ProductAttributeOptionID > 0)
                   {
                       var attr = LS.CurrentEntityContext.ProductAttributeOptions
                           .FirstOrDefault(x => x.ID == item.ProductAttributeOptionID
                           && x.ProductShopID == item.ProductShopID);
                       if (attr != null)
                       {
                           if (attr.OverridenPrice.HasValue)
                           {
                               item.Price = attr.OverridenPrice.Value;
                           }
                           if (attr.OverridenSku != null)
                           {
                               item.SKU = attr.OverridenSku;
                           }
                       }
                   }
                   item.UnitPrice = item.Price * item.Quantity;
               }
               else
               {
                   item.UnitPrice = item.Price * item.Quantity;
               }
               LS.CurrentEntityContext.SaveChanges();
           }
           var order = LS.CurrentEntityContext.Orders.FirstOrDefault(x => x.ID == item.OrderID);
           if (order != null)
           {
               if(order.ShippingMethod == ShippingMethod.Manual)
               {
                   order.ShipCost = 0;
               }
               var itemsTotal = LS.CurrentEntityContext
                   .OrderItems.Where(x => x.OrderID == order.ID && x.OrderItemStatus != OrderItemStatus.OutOfStock)
                   .Select(x => x.UnitPrice).DefaultIfEmpty(0).Sum();
               order.SubTotal = itemsTotal;
               order.Total = order.SubTotal + order.ShipCost + order.Fee - order.TotalDiscountAmount;
               
           }
           LS.CurrentEntityContext.SaveChanges();
           return order;
       }
        public static Order OnCreated(OrderItem item)
       {
           return FixOrderTotals(item);
       }

        public static Order OnUpdated(OrderItem item)
        {
            return FixOrderTotals(item);
        }
        public static Order OnDeleted(OrderItem item)
        {
            item.ID = 0;
            return FixOrderTotals(item);
        }
       #endregion

       #region Access controll func
       public static bool AccessTest(OrderItem orderItem, ModelGeneralAttribute attr)
       {
           if (attr.Acl)
           {
               if (LS.CurrentShop != null)
               {
                   var productShop = LS.CurrentEntityContext.ProductShopMap.FirstOrDefault(x => x.ID == orderItem.ProductShopID);
                   if (productShop != null)
                   {
                       return productShop.ShopID == LS.CurrentShop.ID;
                   }
               }
               return false;
           };
           return true;
       }
       public static IQueryable<OrderItem> AccessList(ModelGeneralAttribute attr, Dictionary<string, object> param = null)
       {
           int ShopID = 0;
           //sort
           bool sortCategory = false;
           if (param != null && param.ContainsKey("_SortVariants"))
           {
               var sortVariants = param["_SortVariants"] as List<JsonKeyValue>;
               if (sortVariants.Count > 0 && sortVariants.FirstOrDefault().Name == "ProductShop.OrderPosition")
               {
                   sortCategory = true;
               }
           }
           if (param != null && param.ContainsKey("ShopID"))
           {
               ShopID = (int)param["ShopID"];
           }
           else if (attr.Acl)
           {
               ShopID = LS.CurrentShop.ID;
           }
           else
           {
               if(sortCategory)
               {
                   return (
                  from oi in LS.CurrentEntityContext.OrderItems
                  from ps in LS.CurrentEntityContext.ProductShopMap.Where(x => x.ID == oi.ProductShopID
                   ).DefaultIfEmpty()
                  orderby ps.OrderPosition
                  select oi).Distinct();
               }

               return (
                  from oi in LS.CurrentEntityContext.OrderItems
                  select oi);
           }

           if (sortCategory)
           {
               return (
              from oi in LS.CurrentEntityContext.OrderItems
              from ps in LS.CurrentEntityContext.ProductShopMap.Where(x => x.ID == oi.ProductShopID
                   && x.ShopID == ShopID).DefaultIfEmpty()
              orderby ps.OrderPosition
              select oi).Distinct();
           }

           return (
              from oi in LS.CurrentEntityContext.OrderItems
              from ps in LS.CurrentEntityContext.ProductShopMap.Where(x=>x.ID == oi.ProductShopID 
                  && x.ShopID == ShopID).DefaultIfEmpty()
              //on oi.ProductShopID equals ps.ID
             // where ps.ShopID == ShopID
              select oi).Distinct();
       }
        #endregion

    }
    
}