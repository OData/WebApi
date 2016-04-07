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

		internal static bool IsIgnored(this PropertyInfo property,
			params IEdmTypeConfiguration[] configurations)
		{
			var config = property.GetConfiguration(configurations);
			if (config != null)
			{
				return config.IsIgnored;
			}
			return true;
		}

		internal static PropertyConfiguration GetConfiguration(this PropertyInfo property, params IEdmTypeConfiguration[] configurations)
		{
			foreach (var config in configurations)
			{
				if (config.ClrType == property.DeclaringType)
				{
					var structuralTypeConfiguration = config as StructuralTypeConfiguration;
					if (structuralTypeConfiguration != null && structuralTypeConfiguration.ExplicitProperties.ContainsKey(property))
					{
						return structuralTypeConfiguration.ExplicitProperties[property];
					}
					return null;
				}
			}
			return null;
		}
	}
}