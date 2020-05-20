// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SortedList
**
** Purpose: A generic sorted list.
**
** Date:  January 28, 2003
** 
===========================================================*/
namespace ME.ECS.Collections {

    using System;
    using System.Security.Permissions;
    using System.Diagnostics;
    using System.Collections.Generic;

    // The SortedDictionary class implements a generic sorted list of keys 
    // and values. Entries in a sorted list are sorted by their keys and 
    // are accessible both by key and by index. The keys of a sorted dictionary
    // can be ordered either according to a specific IComparer 
    // implementation given when the sorted dictionary is instantiated, or 
    // according to the IComparable implementation provided by the keys 
    // themselves. In either case, a sorted dictionary does not allow entries
    // with duplicate or null keys.
    // 
    // A sorted list internally maintains two arrays that store the keys and
    // values of the entries. The capacity of a sorted list is the allocated
    // length of these internal arrays. As elements are added to a sorted list, the
    // capacity of the sorted list is automatically increased as required by
    // reallocating the internal arrays.  The capacity is never automatically 
    // decreased, but users can call either TrimExcess or 
    // Capacity explicitly.
    // 
    // The GetKeyList and GetValueList methods of a sorted list
    // provides access to the keys and values of the sorted list in the form of
    // List implementations. The List objects returned by these
    // methods are aliases for the underlying sorted list, so modifications
    // made to those lists are directly reflected in the sorted list, and vice
    // versa.
    // 
    // The SortedList class provides a convenient way to create a sorted
    // copy of another dictionary, such as a Hashtable. For example:
    // 
    // Hashtable h = new Hashtable();
    // h.Add(...);
    // h.Add(...);
    // ...
    // SortedList s = new SortedList(h);
    // 
    // The last line above creates a sorted list that contains a copy of the keys
    // and values stored in the hashtable. In this particular example, the keys
    // will be ordered according to the IComparable interface, which they
    // all must implement. To impose a different ordering, SortedList also
    // has a constructor that allows a specific IComparer implementation to
    // be specified.
    // 
    [DebuggerDisplay("Count = {Count}")]
    #if !FEATURE_NETCORE
    [Serializable()]
    #endif
    [System.Runtime.InteropServices.ComVisible(false)]
    public class SortedList<TKey, TValue> :
        IDictionary<TKey, TValue>, System.Collections.IDictionary, IReadOnlyDictionary<TKey, TValue> {

        private TKey[] keys;
        private TValue[] values;
        private HashSet<TKey> keysContains;
        private int _size;
        private int version;
        private IComparer<TKey> comparer;
        private KeyList keyList;
        private ValueList valueList;
        #if !FEATURE_NETCORE
        [NonSerialized]
        #endif
        private Object _syncRoot;

        private static TKey[] emptyKeys = new TKey[0];
        private static TValue[] emptyValues = new TValue[0];

        private const int _defaultCapacity = 4;

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to _defaultCapacity, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        public SortedList() {
            this.keysContains = new HashSet<TKey>();
            this.keys = SortedList<TKey, TValue>.emptyKeys;
            this.values = SortedList<TKey, TValue>.emptyValues;
            this._size = 0;
            this.comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to 16, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        //
        public SortedList(int capacity) {
            this.keysContains = new HashSet<TKey>();
            this.keys = new TKey[capacity];
            this.values = new TValue[capacity];
            this.comparer = Comparer<TKey>.Default;
        }

        // Constructs a new sorted list with a given IComparer
        // implementation. The sorted list is initially empty and has a capacity of
        // zero. Upon adding the first element to the sorted list the capacity is
        // increased to 16, and then increased in multiples of two as required. The
        // elements of the sorted list are ordered according to the given
        // IComparer implementation. If comparer is null, the
        // elements are compared to each other using the IComparable
        // interface, which in that case must be implemented by the keys of all
        // entries added to the sorted list.
        // 
        public SortedList(IComparer<TKey> comparer)
            : this() {
            if (comparer != null) {
                this.comparer = comparer;
            }
        }

        // Constructs a new sorted dictionary with a given IComparer
        // implementation and a given initial capacity. The sorted list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required. The elements of the sorted list
        // are ordered according to the given IComparer implementation. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by the keys of all entries added to the sorted list.
        // 
        public SortedList(int capacity, IComparer<TKey> comparer)
            : this(comparer) {
            this.Capacity = capacity;
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the IComparable interface, which must be implemented by the
        // keys of all entries in the the given dictionary as well as keys
        // subsequently added to the sorted list.
        // 
        public SortedList(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null) { }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the given IComparer implementation. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented
        // by the keys of all entries in the the given dictionary as well as keys
        // subsequently added to the sorted list.
        // 
        public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
            : this(dictionary != null ? dictionary.Count : 0, comparer) {

            dictionary.Keys.CopyTo(this.keys, 0);
            dictionary.Values.CopyTo(this.values, 0);
            Array.Sort<TKey, TValue>(this.keys, this.values, comparer);
            this._size = dictionary.Count;
        }

        // Adds an entry with the given key and value to this sorted list. An
        // ArgumentException is thrown if the key is already present in the sorted list.
        // 
        public void Add(TKey key, TValue value) {
            var i = Array.BinarySearch<TKey>(this.keys, 0, this._size, key, this.comparer);
            this.Insert(~i, key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) {
            this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair) {
            var index = this.IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(this.values[index], keyValuePair.Value)) {
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) {
            var index = this.IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(this.values[index], keyValuePair.Value)) {
                this.RemoveAt(index);
                return true;
            }

            return false;
        }

        // Returns the capacity of this sorted list. The capacity of a sorted list
        // represents the allocated length of the internal arrays used to store the
        // keys and values of the list, and thus also indicates the maximum number
        // of entries the list can contain before a reallocation of the internal
        // arrays is required.
        // 
        public int Capacity {
            get {
                return this.keys.Length;
            }
            set {
                if (value != this.keys.Length) {

                    if (value > 0) {
                        var newKeys = new TKey[value];
                        var newValues = new TValue[value];
                        if (this._size > 0) {
                            Array.Copy(this.keys, 0, newKeys, 0, this._size);
                            Array.Copy(this.values, 0, newValues, 0, this._size);
                        }

                        this.keys = newKeys;
                        this.values = newValues;
                    } else {
                        this.keys = SortedList<TKey, TValue>.emptyKeys;
                        this.values = SortedList<TKey, TValue>.emptyValues;
                    }
                }
            }
        }

        public IComparer<TKey> Comparer {
            get {
                return this.comparer;
            }
        }

        void System.Collections.IDictionary.Add(Object key, Object value) {

            var tempKey = (TKey)key;
            this.Add(tempKey, (TValue)value);

        }

        // Returns the number of entries in this sorted list.
        // 
        public int Count {
            get {
                return this._size;
            }
        }

        // Returns a collection representing the keys of this sorted list. This
        // method returns the same object as GetKeyList, but typed as an
        // ICollection instead of an IList.
        // 
        public KeyList Keys {
            get {
                return new KeyList(this);
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys {
            get {
                throw new AllocationException();
            }
        }

        System.Collections.ICollection System.Collections.IDictionary.Keys {
            get {
                throw new AllocationException();
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys {
            get {
                throw new AllocationException();
            }
        }

        // Returns a collection representing the values of this sorted list. This
        // method returns the same object as GetValueList, but typed as an
        // ICollection instead of an IList.
        // 
        public ValueList Values {
            get {
                return new ValueList(this);
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values {
            get {
                throw new AllocationException();
            }
        }

        System.Collections.ICollection System.Collections.IDictionary.Values {
            get {
                throw new AllocationException();
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
            get {
                throw new AllocationException();
            }
        }

        private KeyList GetKeyListHelper() {
            return new KeyList(this);
        }

        private ValueList GetValueListHelper() {
            return new ValueList(this);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
            get {
                return false;
            }
        }

        bool System.Collections.IDictionary.IsReadOnly {
            get {
                return false;
            }
        }

        bool System.Collections.IDictionary.IsFixedSize {
            get {
                return false;
            }
        }

        bool System.Collections.ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        // Synchronization root for this object.
        Object System.Collections.ICollection.SyncRoot {
            get {
                if (this._syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange(ref this._syncRoot, new Object(), null);
                }

                return this._syncRoot;
            }
        }

        // Removes all entries from this sorted list.
        public void Clear() {
            // clear does not change the capacity
            this.version++;
            // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            this.keysContains.Clear();
            Array.Clear(this.keys, 0, this._size);
            Array.Clear(this.values, 0, this._size);
            this._size = 0;
        }


        bool System.Collections.IDictionary.Contains(Object key) {
            if (SortedList<TKey, TValue>.IsCompatibleKey(key)) {
                return this.ContainsKey((TKey)key);
            }

            return false;
        }

        // Checks if this sorted list contains an entry with the given key.
        // 
        public bool ContainsKey(TKey key) {
            return this.keysContains.Contains(key); //this.IndexOfKey(key) >= 0;
        }

        // Checks if this sorted list contains an entry with the given value. The
        // values of the entries of the sorted list are compared to the given value
        // using the Object.Equals method. This method performs a linear
        // search and is substantially slower than the Contains
        // method.
        // 
        public bool ContainsValue(TValue value) {
            return this.IndexOfValue(value) >= 0;
        }

        // Copies the values in this SortedList to an array.
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            for (var i = 0; i < this.Count; i++) {
                var entry = new KeyValuePair<TKey, TValue>(this.keys[i], this.values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex) {

            var keyValuePairArray = array as KeyValuePair<TKey, TValue>[];
            if (keyValuePairArray != null) {
                for (var i = 0; i < this.Count; i++) {
                    keyValuePairArray[i + arrayIndex] = new KeyValuePair<TKey, TValue>(this.keys[i], this.values[i]);
                }
            } else {
                var objects = array as object[];

                for (var i = 0; i < this.Count; i++) {
                    objects[i + arrayIndex] = new KeyValuePair<TKey, TValue>(this.keys[i], this.values[i]);
                }

            }
        }

        private const int MaxArrayLength = 0X7FEFFFFF;

        // Ensures that the capacity of this sorted list is at least the given
        // minimum value. If the currect capacity of the list is less than
        // min, the capacity is increased to twice the current capacity or
        // to min, whichever is larger.
        private void EnsureCapacity(int min) {
            var newCapacity = this.keys.Length == 0 ? SortedList<TKey, TValue>._defaultCapacity : this.keys.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > SortedList<TKey, TValue>.MaxArrayLength) {
                newCapacity = SortedList<TKey, TValue>.MaxArrayLength;
            }

            if (newCapacity < min) {
                newCapacity = min;
            }

            this.Capacity = newCapacity;
        }

        // Returns the value of the entry at the given index.
        // 
        private TValue GetByIndex(int index) {
            return this.values[index];
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            throw new AllocationException();
            //return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            throw new AllocationException();
            //return new Enumerator(this, Enumerator.KeyValuePair);
        }

        System.Collections.IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator() {
            throw new AllocationException();
            //return new Enumerator(this, Enumerator.DictEntry);

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new AllocationException();
            //return new Enumerator(this, Enumerator.KeyValuePair);
        }


        // Returns the key of the entry at the given index.
        // 
        private TKey GetKey(int index) {
            return this.keys[index];
        }


        // Returns the value associated with the given key. If an entry with the
        // given key is not found, the returned value is null.
        // 
        public TValue this[TKey key] {
            get {
                var i = this.IndexOfKey(key);
                if (i >= 0) {
                    return this.values[i];
                }

                return default(TValue);
            }
            set {
                var i = Array.BinarySearch<TKey>(this.keys, 0, this._size, key, this.comparer);
                if (i >= 0) {
                    this.values[i] = value;
                    this.version++;
                    return;
                }

                this.Insert(~i, key, value);
            }
        }

        Object System.Collections.IDictionary.this[Object key] {
            get {
                if (SortedList<TKey, TValue>.IsCompatibleKey(key)) {
                    var i = this.IndexOfKey((TKey)key);
                    if (i >= 0) {
                        return this.values[i];
                    }
                }

                return null;
            }
            set {

                var tempKey = (TKey)key;
                this[tempKey] = (TValue)value;
            }
        }

        // Returns the index of the entry with a given key in this sorted list. The
        // key is located through a binary search, and thus the average execution
        // time of this method is proportional to Log2(size), where
        // size is the size of this sorted list. The returned value is -1 if
        // the given key does not occur in this sorted list. Null is an invalid 
        // key value.
        // 
        public int IndexOfKey(TKey key) {
            var ret = Array.BinarySearch<TKey>(this.keys, 0, this._size, key, this.comparer);
            return ret >= 0 ? ret : -1;
        }

        // Returns the index of the first occurrence of an entry with a given value
        // in this sorted list. The entry is located through a linear search, and
        // thus the average execution time of this method is proportional to the
        // size of this sorted list. The elements of the list are compared to the
        // given value using the Object.Equals method.
        // 
        public int IndexOfValue(TValue value) {
            return Array.IndexOf(this.values, value, 0, this._size);
        }

        // Inserts an entry with a given key and value at a given index.
        private void Insert(int index, TKey key, TValue value) {
            if (this._size == this.keys.Length) {
                this.EnsureCapacity(this._size + 1);
            }

            if (index < this._size) {
                Array.Copy(this.keys, index, this.keys, index + 1, this._size - index);
                Array.Copy(this.values, index, this.values, index + 1, this._size - index);
            }

            this.keysContains.Add(key);
            this.keys[index] = key;
            this.values[index] = value;
            this._size++;
            this.version++;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            var i = this.IndexOfKey(key);
            if (i >= 0) {
                value = this.values[i];
                return true;
            }

            value = default(TValue);
            return false;
        }

        // Removes the entry at the given index. The size of the sorted list is
        // decreased by one.
        // 
        public void RemoveAt(int index) {
            this._size--;
            if (index < this._size) {
                Array.Copy(this.keys, index + 1, this.keys, index, this._size - index);
                Array.Copy(this.values, index + 1, this.values, index, this._size - index);
            }

            this.keys[this._size] = default(TKey);
            this.values[this._size] = default(TValue);
            this.version++;
        }

        // Removes an entry from this sorted list. If an entry with the specified
        // key exists in the sorted list, it is removed. An ArgumentException is
        // thrown if the key is null.
        // 
        public bool Remove(TKey key) {
            this.keysContains.Remove(key);
            var i = this.IndexOfKey(key);
            if (i >= 0) {
                this.RemoveAt(i);
            }

            return i >= 0;
        }

        void System.Collections.IDictionary.Remove(Object key) {
            if (SortedList<TKey, TValue>.IsCompatibleKey(key)) {
                this.Remove((TKey)key);
            }
        }

        // Sets the capacity of this sorted list to the size of the sorted list.
        // This method can be used to minimize a sorted list's memory overhead once
        // it is known that no new elements will be added to the sorted list. To
        // completely clear a sorted list and release all memory referenced by the
        // sorted list, execute the following statements:
        // 
        // SortedList.Clear();
        // SortedList.TrimExcess();
        // 
        public void TrimExcess() {
            var threshold = (int)((double)this.keys.Length * 0.9);
            if (this._size < threshold) {
                this.Capacity = this._size;
            }
        }

        private static bool IsCompatibleKey(object key) {
            return key is TKey;
        }


        /// <include file='doc\SortedList.uex' path='docs/doc[@for="SortedListEnumerator"]/*' />
        #if !FEATURE_NETCORE
        [Serializable()]
        #endif
        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, System.Collections.IDictionaryEnumerator {

            private SortedList<TKey, TValue> _sortedList;
            private TKey key;
            private TValue value;
            private int index;
            private int version;
            private int getEnumeratorRetType; // What should Enumerator.Current return?

            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;

            internal Enumerator(SortedList<TKey, TValue> sortedList, int getEnumeratorRetType) {
                this._sortedList = sortedList;
                this.index = 0;
                this.version = this._sortedList.version;
                this.getEnumeratorRetType = getEnumeratorRetType;
                this.key = default(TKey);
                this.value = default(TValue);
            }

            public void Dispose() {
                this.index = 0;
                this.key = default(TKey);
                this.value = default(TValue);
            }


            Object System.Collections.IDictionaryEnumerator.Key {
                get {
                    return this.key;
                }
            }

            public bool MoveNext() {
                if ((uint)this.index < (uint)this._sortedList.Count) {
                    this.key = this._sortedList.keys[this.index];
                    this.value = this._sortedList.values[this.index];
                    this.index++;
                    return true;
                }

                this.index = this._sortedList.Count + 1;
                this.key = default(TKey);
                this.value = default(TValue);
                return false;
            }

            System.Collections.DictionaryEntry System.Collections.IDictionaryEnumerator.Entry {
                get {
                    throw new AllocationException();
                }
            }

            public KeyValuePair<TKey, TValue> Current {
                get {
                    return new KeyValuePair<TKey, TValue>(this.key, this.value);
                }
            }

            Object System.Collections.IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            Object System.Collections.IDictionaryEnumerator.Value {
                get {
                    throw new AllocationException();
                }
            }

            void System.Collections.IEnumerator.Reset() {
                this.index = 0;
                this.key = default(TKey);
                this.value = default(TValue);
            }

        }

        #if !FEATURE_NETCORE
        [Serializable()]
        #endif
        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, System.Collections.IEnumerator {

            private SortedList<TKey, TValue> _sortedList;
            private int index;
            private int version;
            private TKey currentKey;

            internal SortedListKeyEnumerator(SortedList<TKey, TValue> sortedList) {
                this._sortedList = sortedList;
                this.version = sortedList.version;
            }

            public void Dispose() {
                this.index = 0;
                this.currentKey = default(TKey);
            }

            public bool MoveNext() {
                if ((uint)this.index < (uint)this._sortedList.Count) {
                    this.currentKey = this._sortedList.keys[this.index];
                    this.index++;
                    return true;
                }

                this.index = this._sortedList.Count + 1;
                this.currentKey = default(TKey);
                return false;
            }

            public TKey Current {
                get {
                    return this.currentKey;
                }
            }

            Object System.Collections.IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            void System.Collections.IEnumerator.Reset() {
                this.index = 0;
                this.currentKey = default(TKey);
            }

        }

        #if !FEATURE_NETCORE
        [Serializable()]
        #endif
        private sealed class SortedListValueEnumerator : IEnumerator<TValue>, System.Collections.IEnumerator {

            private SortedList<TKey, TValue> _sortedList;
            private int index;
            private int version;
            private TValue currentValue;

            internal SortedListValueEnumerator(SortedList<TKey, TValue> sortedList) {
                this._sortedList = sortedList;
                this.version = sortedList.version;
            }

            public void Dispose() {
                this.index = 0;
                this.currentValue = default(TValue);
            }

            public bool MoveNext() {
                if ((uint)this.index < (uint)this._sortedList.Count) {
                    this.currentValue = this._sortedList.values[this.index];
                    this.index++;
                    return true;
                }

                this.index = this._sortedList.Count + 1;
                this.currentValue = default(TValue);
                return false;
            }

            public TValue Current {
                get {
                    return this.currentValue;
                }
            }

            Object System.Collections.IEnumerator.Current {
                get {
                    return this.currentValue;
                }
            }

            void System.Collections.IEnumerator.Reset() {
                this.index = 0;
                this.currentValue = default(TValue);
            }

        }

        [DebuggerDisplay("Count = {Count}")]
        #if !FEATURE_NETCORE
        [Serializable()]
        #endif
        public struct KeyList : IList<TKey>, System.Collections.ICollection {

            private SortedList<TKey, TValue> _dict;

            internal KeyList(SortedList<TKey, TValue> dictionary) {
                this._dict = dictionary;
            }

            public int Count {
                get {
                    return this._dict._size;
                }
            }

            public bool IsReadOnly {
                get {
                    return true;
                }
            }

            bool System.Collections.ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            Object System.Collections.ICollection.SyncRoot {
                get {
                    return ((System.Collections.ICollection)this._dict).SyncRoot;
                }
            }

            public void Add(TKey key) { }

            public void Clear() { }

            public bool Contains(TKey key) {
                return this._dict.ContainsKey(key);
            }

            public void CopyTo(TKey[] array, int arrayIndex) {
                // defer error checking to Array.Copy
                Array.Copy(this._dict.keys, 0, array, arrayIndex, this._dict.Count);
            }

            void System.Collections.ICollection.CopyTo(Array array, int arrayIndex) {
                // defer error checking to Array.Copy
                Array.Copy(this._dict.keys, 0, array, arrayIndex, this._dict.Count);
            }

            public void Insert(int index, TKey value) { }

            public TKey this[int index] {
                get {
                    return this._dict.GetKey(index);
                }
                set { }
            }

            public IEnumerator<TKey> GetEnumerator() {
                throw new AllocationException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                throw new AllocationException();
            }

            public int IndexOf(TKey key) {
                var i = Array.BinarySearch<TKey>(this._dict.keys, 0, this._dict.Count, key, this._dict.comparer);
                if (i >= 0) {
                    return i;
                }

                return -1;
            }

            public bool Remove(TKey key) {

                return false;
            }

            public void RemoveAt(int index) { }

        }

        [DebuggerDisplay("Count = {Count}")]
        #if !FEATURE_NETCORE
        [Serializable()]
        #endif
        public struct ValueList : IList<TValue>, System.Collections.ICollection {

            private SortedList<TKey, TValue> _dict;

            internal ValueList(SortedList<TKey, TValue> dictionary) {
                this._dict = dictionary;
            }

            public int Count {
                get {
                    return this._dict._size;
                }
            }

            public bool IsReadOnly {
                get {
                    return true;
                }
            }

            bool System.Collections.ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            Object System.Collections.ICollection.SyncRoot {
                get {
                    return ((System.Collections.ICollection)this._dict).SyncRoot;
                }
            }

            public void Add(TValue key) { }

            public void Clear() { }

            public bool Contains(TValue value) {
                return this._dict.ContainsValue(value);
            }

            public void CopyTo(TValue[] array, int arrayIndex) {
                // defer error checking to Array.Copy
                Array.Copy(this._dict.values, 0, array, arrayIndex, this._dict.Count);
            }

            void System.Collections.ICollection.CopyTo(Array array, int arrayIndex) {
                // defer error checking to Array.Copy
                Array.Copy(this._dict.values, 0, array, arrayIndex, this._dict.Count);
            }

            public void Insert(int index, TValue value) { }

            public TValue this[int index] {
                get {
                    return this._dict.GetByIndex(index);
                }
                set { }
            }

            public IEnumerator<TValue> GetEnumerator() {
                throw new AllocationException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                throw new AllocationException();
            }

            public int IndexOf(TValue value) {
                return Array.IndexOf(this._dict.values, value, 0, this._dict.Count);
            }

            public bool Remove(TValue value) {

                return false;
            }

            public void RemoveAt(int index) { }

        }

    }

}