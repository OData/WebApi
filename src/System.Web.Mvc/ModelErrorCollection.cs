// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace System.Web.Mvc
{
    [Serializable]
    public class ModelErrorCollection : Collection<ModelError>
    {
        public void Add(Exception exception)
        {
            Add(new ModelError(exception));
        }

        public void Add(string errorMessage)
        {
            Add(new ModelError(errorMessage));
        }
    }
}
