// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

namespace System.Web.Mvc
{
    [TypeForwardedFrom("System.Web.Mvc, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ModelClientValidationRangeRule : ModelClientValidationRule
    {
        public ModelClientValidationRangeRule(string errorMessage, object minValue, object maxValue)
        {
            ErrorMessage = errorMessage;
            ValidationType = "range";
            ValidationParameters["min"] = minValue;
            ValidationParameters["max"] = maxValue;
        }
    }
}
