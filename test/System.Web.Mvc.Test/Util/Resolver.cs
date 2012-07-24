// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc.Test
{
    public class Resolver<T> : IResolver<T>
    {
        public T Current { get; set; }
    }
}
