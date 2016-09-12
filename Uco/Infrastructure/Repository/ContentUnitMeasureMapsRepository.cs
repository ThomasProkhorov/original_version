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

        public static void CleanContentUnitMeasureMapRepository()
        {
            LS.Clear<ContentUnitMeasureMap>();
        }

        public static List<ContentUnitMeasureMap> GetContentUnitMeasureMaps()
        {
            return LS.Get<ContentUnitMeasureMap>();
        }

        public static ContentUnitMeasureMap GetContentUnitMeasureMapById(int ID)
        {
            return GetContentUnitMeasureMaps().FirstOrDefault(c => c.ID == ID);
        }
        

    }
}