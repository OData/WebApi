// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.ContentNegotiation
{
    public class ConnegController : ApiController
    {
        public ConnegItem GetItem(string name = "Fido", int age = 3)
        {
            return new ConnegItem()
            {
                Name = name,
                Age = age
            };
        }

        public ConnegItem PostItem(ConnegItem item)
        {
            return item;
        }
    }
}
