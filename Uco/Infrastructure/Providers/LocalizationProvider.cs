using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uco.Infrastructure.Repositories;
using Uco.Models;
using System.Reflection;

namespace Uco.Infrastructure.Providers
{
    public class MyLocalizationProvider : DataAnnotationsModelMetadataProvider
    {

        //private static UiResources ui = new UiResources(); // database class
        protected override ModelMetadata CreateMetadata(
                             IEnumerable<Attribute> attributes,
                             Type containerType,
                             Func<object> modelAccessor,
                             Type modelType,
                             string propertyName)
        {
            ModelMetadata baseres = null;
            try
            {
                baseres = base.CreateMetadata
                 (attributes, containerType, modelAccessor, modelType, propertyName);
               
            }
            catch (Exception e)
            {
                baseres = base.CreateMetadata
                  (new List<Attribute>(), containerType, modelAccessor, modelType, propertyName);
            }
            string sKey = string.Empty; //  ощощ мчбмъ дтшк щм displayname
            string tab = string.Empty;
            string sLocalizedText = string.Empty; // ощъощ мчбм тшк оъешвн
           
            HttpContext.Current.Application.Lock();
            string keyType = "";
            if (containerType!=null)
            {
                keyType = containerType.Name;
            if(containerType.BaseType!= null && containerType.BaseType == typeof(AbstractPage))
            {

                keyType = containerType.BaseType.Name;
            }
            }
            bool? hideTab = null;
            string currole = null;
            if (HttpContext.Current != null)
            {
                var controller = HttpContext.Current.Items["controllerInstance"] as BaseController;
                if (controller != null)
                {
                    currole = controller.ViewBag.CurrentRole;
                }
            }
            if (containerType != null && propertyName != null)
            {
                var modelAttr = containerType.GetProperty(propertyName).GetCustomAttributes<ModelAttribute>()
                    .Where(x => x.Role == currole || x.Role == null).OrderByDescending(x => x.Role).ToList().FirstOrDefault();

                if (modelAttr != null)
                {
                    if (!hideTab.HasValue && !modelAttr.ShowInEdit && !modelAttr.Edit)
                    {
                        hideTab = true;
                    }
                }
            }
            foreach (var attr in attributes)
            {
                if (attr != null)
                {
                    string typeName = attr.GetType().Name; // ощйв аъ сев дтшк мгевоа DisplayAttribute
                    string attrAppKey = string.Empty;
                    
                    if (typeName.Equals("DisplayAttribute"))
                    {
                        sKey = ((DisplayAttribute)attr).Name;
                       var  tabKey = ((DisplayAttribute)attr).Prompt;
                       //if (tabKey != null && tabKey.StartsWith("Tab"))
                       //{
                       //    tabKey = tabKey.Substring("Tab".Length);
                       //}

                       
                        if (!string.IsNullOrEmpty(tabKey))
                        {
                            // attrAppKey = string.Format("{0}-{1}-{2}", containerType.Name, propertyName, typeName);

                            if (RP.Mbool(out tabKey, keyType, ((DisplayAttribute)attr).Prompt, "Prompt"))
                            {
                               // rewriteTab = true;
                                baseres.Watermark = tabKey;
                                //((DisplayAttribute)attr).ResourceType = null;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(tabKey) && tabKey != baseres.Watermark)
                                {
                                    RP.Madd(keyType, tabKey, "Prompt", baseres.Watermark);
                                }
                            }

                        }
                    }
                    //else if (attr is ValidationAttribute)
                    //{
                    //    sKey = ((ValidationAttribute)attr).ErrorMessage;

                    //    if (!string.IsNullOrEmpty(sKey))
                    //    {
                    //        attrAppKey = string.Format("{0}-{1}-{2}", containerType.Name, propertyName, typeName);


                    //        ((ValidationAttribute)attr).ErrorMessage = sKey;
                    //    }
                    //}
                    else if (attr is ValidationAttribute)
                    {
                        sKey = ((ValidationAttribute)attr).ErrorMessage;

                        if (!string.IsNullOrEmpty(sKey))
                        {
                            attrAppKey = string.Format("{0}-{1}-{2}",
                            keyType, propertyName, typeName);
                            if (HttpContext.Current.Application[attrAppKey] == null)
                            {
                                HttpContext.Current.Application[attrAppKey] = sKey;
                            }
                            else
                            {
                                sKey = HttpContext.Current.Application[attrAppKey].ToString();
                            }

                            sLocalizedText = RP.T(sKey).ToString();
                            if (string.IsNullOrEmpty(sLocalizedText))
                            {
                                sLocalizedText = sKey;
                            }

                            ((ValidationAttribute)attr).ErrorMessage = sLocalizedText;
                        }
                    }
                }
            }


            if (!string.IsNullOrEmpty(sKey))
            {
                // attrAppKey = string.Format("{0}-{1}-{2}", containerType.Name, propertyName, typeName);

                if (RP.Mbool(out sKey, keyType, propertyName))
                {
                    // rewriteName = true;
                    baseres.DisplayName = sKey;
                    //((DisplayAttribute)attr).ResourceType = null;
                }
                // else
                // {

                if (!string.IsNullOrEmpty(sKey)
                    && sKey != baseres.DisplayName
                    //&& ((DisplayAttribute)attr).Name != baseres.DisplayName //update if different
                    && sKey.Contains(".")) // update if it auto generated
                {
                    RP.Madd(keyType, propertyName, "", baseres.DisplayName);
                }

                // }

            }
            else if (containerType != null && keyType != null && propertyName != null)
            {
                baseres.DisplayName = RP.M(keyType, propertyName);

            }
            if (hideTab.HasValue && hideTab.Value && baseres.Watermark!=null)
            {
                baseres.Watermark = null;
            }
            return baseres;
        }
    }

