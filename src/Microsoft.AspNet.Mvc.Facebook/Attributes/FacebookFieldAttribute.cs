// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Facebook.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class FacebookFieldAttribute : Attribute
    {
        public string FieldName { get; set; }
        public string JsonField { get; set; }
        public bool Ignore { get; set; }
    }
}
