using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNetCore.OData
{
	public class ODataServiceBuilder
	{
		//private readonly ODataContextProvider _provider;
		private readonly IServiceCollection _serviceCollection;

		public ODataServiceBuilder([NotNull]IServiceCollection serviceCollection)
		{
			this._serviceCollection = serviceCollection;
			//this._provider = new ODataContextProvider();
			//_serviceCollection.AddSingleton(_ => _provider);
		}

		//public void Register<T>(string prefix) where T : class
		//{
		//	this._provider.Register<T>(prefix);
		//}

		public IServiceCollection Service => _serviceCollection;
	}

	//public class ODataContextProvider
	//{
	//	internal IDictionary<string, ODataContext> ContextMap { get; } = new Dictionary<string, ODataContext>();

	//	internal void Register<T>(string prefix) where T : class
	//	{
	//		ContextMap[prefix] = new ODataContext(typeof(T));
	//	}

	//	internal ODataContext Lookup(string prefix)
	//	{
	//		return ContextMap[prefix];
	//	}
	//}
}
