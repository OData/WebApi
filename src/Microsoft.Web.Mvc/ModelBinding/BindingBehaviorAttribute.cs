// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Web.Mvc.ModelBinding
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This class is designed to be overridden")]
    public class BindingBehaviorAttribute : Attribute
    {
        private static readonly object _typeId = new object();

        public BindingBehaviorAttribute(BindingBehavior behavior)
        {
            Behavior = behavior;
        }

        public BindingBehavior Behavior { get; private set; }

        public override object TypeId
        {
            get { return _typeId; }
        }
    }
}
