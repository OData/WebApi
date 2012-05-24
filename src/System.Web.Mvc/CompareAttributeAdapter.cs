// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace System.Web.Mvc
{
    internal class CompareAttributeAdapter : DataAnnotationsModelValidator
    {
        private static Lazy<Func<ValidationAttribute, string>> otherProperty = new Lazy<Func<ValidationAttribute, string>>(
                () => ValidationAttributeHelpers.GetPropertyDelegate<string>(ValidationAttributeHelpers.CompareAttributeType, "OtherProperty"));

        public CompareAttributeAdapter(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
            Contract.Assert(attribute.GetType() == ValidationAttributeHelpers.CompareAttributeType);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationEqualToRule(ErrorMessage, FormatPropertyForClientValidation(otherProperty.Value(Attribute)));
        }

        private static string FormatPropertyForClientValidation(string property)
        {
            Contract.Assert(property != null);

            return "*." + property;
        }
    }
}
