using System.Reflection;

namespace Microsoft.AspNet.OData.Builder.Conventions
{
	internal class DescriptionAnnotationConvention : IEdmTypeConvention
	{
		public void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model)
		{
			if (!(edmTypeConfiguration is StructuralTypeConfiguration structuralType)) return;

			var attribute = (DescriptionAttribute) structuralType.ClrType.GetCustomAttribute(typeof(DescriptionAttribute), true);
			if (attribute != null)
			{
				structuralType.Description = attribute.Description;
			}

			foreach (var property in structuralType.Properties)
			{
				attribute = (DescriptionAttribute) property.PropertyInfo.GetCustomAttribute(typeof(DescriptionAttribute), true);
				if (attribute != null)
				{
					property.Description = attribute.Description;
				}
			}
		
			
		}
	}
}