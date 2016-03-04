using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using System;

namespace Microsoft.Framework.Internal
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	internal sealed class NotNullAttribute : Attribute
	{
	}
}