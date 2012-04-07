// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace System.Web.WebPages
{
    public interface IValidator
    {
        ModelClientValidationRule ClientValidationRule { get; }
        ValidationResult Validate(ValidationContext validationContext);
    }
}
