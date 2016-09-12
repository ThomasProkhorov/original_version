using System.Web.Mvc;
using Uco.Infrastructure;

namespace Uco
{
    public class AttributeConfig
    {
        public static void RegisterGlobalAttribute()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(UcoEmailAttribute), typeof(RegularExpressionAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(UcoPhoneAttribute), typeof(RegularExpressionAttributeAdapter));
        }
    }
}
