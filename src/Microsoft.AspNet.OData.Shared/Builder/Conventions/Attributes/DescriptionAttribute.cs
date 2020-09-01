using System;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
	/// <summary>
	/// Represents an <see cref="Attribute"/> that can be placed on a property or class to document its purpose. The content will be includes in the Odata metadata document.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class DescriptionAttribute : Attribute
	{
		/// <summary>
		/// Gets or summary about the purpose of the property or class.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Gets or sets a detailed description about the purpose of the property or class.
		/// </summary>
		public string LongDescription { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DescriptionAttribute"/> class.
		/// </summary>
		/// <param name="description">A summary about the purpose of the property or class.</param>
		public DescriptionAttribute(string description)
		{
			Description = description;
		}
	}
}