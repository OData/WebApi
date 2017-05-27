using System;

namespace Microsoft.AspNetCore.OData.Query
{
	public sealed class PageSizeAttribute : Attribute
	{
		public int Value { get; set; }

		public PageSizeAttribute(int value)
		{
			Value = value;
		}
	}
}