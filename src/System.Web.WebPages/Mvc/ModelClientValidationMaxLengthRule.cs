// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class ModelClientValidationMaxLengthRule : ModelClientValidationRule
    {
        public ModelClientValidationMaxLengthRule(string errorMessage, int maximumLength)
        {
            ErrorMessage = errorMessage;
            ValidationType = "maxlength";
            ValidationParameters["max"] = maximumLength;
        }
    }
}
