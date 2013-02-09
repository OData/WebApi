// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;

namespace System.Web.Mvc
{
    // Concrete implementation for the IUnvalidatedRequestValues helper interface

    internal sealed class UnvalidatedRequestValuesWrapper : IUnvalidatedRequestValues
    {
        private readonly UnvalidatedRequestValuesBase _unvalidatedValues;

        public UnvalidatedRequestValuesWrapper(UnvalidatedRequestValuesBase unvalidatedValues)
        {
            _unvalidatedValues = unvalidatedValues;
        }

        public NameValueCollection Form
        {
            get { return _unvalidatedValues.Form; }
        }

        public NameValueCollection QueryString
        {
            get { return _unvalidatedValues.QueryString; }
        }

        public string this[string key]
        {
            get { return _unvalidatedValues[key]; }
        }
    }
}
