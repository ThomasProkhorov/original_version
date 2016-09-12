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
using Uco.Infrastructure.Repositories;
using System.IO;
using System.Text;

using Uco.Infrastructure.Livecycle;
using System.Threading.Tasks;
using System.Globalization;
using Uco.Infrastructure.EntityExtensions;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExportToExcel;


namespace Uco.Areas.Member.Controllers
{
    [Authorize(Roles = "Member")]
    public class ProductController : BaseMemberController
    {


        #region autocomplite
        public ActionResult _AjaxDetailAutoComplete([DataSourceRequest]DataSourceRequest request, string text)
        {
            var items = LS.Get<ProductSmall>(
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


            if (!string.IsNullOrEmpty(text))
            {
                items = items.Where(x => (x.SKU != null && x.SKU.Contains(text))
                     || (x.Name != null && x.Name.Contains(text))
                    );

            }
            request.PageSize = 100;
            DataSourceResult result = items.ToDataSourceResult(request);
            return Json(result);
        }
        #endregion




    }
}
