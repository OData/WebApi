// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Web.Mvc
{
    public class ValueProviderFactoryCollection : Collection<ValueProviderFactory>
    {
        private ValueProviderFactory[] _combinedItems;
        private IDependencyResolver _dependencyResolver;

        public ValueProviderFactoryCollection()
        {
        }

        public ValueProviderFactoryCollection(IList<ValueProviderFactory> list)
            : base(list)
        {
        }

        internal ValueProviderFactoryCollection(IList<ValueProviderFactory> list, IDependencyResolver dependencyResolver)
            : base(list)
        {
            _dependencyResolver = dependencyResolver;
        }

        internal ValueProviderFactory[] CombinedItems
        {
            get
            {
                ValueProviderFactory[] combinedItems = _combinedItems;
                if (combinedItems == null)
                {
                    combinedItems = MultiServiceResolver.GetCombined<ValueProviderFactory>(Items, _dependencyResolver);
                    _combinedItems = combinedItems;
                }
                return combinedItems;
            }
        }

        public IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            var valueProviders = from factory in CombinedItems
                                 let valueProvider = factory.GetValueProvider(controllerContext)
                                 where valueProvider != null
                                 select valueProvider;

            return new ValueProviderCollection(valueProviders.ToList());
        }

        protected override void InsertItem(int index, ValueProviderFactory item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _combinedItems = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ValueProviderFactory item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.SetItem(index, item);
        }
    }
}
