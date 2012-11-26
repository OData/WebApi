// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Facebook
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class FacebookFieldModifierAttribute : Attribute
    {
        public FacebookFieldModifierAttribute(string fieldModifier)
        {
            FieldModifier = fieldModifier;
        }

        public string FieldModifier { get; set; }
    }
}
