// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public abstract class PropertyConfiguration
    {
        protected PropertyConfiguration(PropertyInfo property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            PropertyInfo = property;
        }

        public string Name
        {
            get { return PropertyInfo.Name; }
        }

        public PropertyInfo PropertyInfo { get; private set; }

        public abstract Type RelatedClrType { get; }

        public abstract PropertyKind Kind { get; }
    }
}
