using System;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class DecimalRoundTestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal ExpectedDecimalValue { get; set; }
        public decimal ActualDecimalValue { get; set; }
        public DecimalRoundTestEntity(decimal actualValue, decimal expectedValue)
        {
            ActualDecimalValue = actualValue;
            ExpectedDecimalValue = expectedValue;
        }
    }
}
