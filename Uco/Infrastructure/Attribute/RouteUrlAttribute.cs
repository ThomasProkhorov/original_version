using System;

namespace Uco.Infrastructure
{
    public class RouteUrlAttribute : Attribute
    {
        public string RouteUrl { get; set; }
        public RouteUrlAttribute(string routeurl) { RouteUrl = routeurl; }
    }
}