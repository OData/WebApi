// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Web.Mvc;

namespace System.Web.WebPages
{
    internal class CompareValidator : RequestFieldValidatorBase
    {
        private readonly string _otherField;
        private readonly ModelClientValidationEqualToRule _clientValidationRule;

        public CompareValidator(string otherField, string errorMessage)
            : base(errorMessage)
        {
            Debug.Assert(!String.IsNullOrEmpty(otherField));
            _otherField = otherField;
            _clientValidationRule = new ModelClientValidationEqualToRule(errorMessage, otherField);
        }

        public override ModelClientValidationRule ClientValidationRule
        {
            get { return _clientValidationRule; }
        }

        protected override bool IsValid(HttpContextBase httpContext, string value)
        {
            string otherValue = GetRequestValue(httpContext.Request, _otherField);
            return String.Equals(value, otherValue, StringComparison.CurrentCulture);
        }
    }
}
