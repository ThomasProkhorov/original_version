using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Uco.Infrastructure.Livecycle;
using Uco.Models;
using Uco.Models.Shopping.Measure;

namespace Uco.Infrastructure.Repositories
{
    public static partial class RP
    {

        public static void CleanContentUnitMeasureRepository()
        {
            LS.Clear<ContentUnitMeasure>();
        }

        public static List<ContentUnitMeasure> GetContentUnitMeasures()
        {
            return LS.Get<ContentUnitMeasure>();
        }

        public static ContentUnitMeasure GetContentUnitMeasureById(int ID)
        {
            return GetContentUnitMeasures().FirstOrDefault(c => c.ID == ID);
        }
        

    }
}