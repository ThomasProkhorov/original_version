using System;
using System.Linq.Expressions;

namespace Uco.Infrastructure
{
    public class PageNameAttribute : Attribute
    {
        public string PageName { get; set; }
        public Type ResourceType { get; set; }
        public PageNameAttribute(string pageName) { PageName = pageName; }
        public PageNameAttribute(string pageName, Type resourceType) { PageName = pageName; ResourceType = resourceType; }
    }

   
}