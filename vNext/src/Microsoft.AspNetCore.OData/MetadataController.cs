using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.Framework.Internal;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using System.Linq;

namespace Microsoft.AspNetCore.OData
{
	[ApiExplorerSettings(IgnoreApi = true)]
	//[Route("odata")]
	public class MetadataController : Controller
	{
		private readonly IEdmModel _model;

		public MetadataController([NotNull]ODataProperties odataProperties)
		{
			_model = odataProperties.Model;
		}

		// not work: public IEdmModel Get => this._model;
		[HttpGet("$metadata")]
		public IEdmModel Get()
		{
			return _model;
		}

		/// <summary>
		/// Generates the OData service document.
		/// </summary>
		/// <returns>The service document for the service.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
			Justification = "Property not appropriate")]
		[HttpGet]
		public ODataServiceDocument GetServiceDocument()
		{
			IEdmModel model = Get();
			ODataServiceDocument serviceDocument = new ODataServiceDocument();
			IEdmEntityContainer container = model.EntityContainer;

			// Add EntitySets into service document
			serviceDocument.EntitySets = container.EntitySets().Select(
				e => GetODataEntitySetInfo(model.GetNavigationSourceUrl(e).ToString(), e.Name));

			// Add Singletons into the service document
			IEnumerable<IEdmSingleton> singletons = container.Elements.OfType<IEdmSingleton>();
			serviceDocument.Singletons = singletons.Select(
				e => GetODataSingletonInfo(model.GetNavigationSourceUrl(e).ToString(), e.Name));

			// Add FunctionImports into service document
			// ODL spec says:
			// The edm:FunctionImport for a parameterless function MAY include the IncludeInServiceDocument attribute
			// whose Boolean value indicates whether the function import is advertised in the service document.
			// If no value is specified for this attribute, its value defaults to false.

			// Find all parameterless functions with "IncludeInServiceDocument = true"
			IEnumerable<IEdmFunctionImport> functionImports = container.Elements.OfType<IEdmFunctionImport>()
				.Where(f => !f.Function.Parameters.Any() && f.IncludeInServiceDocument);

			serviceDocument.FunctionImports = functionImports.Distinct(new FunctionImportComparer())
				.Select(f => GetODataFunctionImportInfo(f.Name));

			return serviceDocument;
		}

		private static ODataEntitySetInfo GetODataEntitySetInfo(string url, string name)
		{
			ODataEntitySetInfo info = new ODataEntitySetInfo
			{
				Name = name, // Required for JSON support
				Url = new Uri(url, UriKind.Relative)
			};

			return info;
		}

		private static ODataSingletonInfo GetODataSingletonInfo(string url, string name)
		{
			ODataSingletonInfo info = new ODataSingletonInfo
			{
				Name = name,
				Url = new Uri(url, UriKind.Relative)
			};

			return info;
		}

		private static ODataFunctionImportInfo GetODataFunctionImportInfo(string name)
		{
			ODataFunctionImportInfo info = new ODataFunctionImportInfo
			{
				Name = name,
				Url = new Uri(name, UriKind.Relative) // Relative to the OData root
			};

			return info;
		}
	}
}