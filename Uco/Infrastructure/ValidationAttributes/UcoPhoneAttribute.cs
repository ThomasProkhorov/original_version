using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class UcoPhoneAttribute : RegularExpressionAttribute
    {
        public UcoPhoneAttribute()
            : base(GetRegex())
        { }

        private static string GetRegex()
        {
            return @"^[0-9 ()*-]*$";
        }
    }
}