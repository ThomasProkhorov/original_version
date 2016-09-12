using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Repositories;
using Uco.Models.Overview;

namespace Uco.Infrastructure.Helpers
{
    public static class ProductHelper
    {
        public static void PrepareContentUnitMeasure(this ProductOverviewModel model)
        {
            if (model.ContentUnitPriceMultiplicator > 0 && model.ContentUnitMeasureID > 0)
            {
                var contentUnitMeasure = RP.GetContentUnitMeasureById(model.ContentUnitMeasureID);
                model.ContentUnitName = contentUnitMeasure.DisplayName;
                model.ContentUnitPrice = Math.Truncate(model.Price * model.ContentUnitPriceMultiplicator * 100) / 100;
            }
        }

        public static void PrepareContentUnitMeasures(this IEnumerable<ProductOverviewModel> models)
        {
            
            foreach (var model in models)
	        {
                model.PrepareContentUnitMeasure();
	        }
        }
    }
}