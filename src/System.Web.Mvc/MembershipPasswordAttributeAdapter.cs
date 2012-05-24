// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace System.Web.Mvc
{
    internal class MembershipPasswordAttributeAdapter : DataAnnotationsModelValidator
    {
        private static Lazy<Func<ValidationAttribute, int>> minRequiredNonAlphanumericCharacters = GetLazyPropertyDelegate<int>("MinRequiredNonAlphanumericCharacters");
        private static Lazy<Func<ValidationAttribute, int>> minRequiredPasswordLength = GetLazyPropertyDelegate<int>("MinRequiredPasswordLength");
        private static Lazy<Func<ValidationAttribute, string>> passwordStrengthRegularExpression = GetLazyPropertyDelegate<string>("PasswordStrengthRegularExpression");

        public MembershipPasswordAttributeAdapter(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
            Contract.Assert(attribute.GetType() == ValidationAttributeHelpers.MembershipPasswordAttributeType);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationMembershipPasswordRule(ErrorMessage, minRequiredPasswordLength.Value(Attribute), minRequiredNonAlphanumericCharacters.Value(Attribute), passwordStrengthRegularExpression.Value(Attribute));
        }

        private static Lazy<Func<ValidationAttribute, TProperty>> GetLazyPropertyDelegate<TProperty>(string propertyName)
        {
            return new Lazy<Func<ValidationAttribute, TProperty>>(
                () => ValidationAttributeHelpers.GetPropertyDelegate<TProperty>(ValidationAttributeHelpers.MembershipPasswordAttributeType, propertyName));
        }
    }
}
