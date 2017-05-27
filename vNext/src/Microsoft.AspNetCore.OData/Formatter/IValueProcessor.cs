namespace Microsoft.AspNetCore.OData.Formatter
{
	public interface IValueProcessor
	{
		bool Process(ValueInterceptor value);
	}
}