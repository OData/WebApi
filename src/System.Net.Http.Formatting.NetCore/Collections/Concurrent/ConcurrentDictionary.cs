// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    // TODO: Remove this class after BCL makes their portable library version.
    internal sealed class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public ICollection<TKey> Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary)_dictionary).IsReadOnly;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                 return _dictionary.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        // ConcurrentDictionary members
        public bool TryRemove(TKey key, out TValue removedValue)
        {
            throw new NotImplementedException();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> addValueFactory)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                TValue value;

                if (_dictionary.ContainsKey(key))
                {
                    return _dictionary[key];
                }
                else
                {
                    value = addValueFactory.Invoke(key);
                    _lock.EnterWriteLock();
                    try
                    {
                        _dictionary.Add(key, value);
                        return value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_dictionary.ContainsKey(key))
                {
                    return false;
                }

                _lock.EnterWriteLock();
                try
                {
                    _dictionary.Add(key, value);
                    return true;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                // update
                if (_dictionary.ContainsKey(key))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _dictionary[key] = updateValueFactory.Invoke(key, _dictionary[key]);
                        return _dictionary[key];
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                // add
                _lock.EnterWriteLock();
                try
                {
                    _dictionary.Add(key, addValue);
                    return addValue;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }
}