    public class MyLocalizationProviderTest : DataAnnotationsModelMetadataProvider
    {
        //private static UiResources ui = new UiResources();
        protected override ModelMetadata CreateMetadata(
                             IEnumerable<Attribute> attributes,
                             Type containerType,
                             Func<object> modelAccessor,
                             Type modelType,
                             string propertyName)
        {

            string sKey = string.Empty;
            string sLocalizedText = string.Empty;

            HttpContext.Current.Application.Lock();
            foreach (var attr in attributes)
            {
                if (attr != null)
                {
                    string typeName = attr.GetType().Name;
                    string attrAppKey = string.Empty;

                    if (typeName.Equals("DisplayAttribute"))
                    {
                        sKey = ((DisplayAttribute)attr).Name;

                        if (!string.IsNullOrEmpty(sKey))
                        {
                            attrAppKey = string.Format("{0}-{1}-{2}",
                            containerType.Name, propertyName, typeName);
                            if (HttpContext.Current.Application[attrAppKey] == null)
                            {
                                HttpContext.Current.Application[attrAppKey] = sKey;
                            }
                            else
                            {
                                sKey = HttpContext.Current.Application[attrAppKey].ToString();
                            }

                            sLocalizedText = RP.T(sKey).ToString();
                            if (string.IsNullOrEmpty(sLocalizedText))
                            {
                                sLocalizedText = sKey;
                            }

                            ((DisplayAttribute)attr).Name = sLocalizedText;
                        }
                    }
                    else if (attr is ValidationAttribute)
                    {
                        sKey = ((ValidationAttribute)attr).ErrorMessage;

                        if (!string.IsNullOrEmpty(sKey))
                        {
                            attrAppKey = string.Format("{0}-{1}-{2}",
                            containerType.Name, propertyName, typeName);
                            if (HttpContext.Current.Application[attrAppKey] == null)
                            {
                                HttpContext.Current.Application[attrAppKey] = sKey;
                            }
                            else
                            {
                                sKey = HttpContext.Current.Application[attrAppKey].ToString();
                            }

                            sLocalizedText = RP.T(sKey).ToString();
                            if (string.IsNullOrEmpty(sLocalizedText))
                            {
                                sLocalizedText = sKey;
                            }

                            ((ValidationAttribute)attr).ErrorMessage = sLocalizedText;
                        }
                    }
                }
            }
            HttpContext.Current.Application.UnLock();

            return base.CreateMetadata
              (attributes, containerType, modelAccessor, modelType, propertyName);
        }
    }

}