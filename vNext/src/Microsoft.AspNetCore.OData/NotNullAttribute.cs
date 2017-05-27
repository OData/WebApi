using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Framework.Internal
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	internal sealed class NotNullAttribute : Attribute
	{
	}
}