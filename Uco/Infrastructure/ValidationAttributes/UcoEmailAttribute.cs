using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Uco.Infrastructure
{
    public class UcoEmailAttribute : RegularExpressionAttribute
    {
        public UcoEmailAttribute()
            : base(GetRegex())
        { }

        private static string GetRegex()
        {
            return @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        }
    }
}