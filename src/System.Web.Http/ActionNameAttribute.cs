// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ActionNameAttribute : Attribute
    {
        public ActionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
