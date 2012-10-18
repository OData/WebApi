// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class FacebookUserAttribute : CustomModelBinderAttribute
    {
        public string Fields { get; set; }

        public override IModelBinder GetBinder()
        {
            return new FacebookUserModelBinder(Fields);
        }
    }
}
