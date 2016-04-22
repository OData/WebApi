using System;

namespace Microsoft.AspNetCore.OData.Formatter
{
	internal class InlineValueProcessor : IValueProcessor
	{
		private readonly Func<ValueInterceptor, bool> _processor;

		public InlineValueProcessor(Func<ValueInterceptor, bool> processor)
		{
			_processor = processor;
		}

		public bool Process(ValueInterceptor value)
		{
			return _processor(value);
		}
	}

	public class ValueInterceptor
	{
		public object Value { get; set; }
		public Type IntendedType { get; set; }
		public string PropertyName { get; set; }
		public object DeclaringInstance { get; set; }
		public Type DeclaringType { get; set; }
		public bool IsRoot { get; set; }

		public ValueInterceptor(object value, Type intendedType, string propertyName, object declaringInstance, Type declaringType, bool isRoot)
		{
			Value = value;
			IntendedType = intendedType;
			PropertyName = propertyName;
			DeclaringInstance = declaringInstance;
			DeclaringType = declaringType;
			IsRoot = isRoot;
		}
	}
}