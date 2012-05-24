// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace System.Web.Mvc
{
    internal class FileExtensionsAttributeAdapter : DataAnnotationsModelValidator
    {
        private static Lazy<Func<ValidationAttribute, string>> extensions = new Lazy<Func<ValidationAttribute, string>>(
                () => ValidationAttributeHelpers.GetPropertyDelegate<string>(ValidationAttributeHelpers.FileExtensionsAttributeType, "Extensions"));

        public FileExtensionsAttributeAdapter(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
            Contract.Assert(attribute.GetType() == ValidationAttributeHelpers.FileExtensionsAttributeType);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "accept",
                ErrorMessage = ErrorMessage
            };
            rule.ValidationParameters["exts"] = extensions.Value(Attribute);
            yield return rule;
        }
    }
}
