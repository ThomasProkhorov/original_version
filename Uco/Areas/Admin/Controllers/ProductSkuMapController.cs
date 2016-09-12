using Kendo.Mvc;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Models;
using Uco.Infrastructure.EntityExtensions;
using ExportToExcel;
using Kendo.Mvc.Extensions;
using Uco.Infrastructure.Livecycle;

namespace Uco.Areas.Admin.Controllers
{

    [AuthorizeAdmin]
    public class ProductSkuMapController : BaseAdminController
    {


        #region  export
        public ActionResult CSVExport([DataSourceRequest]DataSourceRequest request)
        {
            var filtered = _db.ProductSkuMaps.AsQueryable().ToDataSourceResult(request);
            var shops = LS.Get<Shop>();
            var products = LS.Get<ProductSmall>(
              "ProductSmall", false, () =>
              {
                  return _db.Products.AsNoTracking()
                      .Select(x => new ProductSmall()
                      {
                          ID = x.ID,
                          Image = x.Image,
                          Name = x.Name,
                          SKU = x.SKU
                      })
                      .ToList();
              },
              "Product"
              ).AsQueryable();
            var neededData = (filtered.Data as IEnumerable<ProductSkuMap>)
                .Select(x => new
                {
                    ProductName = products.Any(y => y.SKU == x.ProductSKU) ? products.FirstOrDefault(y => y.SKU == x.ProductSKU).Name : "",
                    x.ImportProductName,
                    x.Imported,

                    x.ShortSKU,
                    x.ProductSKU,
                    x.Price,
                    x.Quantity,
                    x.ShopID,
                    ShopName = shops.Any(y => y.ID == x.ShopID) ? shops.FirstOrDefault(y => y.ID == x.ShopID).Name : ""

                })

                .ToList();
            byte[] bytes = null;
            bytes = CreateExcelFile.CreateExcelDocument(
                    neededData, "ProductSkuMaps_" + DateTime.Now.ToString() + ".xlsx"
                    , HttpContext.Response);

            return File(bytes
                , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                , "ProductSkuMaps_" + DateTime.Now.ToString() + ".xlsx");
        }
        #endregion
    }
}