using System.Threading.Tasks;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
	public class SampleODataSerializerProvider : DefaultODataSerializerProvider
	{
		private ODataEntityTypeSerializer _sampleEntityTypeSerializer;

		public SampleODataSerializerProvider()
		{
			_sampleEntityTypeSerializer = new SampleODataEntityTypeSerializer(this);
		}

		public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
		{
			switch (edmType.TypeKind())
			{
				case EdmTypeKind.Entity:
					return _sampleEntityTypeSerializer;
			}
			return base.GetEdmTypeSerializer(edmType);
		}
	}

	public class SampleODataEntityTypeSerializer : ODataEntityTypeSerializer
	{
		public SampleODataEntityTypeSerializer(ODataSerializerProvider serializerProvider) : base(serializerProvider)
		{
		}

		protected override async Task CreatingEntityInstanceContextAsync(object entity, IEdmEntityTypeReference entityType, object graph, ODataSerializerContext writeContext)
		{
			if (entity is Customer)
			{
				await PreProcessCustomerAsync(entity as Customer);
			}
			await base.CreatingEntityInstanceContextAsync(entity, entityType, graph, writeContext);
		}

		private async Task PreProcessCustomerAsync(Customer customer)
		{
			customer.FirstName = "We did it";
		}
	}
}