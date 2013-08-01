// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Security;

namespace System.Web.Mvc
{
    internal class MembershipPasswordAttributeAdapter : DataAnnotationsModelValidator<MembershipPasswordAttribute>
    {
        public MembershipPasswordAttributeAdapter(ModelMetadata metadata, ControllerContext context, MembershipPasswordAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationMembershipPasswordRule(ErrorMessage, Attribute.MinRequiredPasswordLength, Attribute.MinRequiredNonAlphanumericCharacters, Attribute.PasswordStrengthRegularExpression);
        }
    }
}
