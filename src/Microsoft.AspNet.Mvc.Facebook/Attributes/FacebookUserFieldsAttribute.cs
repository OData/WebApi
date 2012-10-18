// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class FacebookUserFieldsAttribute : CustomModelBinderAttribute
    {
        public FacebookUserFieldsAttribute() { }

        public FacebookUserFieldsAttribute(string fields)
        {
            Fields = fields;
        }

        public string Fields { get; set; }

        public override IModelBinder GetBinder()
        {
            return new FacebookUserModelBinder(Fields);
        }
    }
}
