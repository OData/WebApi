// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.Internal
{
    internal static class MemberInfoExtensions
    {
        public static TAttribute[] GetCustomAttributes<TAttribute>(this MemberInfo member, bool inherit) where TAttribute : class
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            return (TAttribute[])member.GetCustomAttributes(typeof(TAttribute), inherit);
        }
    }
}
