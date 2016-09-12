using System;
using System.Linq.Expressions;

namespace Uco.Infrastructure
{
    //mail template usage
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MailTemplateAttribute : Attribute
    {
        public MailTemplateAttribute(string TemplateName)
        {
            TemplateSystemName = TemplateName;
        }
        public string TemplateSystemName { get; set; }
    }

   
}