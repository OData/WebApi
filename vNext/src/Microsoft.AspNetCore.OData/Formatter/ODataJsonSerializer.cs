using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter
{
	public class ODataJsonSerializer
	{
		public ODataMessageWriterSettings MessageWriterSettings { get; set; }
		private readonly OutputFormatterWriteContext _context;
		private readonly ODataProperties _odataProperties;
		private readonly ODataVersion _version;
		private HttpRequest Request => _context.HttpContext.Request;
		private HttpResponse Response => _context.HttpContext.Response;

		public ODataJsonSerializer(OutputFormatterWriteContext context)
		{
			_context = context;
			_odataProperties = context.HttpContext.ODataProperties();
			_version = ODataProperties.DefaultODataVersion;
			MessageWriterSettings = new ODataMessageWriterSettings
			{
				Indent = true,
				DisableMessageStreamDisposal = true,
				MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
				AutoComputePayloadMetadataInJson = true,
			};
		}

		public async Task WriteJson(object value, Stream writeStream)
		{
			IEdmModel model = _odataProperties.Model;
			if (model == null)
			{
				throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
			}

			var type = value.GetType();
			type = _context.ObjectType;

			var urlHelper = UrlHelper(_context.HttpContext);
			ODataPath path = Request.ODataProperties().Path;
			IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

			// serialize a response
			//HttpConfiguration configuration = Request.GetConfiguration();
			//if (configuration == null)
			//{
			//	throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
			//}

			// TODO: Fix this ffs...

			string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(Request);
			string annotationFilter = null;

			if (ODataCountMediaTypeMapping.IsCountRequest(Request))
			{
				Response.ContentType = "text/plain";
			}

			if (!String.IsNullOrEmpty(preferHeader))
			{
				ODataMessageWrapper messageWrapper = new ODataMessageWrapper(writeStream, Response.Headers);
				messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
				annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
			}

			IODataResponseMessage responseMessage = new ODataMessageWrapper(writeStream, Response.Headers);
			if (annotationFilter != null)
			{
				responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
			}

			Uri baseAddress = GetBaseAddress();
			ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings(MessageWriterSettings)
			{
				PayloadBaseUri = baseAddress,
				Version = _version,
			};

			writerSettings.ODataUri = new ODataUri
			{
				ServiceRoot = baseAddress,

				// TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
				SelectAndExpand = Request.ODataProperties().SelectExpandClause,
				// TODO: Support $apply
				//Apply = Request.ODataProperties().ApplyClause,
				Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
			};

			MediaTypeHeaderValue contentType = null;
			// TODO: Restore
			//if (contentHeaders != null && contentHeaders.ContentType != null)
			//{
			//	contentType = contentHeaders.ContentType;
			//}

			using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
			{
				ODataSerializer serializer = GetSerializer(type, value, model, _context.HttpContext.RequestServices.GetService<ODataSerializerProvider>());
				ODataSerializerContext writeContext = new ODataSerializerContext()
				{
					Request = Request,
					RequestContext = _context.HttpContext,
					Url = urlHelper,
					NavigationSource = targetNavigationSource,
					Model = model,
					RootElementName = GetRootElementName(path) ?? "root",
					SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.Feed,
					Path = path,
					MetadataLevel = ODataMediaTypes.GetMetadataLevel(contentType),
					SelectExpandClause = Request.ODataProperties().SelectExpandClause
				};

				await serializer.WriteObject(value, type, messageWriter, writeContext);
			}

		}

		private IUrlHelper UrlHelper(HttpContext httpContext)
		{
			//return httpContext.UrlHelper();
			return new UrlHelper(new ActionContext(httpContext,new RouteData(), new ActionDescriptor()));
		}

		private static bool IsOperationPath(ODataPath path)
		{
			if (path == null)
			{
				return false;
			}

			foreach (ODataPathSegment segment in path.Segments)
			{
				switch (segment.SegmentKind)
				{
					case ODataSegmentKinds._Action:
					case ODataSegmentKinds._Function:
					case ODataSegmentKinds._UnboundAction:
					case ODataSegmentKinds._UnboundFunction:
						return true;
				}
			}

			return false;
		}
		private static string GetRootElementName(ODataPath path)
		{
			if (path != null)
			{
				ODataPathSegment lastSegment = path.Segments.LastOrDefault();
				if (lastSegment != null)
				{
					BoundActionPathSegment actionSegment = lastSegment as BoundActionPathSegment;
					if (actionSegment != null)
					{
						return actionSegment.Action.Name;
					}

					PropertyAccessPathSegment propertyAccessSegment = lastSegment as PropertyAccessPathSegment;
					if (propertyAccessSegment != null)
					{
						return propertyAccessSegment.Property.Name;
					}
				}
			}
			return null;
		}

		private Uri GetBaseAddress()
		{
			//var urlHelper = UrlHelper(_context.HttpContext);

			var uri = 
				new Uri(_context.HttpContext.Request.GetDisplayUrl());
			var baseAddress =
				uri.Scheme + "://" + uri.Host + (uri.Port == 80 ? "" : ":" + uri.Port) + "/" + ODataRoute.Instance.RoutePrefix;

			return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
		}

		private ODataSerializer GetSerializer(Type type, object value, IEdmModel model, ODataSerializerProvider serializerProvider)
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
					string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString(), typeof(ODataJsonSerializer).Name);
					throw new SerializationException(message);
				}
			}
			else
			{
				// TODO: Support $apply
				// Currently don't support $apply
				//var applyClause = Request.ODataProperties().ApplyClause;
				//// get the most appropriate serializer given that we support inheritance.
				//if (applyClause == null)
				//{
				//	type = value == null ? type : value.GetType();
				//}

				serializer = serializerProvider.GetODataPayloadSerializer(model, type, Request);
				if (serializer == null)
				{
					string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataJsonSerializer).Name);
					throw new SerializationException(message);
				}
			}

			return serializer;
		}

	}
}