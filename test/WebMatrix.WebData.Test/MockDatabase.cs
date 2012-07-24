// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace WebMatrix.WebData.Test
{
    public abstract class MockDatabase : IDatabase
    {
        public abstract dynamic QuerySingle(string commandText, params object[] args);

        public abstract IEnumerable<dynamic> Query(string commandText, params object[] parameters);

        public abstract dynamic QueryValue(string commandText, params object[] parameters);

        public abstract int Execute(string commandText, params object[] args);

        public void Dispose()
        {
            // Do nothing.
        }
    }
}
