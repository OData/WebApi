// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace System.Web.Mvc
{
    public class DataAnnotationsModelValidator<TAttribute> : DataAnnotationsModelValidator
        where TAttribute : ValidationAttribute
    {
        public DataAnnotationsModelValidator(ModelMetadata metadata, ControllerContext context, TAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        protected new TAttribute Attribute
        {
            get { return (TAttribute)base.Attribute; }
        }
    }
}
