using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Builder;

namespace Microsoft.AspNetCore.OData.Extensions
{
	internal static class PropertyInfoExtensions
	{
		internal static PropertyConfiguration GetConfiguration(this PropertyInfo property,
			IEnumerable<IEdmTypeConfiguration> configurations)
		{
			return property.GetConfiguration(configurations.ToArray());
		}

		internal static PropertyConfiguration GetConfiguration(this PropertyInfo property, params IEdmTypeConfiguration[] configurations)
		{
			foreach (var config in configurations)
			{
				if (config.ClrType == property.DeclaringType)
				{
					return (config as StructuralTypeConfiguration).AddProperty(property);
				}
			}
			return null;
		}
	}
}