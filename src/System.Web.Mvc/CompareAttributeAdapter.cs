// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DataAnnotationsCompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace System.Web.Mvc
{
    internal class CompareAttributeAdapter : DataAnnotationsModelValidator<DataAnnotationsCompareAttribute>
    {
        public CompareAttributeAdapter(ModelMetadata metadata, ControllerContext context, DataAnnotationsCompareAttribute attribute)
            : base(metadata, context, attribute)
        {
            Contract.Assert(attribute.GetType() == typeof(DataAnnotationsCompareAttribute));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationEqualToRule(ErrorMessage, FormatPropertyForClientValidation(Attribute.OtherProperty));
        }

        private static string FormatPropertyForClientValidation(string property)
        {
            Contract.Assert(property != null);

            return "*." + property;
        }
    }
}
