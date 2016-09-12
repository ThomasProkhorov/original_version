using System;

namespace Uco.Infrastructure
{
    public class RestrictParentsAttribute : Attribute
    {
        public string[] Parents { get; set; }
        public RestrictParentsAttribute(string[] parents) { Parents = parents; }
    }
}