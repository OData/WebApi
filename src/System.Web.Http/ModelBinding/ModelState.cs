// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    [Serializable]
    public class ModelState
    {
        private ModelErrorCollection _errors = new ModelErrorCollection();

        public ValueProviderResult Value { get; set; }

        public ModelErrorCollection Errors
        {
            get { return _errors; }
        }
    }
}
