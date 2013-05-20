// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace System.Web.Mvc
{
    internal class AcceptAttributeAdapter : DataAnnotationsModelValidator
    {
        private static Lazy<Func<ValidationAttribute, string>> mimeTypes = new Lazy<Func<ValidationAttribute, string>>(
                () => ValidationAttributeHelpers.GetPropertyDelegate<string>(ValidationAttributeHelpers.AcceptAttributeType, "MimeTypes"));

        public AcceptAttributeAdapter(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
            Contract.Assert(attribute.GetType() == ValidationAttributeHelpers.AcceptAttributeType);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "accept",
                ErrorMessage = ErrorMessage
            };
            rule.ValidationParameters["mimetype"] = mimeTypes.Value(Attribute);
            yield return rule;
        }
    }
}
