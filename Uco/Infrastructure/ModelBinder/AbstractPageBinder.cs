using System;
using System.Web.Mvc;
using Uco.Infrastructure;
using Uco.Infrastructure.Repositories;

namespace Uco.Models
{

    public class AbstractPageBinder : DefaultModelBinder
    {
        protected override object CreateModel(
            ControllerContext controllerContext,
            ModelBindingContext bindingContext,
            Type modelType)
        {
            ValueProviderResult result;
            result = bindingContext.ValueProvider.GetValue("RouteUrl");
            if (result == null) throw new NotImplementedException("RouteUrl must be specified");

            foreach (Type item in RP.GetPageTypesReprository())
            {
                if (result.AttemptedValue == SF.GetTypeRouteUrl(item))
                {
                    var model = Activator.CreateInstance(item);
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, item);
                    return model;
                }
            }
            
            throw new NotImplementedException("RouteUrl must be specified");
        }
    }


}