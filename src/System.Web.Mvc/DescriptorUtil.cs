// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;

namespace System.Web.Mvc
{
    internal static class DescriptorUtil
    {
        private static void AppendPartToUniqueIdBuilder(StringBuilder builder, object part)
        {
            if (part == null)
            {
                builder.Append("[-1]");
            }
            else
            {
                string partString = Convert.ToString(part, CultureInfo.InvariantCulture);
                builder.AppendFormat("[{0}]{1}", partString.Length, partString);
            }
        }

        public static string CreateUniqueId(params object[] parts)
        {
            return CreateUniqueId((IEnumerable<object>)parts);
        }

        public static string CreateUniqueId(IEnumerable<object> parts)
        {
            // returns a unique string made up of the pieces passed in
            StringBuilder builder = new StringBuilder();
            foreach (object part in parts)
            {
                // We can special-case certain part types

                MemberInfo memberInfo = part as MemberInfo;
                if (memberInfo != null)
                {
                    AppendPartToUniqueIdBuilder(builder, memberInfo.Module.ModuleVersionId);
                    AppendPartToUniqueIdBuilder(builder, memberInfo.MetadataToken);
                    continue;
                }

                IUniquelyIdentifiable uniquelyIdentifiable = part as IUniquelyIdentifiable;
                if (uniquelyIdentifiable != null)
                {
                    AppendPartToUniqueIdBuilder(builder, uniquelyIdentifiable.UniqueId);
                    continue;
                }

                AppendPartToUniqueIdBuilder(builder, part);
            }

            return builder.ToString();
        }

        public static TDescriptor[] LazilyFetchOrCreateDescriptors<TReflection, TDescriptor>(ref TDescriptor[] cacheLocation, Func<TReflection[]> initializer, Func<TReflection, TDescriptor> converter)
        {
            // did we already calculate this once?
            TDescriptor[] existingCache = Interlocked.CompareExchange(ref cacheLocation, null, null);
            if (existingCache != null)
            {
                return existingCache;
            }

            // Note: since this code operates on arrays it is more efficient to call simple array operations
            // instead of LINQ-y extension methods such as Select and Where. DO NOT attempt to simplify this
            // without testing the performance impact.
            TReflection[] memberInfos = initializer();
            List<TDescriptor> descriptorsList = new List<TDescriptor>(memberInfos.Length);
            for (int i = 0; i < memberInfos.Length; i++)
            {
                TDescriptor descriptor = converter(memberInfos[i]);
                if (descriptor != null)
                {
                    descriptorsList.Add(descriptor);
                }
            }
            TDescriptor[] descriptors = descriptorsList.ToArray();

            TDescriptor[] updatedCache = Interlocked.CompareExchange(ref cacheLocation, descriptors, null);
            return updatedCache ?? descriptors;
        }
    }
}
