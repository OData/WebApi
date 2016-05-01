// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
	/// <summary>
	/// Represents an <see cref="ODataSerializer"/> that serializes instances of objects backed by an <see cref="IEdmType"/>.
	/// </summary>
	public abstract class ODataEdmTypeSerializer : ODataSerializer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
		/// </summary>
		/// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
		protected ODataEdmTypeSerializer(ODataPayloadKind payloadKind)
			: base(payloadKind)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
		/// </summary>
		/// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
		/// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write inner objects.</param>
		protected ODataEdmTypeSerializer(ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)
			: this(payloadKind)
		{
			if (serializerProvider == null)
			{
				throw Error.ArgumentNull("serializerProvider");
			}

			SerializerProvider = serializerProvider;
		}

		/// <summary>
		/// Gets the <see cref="ODataSerializerProvider"/> that can be used to write inner objects.
		/// </summary>
		public ODataSerializerProvider SerializerProvider { get; private set; }

		/// <summary>
		/// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
		/// messageWriter and the writeContext.
		/// </summary>
		/// <param name="graph">The object to be written.</param>
		/// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
		/// <param name="writer">The <see cref="ODataWriter" /> to be used for writing.</param>
		/// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
		public virtual async Task WriteObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer,
			ODataSerializerContext writeContext)
		{
			throw Error.NotSupported(SRResources.WriteObjectInlineNotSupported, GetType().Name);
		}

		/// <summary>
		/// Creates an <see cref="ODataValue"/> for the object represented by <paramref name="graph"/>.
		/// </summary>
		/// <param name="graph">The value of the <see cref="ODataValue"/> to be created.</param>
		/// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
		/// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
		/// <returns>The <see cref="ODataValue"/> created.</returns>
		public virtual async Task<ODataValue> CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
		{
			throw Error.NotSupported(SRResources.CreateODataValueNotSupported, GetType().Name);
		}

		internal async Task<ODataProperty> CreateProperty(object graph, IEdmTypeReference expectedType, string elementName,
			ODataSerializerContext writeContext, EntityInstanceContext entityInstanceContext)
		{
			return await CreateProperty(graph, expectedType, elementName, writeContext, entityInstanceContext.EntityInstance,
				null, entityInstanceContext.EntityType, null);
		}

		internal async Task<ODataProperty> CreateProperty(object graph, IEdmTypeReference expectedType, string elementName,
			ODataSerializerContext writeContext)
		{
			return await CreateProperty(graph, expectedType, elementName, writeContext, null,
				null, null, null);
		}

		private async Task<ODataProperty> CreateProperty(object graph, IEdmTypeReference expectedType, string elementName,
			ODataSerializerContext writeContext, object declaringInstance,
			IEdmTypeReference declaringTypeReference,
			IEdmType declaringType,
			Type declaringClrType)
		{
			Contract.Assert(elementName != null);
			graph = InterceptValue(graph, expectedType, elementName, writeContext, declaringInstance, declaringTypeReference, declaringType, declaringClrType);
			return new ODataProperty
			{
				Name = elementName,
				Value = await CreateODataValue(graph, expectedType, writeContext)
			};
		}

		private static object InterceptValue(object graph, IEdmTypeReference expectedType, string elementName,
			ODataSerializerContext writeContext, object declaringInstance, IEdmTypeReference declaringTypeReference,
			IEdmType declaringType, Type declaringClrType)
		{
			var modelBuilder = EdmModelHelperMethods.Configuration[writeContext.Model];
			var assemblyNames = writeContext.Request.HttpContext.RequestServices.GetService<AssembliesResolver>();
			var clrType = EdmLibHelpers.GetClrType(expectedType, writeContext.Model, assemblyNames);
			var serializers = modelBuilder.GetSerializeInterceptors(
				clrType).ToList();
			var isRoot = declaringInstance == null &&
						 declaringTypeReference == null &&
						 declaringType == null &&
						 declaringClrType == null;

			if (serializers.Any())
			{
				if (declaringClrType == null && declaringTypeReference != null)
				{
					declaringClrType = declaringType != null
						? EdmLibHelpers.GetClrType(declaringType, writeContext.Model, assemblyNames)
						: EdmLibHelpers.GetClrType(declaringTypeReference, writeContext.Model, assemblyNames);
				}
				var vp = new ValueInterceptor(
					graph, clrType, elementName, declaringInstance, declaringClrType, isRoot);
				foreach (var serializer in serializers)
				{
					if (!serializer.Process(vp))
					{
						break;
					}
				}
				graph = vp.Value;
			}
			return graph;
		}

		internal async Task<List<ODataProperty>> AppendDynamicProperties(object source, IEdmStructuredTypeReference structuredType,
			ODataSerializerContext writeContext, List<ODataProperty> declaredProperties,
			string[] selectedDynamicProperties)
		{
			Contract.Assert(source != null);
			Contract.Assert(structuredType != null);
			Contract.Assert(writeContext != null);
			Contract.Assert(writeContext.Model != null);

			PropertyInfo dynamicPropertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(
				structuredType.StructuredDefinition(), writeContext.Model);

			IEdmStructuredObject structuredObject = source as IEdmStructuredObject;
			object value;
			IDelta delta = source as IDelta;
			if (delta == null)
			{
				if (dynamicPropertyInfo == null || structuredObject == null ||
					!structuredObject.TryGetPropertyValue(dynamicPropertyInfo.Name, out value) || value == null)
				{
					return null;
				}
			}
			else
			{
				value = ((EdmStructuredObject)structuredObject).TryGetDynamicProperties();
			}

			IDictionary<string, object> dynamicPropertyDictionary = (IDictionary<string, object>)value;

			// Build a HashSet to store the declared property names.
			// It is used to make sure the dynamic property name is different from all declared property names.
			HashSet<string> declaredPropertyNameSet = new HashSet<string>(declaredProperties.Select(p => p.Name));
			List<ODataProperty> dynamicProperties = new List<ODataProperty>();
			IEnumerable<KeyValuePair<string, object>> dynamicPropertiesToSelect =
				dynamicPropertyDictionary.Where(
					x => !selectedDynamicProperties.Any() || selectedDynamicProperties.Contains(x.Key));
			foreach (KeyValuePair<string, object> dynamicProperty in dynamicPropertiesToSelect)
			{
				if (String.IsNullOrEmpty(dynamicProperty.Key) || dynamicProperty.Value == null)
				{
					continue; // skip the null object
				}

				if (declaredPropertyNameSet.Contains(dynamicProperty.Key))
				{
					throw Error.InvalidOperation(SRResources.DynamicPropertyNameAlreadyUsedAsDeclaredPropertyName,
						dynamicProperty.Key, structuredType.FullName());
				}

				IEdmTypeReference edmTypeReference = writeContext.GetEdmType(dynamicProperty.Value,
					dynamicProperty.Value.GetType());
				if (edmTypeReference == null)
				{
					throw Error.NotSupported(SRResources.TypeOfDynamicPropertyNotSupported,
						dynamicProperty.Value.GetType().FullName, dynamicProperty.Key);
				}

				ODataEdmTypeSerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);
				if (propertySerializer == null)
				{
					throw Error.NotSupported(SRResources.DynamicPropertyCannotBeSerialized, dynamicProperty.Key,
						edmTypeReference.FullName());
				}

				dynamicProperties.Add(await propertySerializer.CreateProperty(
					dynamicProperty.Value, edmTypeReference, dynamicProperty.Key, writeContext, source, structuredType, null, null));
			}

			return dynamicProperties;
		}
	}
}
