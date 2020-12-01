using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// ResourceSet items for resources set wrapper
    /// </summary>
    internal class ODataResourceSetItemList : IList<ODataResourceSetItemBase>
    {
        private IList<ODataResourceWrapper> items;

        public ODataResourceSetItemList(IList<ODataResourceWrapper> items)
        {
            this.items = items;
        }

        public ODataResourceSetItemBase this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                ODataResourceWrapper resourceValue = ValidateResourceWrapper(value);
                this.items[index] = resourceValue;
            }
        }

        public int Count => this.items.Count;

        public bool IsReadOnly => this.items.IsReadOnly;

        public void Add(ODataResourceSetItemBase item)
        {
            ODataResourceWrapper resourceValue = ValidateResourceWrapper(item);
            this.items.Add(resourceValue);
        }

        public void Clear()
        {
            this.items.Clear();
        }

        public bool Contains(ODataResourceSetItemBase item)
        {
            return this.items.Contains(item as ODataResourceWrapper);
        }

        public void CopyTo(ODataResourceSetItemBase[] array, int arrayIndex)
        {
            for (int index = 0; index < array.Length; index++)
            {
                ODataResourceWrapper resourceValue = ValidateResourceWrapper(array[index]);
                this.items.Insert(arrayIndex + index, resourceValue);
            }
        }

        public IEnumerator<ODataResourceSetItemBase> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public int IndexOf(ODataResourceSetItemBase item)
        {
            return this.IndexOf(item);
        }

        public void Insert(int index, ODataResourceSetItemBase item)
        {
            ODataResourceWrapper resourceValue = ValidateResourceWrapper(item);
            this.items.Insert(index, resourceValue);
        }

        public bool Remove(ODataResourceSetItemBase item)
        {
            return this.items.Remove(item as ODataResourceWrapper);
        }

        public void RemoveAt(int index)
        {
            this.items.RemoveAt(index);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private ODataResourceWrapper ValidateResourceWrapper(ODataResourceSetItemBase item)
        {
            ODataResourceWrapper resourceWrapper = item as ODataResourceWrapper;
            if (item == null)
            {
                throw new ODataException(Error.Format(SRResources.ResourceSetWrapperSupported));
            }
            return resourceWrapper;
        }
    }
}