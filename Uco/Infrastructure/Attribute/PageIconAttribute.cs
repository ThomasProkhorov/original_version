using System;

namespace Uco.Infrastructure
{
    public class PageIconAttribute : Attribute
    {
        public string PageIcon { get; set; }
        public PageIconAttribute(string pageIcon) { PageIcon = pageIcon; }
    }
}