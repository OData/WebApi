// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Facebook
{
    /// <summary>
    /// Allows adding field modifiers when querying Facebook Graph API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class FacebookFieldModifierAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookFieldModifierAttribute" /> class.
        /// </summary>
        /// <param name="fieldModifier">The field modifier.</param>
        public FacebookFieldModifierAttribute(string fieldModifier)
        {
            FieldModifier = fieldModifier;
        }

        /// <summary>
        /// Gets the field modifier.
        /// </summary>
        public string FieldModifier { get; private set; }
    }
}