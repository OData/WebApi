using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;
using Microsoft.OData.Core;

namespace Microsoft.AspNet.OData.Formatter
{
    public class ODataOutputFormatter : OutputFormatter
    {
        public ODataOutputFormatter()
        {
        }

        public override Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            HttpRequest request = context.ActionContext.HttpContext.Request;

            IEdmModel model = request.ODataProperties().Model;
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            object value = context.Object;
            Type type = value.GetType();
            ODataSerializer serializer = GetSerializer(type, value, model, new DefaultODataSerializerProvider(), request);

            IUrlHelper urlHelper = context.ActionContext.HttpContext.UrlHelper();

            ODataPath path = request.ODataProperties().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            throw new NotImplementedException("WriteResponseBodyAsync");
        }

        public override void WriteResponseHeaders(OutputFormatterContext context)
        {
            
        }

        private ODataSerializer GetSerializer(Type type, object value, IEdmModel model, ODataSerializerProvider serializerProvider, HttpRequest request)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException(Error.Format(SRResources.EdmTypeCannotBeNull,
                        edmObject.GetType().FullName, typeof(IEdmObject).Name));
                }

                serializer = serializerProvider.GetEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString(), typeof(ODataOutputFormatter).Name);
                    throw new SerializationException(message);
                }
            }
            else
            {
                // get the most appropriate serializer given that we support inheritance.
                type = value == null ? type : value.GetType();
                serializer = serializerProvider.GetODataPayloadSerializer(model, type, request);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataOutputFormatter).Name);
                    throw new SerializationException(message);
                }
            }

            return serializer;
        }

    }
}