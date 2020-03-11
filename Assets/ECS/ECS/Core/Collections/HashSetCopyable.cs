namespace ME.ECS.Collections {
    
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
 
    /// <summary>
    /// Duplicated because internal to mscorlib
    /// </summary>
    internal static class HashSetCopyableHashHelpers {
        // Table of prime numbers to use as hash table sizes. 
        // The entry used for capacity is the smallest prime number in this array
        // that is larger than twice the previous capacity. 

        internal static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

        internal static bool IsPrime(int candidate) {
            if ((candidate & 1) != 0) {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2) {
                    if ((candidate % divisor) == 0) {
                        return false;
                    }
                }
                return true;
            }
            return (candidate == 2);
        }

        internal static int GetPrime(int min) {

            for (int i = 0; i < primes.Length; i++) {
                int prime = primes[i];
                if (prime >= min) {
                    return prime;
                }
            }

            // Outside of our predefined table. Compute the hard way. 
            for (int i = (min | 1); i < Int32.MaxValue; i += 2) {
                if (IsPrime(i)) {
                    return i;
                }
            }
            return min;
        }

        internal static int GetMinPrime() {
            return primes[0];
        }

        // Returns size of hashtable to grow to.
        internal static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;

            // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength)
                return MaxPrimeArrayLength;

            return GetPrime(newSize);
        }

        // This is the maximum prime smaller than Array.MaxArrayLength
        internal const int MaxPrimeArrayLength = 0x7FEFFFFD;
    }


    /// <summary>
    /// Implementation notes:
    /// This uses an array-based implementation similar to Dictionary<T>, using a buckets array
    /// to map hash values to the Slots array. Items in the Slots array that hash to the same value
    /// are chained together through the "next" indices. 
    /// 
    /// The capacity is always prime; so during resizing, the capacity is chosen as the next prime
    /// greater than double the last capacity. 
    /// 
    /// The underlying data structures are lazily initialized. Because of the observation that, 
    /// in practice, hashtables tend to contain only a few elements, the initial capacity is
    /// set very small (3 elements) unless the ctor with a collection is used.
    /// 
    /// The +/- 1 modifications in methods that add, check for containment, etc allow us to 
    /// distinguish a hash code of 0 from an uninitialized bucket. This saves us from having to 
    /// reset each bucket to -1 when resizing. See Contains, for example.
    /// 
    /// Set methods such as UnionWith, IntersectWith, ExceptWith, and SymmetricExceptWith modify
    /// this set.
    /// 
    /// Some operations can perform faster if we can assume "other" contains unique elements
    /// according to this equality comparer. The only times this is efficient to check is if
    /// other is a hashset. Note that checking that it's a hashset alone doesn't suffice; we
    /// also have to check that the hashset is using the same equality comparer. If other 
    /// has a different equality comparer, it will have unique elements according to its own
    /// equality comparer, but not necessarily according to ours. Therefore, to go these 
    /// optimized routes we check that other is a hashset using the same equality comparer.
    /// 
    /// A HashSet with no elements has the properties of the empty set. (See IsSubset, etc. for 
    /// special empty set checks.)
    /// 
    /// A couple of methods have a special case if other is this (e.g. SymmetricExceptWith). 
    /// If we didn't have these checks, we could be iterating over the set and modifying at
    /// the same time. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashSetCopyable<T> : ICollection<T>, ISerializable, IDeserializationCallback, IReadOnlyCollection<T>
    {
 
        // store lower 31 bits of hash code
        private const int Lower31BitMask = 0x7FFFFFFF;
        // cutoff point, above which we won't do stackallocs. This corresponds to 100 integers.
        private const int StackAllocThreshold = 100;
        // when constructing a hashset from an existing collection, it may contain duplicates, 
        // so this is used as the max acceptable excess ratio of capacity to count. Note that
        // this is only used on the ctor and not to automatically shrink if the hashset has, e.g,
        // a lot of adds followed by removes. Users must explicitly shrink by calling TrimExcess.
        // This is set to 3 because capacity is acceptable as 2x rounded up to nearest prime.
        private const int ShrinkThreshold = 3;
 
#if !SILVERLIGHT
        // constants for serialization
        private const String CapacityName = "Capacity";
        private const String ElementsName = "Elements";
        private const String ComparerName = "Comparer";
        private const String VersionName = "Version";
#endif
 
        private int[] m_buckets;
        private Slot[] m_slots;
        private int m_count;
        private int m_lastIndex;
        private int m_freeList;
        private IEqualityComparer<T> m_comparer;
        private int m_version;
 
#if !SILVERLIGHT
        // temporary variable needed during deserialization
        private SerializationInfo m_siInfo;
#endif
 
        #region Constructors
 
        public HashSetCopyable()
            : this(EqualityComparer<T>.Default) { }
 
        public HashSetCopyable(int capacity)
            : this(capacity, EqualityComparer<T>.Default) { }
 
        public HashSetCopyable(IEqualityComparer<T> comparer) {
            if (comparer == null) {
                comparer = EqualityComparer<T>.Default;
            }
 
            this.m_comparer = comparer;
            m_lastIndex = 0;
            m_count = 0;
            m_freeList = -1;
            m_version = 0;
        }
 
#if !SILVERLIGHT
        protected HashSetCopyable(SerializationInfo info, StreamingContext context) {
            // We can't do anything with the keys and values until the entire graph has been 
            // deserialized and we have a reasonable estimate that GetHashCode is not going to 
            // fail.  For the time being, we'll just cache this.  The graph is not valid until 
            // OnDeserialization has been called.
            m_siInfo = info;
        }
#endif
 
        public HashSetCopyable(int capacity, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }
            
            if (capacity > 0)
            {
                Initialize(capacity);
            }
        }

        public void CopyFrom(HashSetCopyable<T> other) {
        
            if (this.m_buckets != null) PoolArray<int>.Recycle(ref this.m_buckets);
            if (other.m_buckets != null) {

                this.m_buckets = PoolArray<int>.Spawn(other.m_buckets.Length);
                for (int i = 0; i < this.m_buckets.Length; ++i) this.m_buckets[i] = other.m_buckets[i];

            }

            if (this.m_slots != null) PoolArray<Slot>.Recycle(ref this.m_slots);
            if (other.m_slots != null) {

                this.m_slots = PoolArray<Slot>.Spawn(other.m_slots.Length);
                for (int i = 0; i < this.m_slots.Length; ++i) this.m_slots[i] = other.m_slots[i];

            }

            this.m_count = other.m_count;
            this.m_lastIndex = other.m_lastIndex;
            this.m_freeList = other.m_freeList;
            this.m_comparer = other.m_comparer;
            this.m_version = other.m_version;
            
        }

        #endregion
 
        #region ICollection<T> methods
 
        /// <summary>
        /// Add item to this hashset. This is the explicit implementation of the ICollection<T>
        /// interface. The other Add method returns bool indicating whether item was added.
        /// </summary>
        /// <param name="item">item to add</param>
        void ICollection<T>.Add(T item) {
            AddIfNotPresent(item);
        }
 
        /// <summary>
        /// Remove all items from this set. This clears the elements but not the underlying 
        /// buckets and slots array. Follow this call by TrimExcess to release these.
        /// </summary>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            if (m_lastIndex > 0) {
                // clear the elements so that the gc can reclaim the references.
                // clear only up to m_lastIndex for m_slots 
                Array.Clear(m_slots, 0, m_lastIndex);
                Array.Clear(m_buckets, 0, m_buckets.Length);
                m_lastIndex = 0;
                m_count = 0;
                m_freeList = -1;
            }
            m_version++;
        }
 
        /// <summary>
        /// Checks if this hashset contains the item
        /// </summary>
        /// <param name="item">item to check for containment</param>
        /// <returns>true if item contained; false if not</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) {
            if (m_buckets != null) {
                int hashCode = InternalGetHashCode(item);
                // see note at "HashSet" level describing why "- 1" appears in for loop
                for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next) {
                    if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, item)) {
                        return true;
                    }
                }
            }
            // either m_buckets is null or wasn't found
            return false;
        }
 
        /// <summary>
        /// Copy items in this hashset to array, starting at arrayIndex
        /// </summary>
        /// <param name="array">array to add items to</param>
        /// <param name="arrayIndex">index to start at</param>
        public void CopyTo(T[] array, int arrayIndex) {
            CopyTo(array, arrayIndex, m_count);
        }
 
        /// <summary>
        /// Remove item from this hashset
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
        public bool Remove(T item) {
            if (m_buckets != null) {
                int hashCode = InternalGetHashCode(item);
                int bucket = hashCode % m_buckets.Length;
                int last = -1;
                for (int i = m_buckets[bucket] - 1; i >= 0; last = i, i = m_slots[i].next) {
                    if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, item)) {
                        if (last < 0) {
                            // first iteration; update buckets
                            m_buckets[bucket] = m_slots[i].next + 1;
                        }
                        else {
                            // subsequent iterations; update 'next' pointers
                            m_slots[last].next = m_slots[i].next;
                        }
                        m_slots[i].hashCode = -1;
                        m_slots[i].value = default(T);
                        m_slots[i].next = m_freeList;
 
                        m_count--;
                        m_version++;
                        if (m_count == 0) {
                            m_lastIndex = 0;
                            m_freeList = -1;
                        }
                        else {
                            m_freeList = i;
                        }
                        return true;
                    }
                }
            }
            // either m_buckets is null or wasn't found
            return false;
        }
 
        /// <summary>
        /// Number of elements in this hashset
        /// </summary>
        public int Count {
            get { return m_count; }
        }
 
        /// <summary>
        /// Whether this is readonly
        /// </summary>
        bool ICollection<T>.IsReadOnly {
            get { return false; }
        }
 
        #endregion
 
        #region IEnumerable methods
 
        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }
 
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            throw new AllocationException();
        }
 
        IEnumerator IEnumerable.GetEnumerator() {
            throw new AllocationException();
        }
 
        #endregion
 
        #region ISerializable methods
 
#if !SILVERLIGHT
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
 
            // need to serialize version to avoid problems with serializing while enumerating
            info.AddValue(VersionName, m_version);
 
#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
            info.AddValue(ComparerName, HashHelpers.GetEqualityComparerForSerialization(m_comparer), typeof(IEqualityComparer<T>));
#else
            info.AddValue(ComparerName, m_comparer, typeof(IEqualityComparer<T>));
#endif
 
            info.AddValue(CapacityName, m_buckets == null ? 0 : m_buckets.Length);
            if (m_buckets != null) {
                T[] array = new T[m_count];
                CopyTo(array);
                info.AddValue(ElementsName, array, typeof(T[]));
            }
        }
#endif
        #endregion
 
        #region IDeserializationCallback methods
 
#if !SILVERLIGHT
        public virtual void OnDeserialization(Object sender) {
 
            if (m_siInfo == null) {
                // It might be necessary to call OnDeserialization from a container if the 
                // container object also implements OnDeserialization. However, remoting will 
                // call OnDeserialization again. We can return immediately if this function is 
                // called twice. Note we set m_siInfo to null at the end of this method.
                return;
            }
 
            int capacity = m_siInfo.GetInt32(CapacityName);
            m_comparer = (IEqualityComparer<T>)m_siInfo.GetValue(ComparerName, typeof(IEqualityComparer<T>));
            m_freeList = -1;
 
            if (capacity != 0) {
                m_buckets = new int[capacity];
                m_slots = new Slot[capacity];
 
                T[] array = (T[])m_siInfo.GetValue(ElementsName, typeof(T[]));
 
                if (array == null) {
                    throw new SerializationException();
                }
 
                // there are no resizes here because we already set capacity above
                for (int i = 0; i < array.Length; i++) {
                    AddIfNotPresent(array[i]);
                }
            }
            else {
                m_buckets = null;
            }
 
            m_version = m_siInfo.GetInt32(VersionName);
            m_siInfo = null;
        }
#endif
 
        #endregion
 
        #region HashSet methods
 
        /// <summary>
        /// Add item to this HashSet. Returns bool indicating whether item was added (won't be 
        /// added if already present)
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if added, false if already present</returns>
        public bool Add(T item) {
            return AddIfNotPresent(item);
        }
 
        /// <summary>
        /// Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        /// This can be useful when you want to reuse a previously stored reference instead of 
        /// a newly constructed one (so that more sharing of references can occur) or to look up
        /// a value that has more complete data than the value you currently have, although their
        /// comparer functions indicate they are equal.
        /// </remarks>
        public bool TryGetValue(T equalValue, out T actualValue) {
            if (m_buckets != null) {
                int i = InternalIndexOf(equalValue);
                if (i >= 0) {
                    actualValue = m_slots[i].value;
                    return true;
                }
            }
            actualValue = default(T);
            return false;
        }
 
        /// <summary>
        /// Take the union of this HashSet with other. Modifies this set.
        /// 
        /// Implementation note: GetSuggestedCapacity (to increase capacity in advance avoiding 
        /// multiple resizes ended up not being useful in practice; quickly gets to the 
        /// point where it's a wasteful check.
        /// </summary>
        /// <param name="other">enumerable with items to add</param>
        public void UnionWith(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }
 
            foreach (T item in other) {
                AddIfNotPresent(item);
            }
        }
 
        /// <summary>
        /// Remove items in other from this set. Modifies this set.
        /// </summary>
        /// <param name="other">enumerable with items to remove</param>
        public void ExceptWith(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }
 
            // this is already the enpty set; return
            if (m_count == 0) {
                return;
            }
 
            // special case if other is this; a set minus itself is the empty set
            if (other == this) {
                Clear();
                return;
            }
 
            // remove every element in other from this
            foreach (T element in other) {
                Remove(element);
            }
        }
        
        /// <summary>
        /// Checks if this is a superset of other
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If other has no elements (it's the empty set), then this is a superset, even if this
        /// is also the empty set.
        /// 2. If other has unique elements according to this equality comparer, and this has less 
        /// than the number of elements in other, then this can't be a superset
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a superset of other; false if not</returns>
        public bool IsSupersetOf(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }
 
            // try to fall out early based on counts
            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null) {
                // if other is the empty set then this is a superset
                if (otherAsCollection.Count == 0) {
                    return true;
                }
                HashSetCopyable<T> otherAsSet = other as HashSetCopyable<T>;
                // try to compare based on counts alone if other is a hashset with
                // same equality comparer
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet)) {
                    if (otherAsSet.Count > m_count) {
                        return false;
                    }
                }
            }
 
            return ContainsAllElements(other);
        }
 
        /// <summary>
        /// Checks if this set overlaps other (i.e. they share at least one item)
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if these have at least one common element; false if disjoint</returns>
        public bool Overlaps(IEnumerable<T> other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }
 
            if (m_count == 0) {
                return false;
            }
 
            foreach (T element in other) {
                if (Contains(element)) {
                    return true;
                }
            }
            return false;
        }
 
        public void CopyTo(T[] array) { CopyTo(array, 0, m_count); }
 
        public void CopyTo(T[] array, int arrayIndex, int count) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
 
            // check array index valid index into array
            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
 
            // also throw if count less than 0
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
 
            // will array, starting at arrayIndex, be able to hold elements? Note: not
            // checking arrayIndex >= array.Length (consistency with list of allowing
            // count of 0; subsequent check takes care of the rest)
            if (arrayIndex > array.Length || count > array.Length - arrayIndex) {
                throw new ArgumentException();
            }
 
            int numCopied = 0;
            for (int i = 0; i < m_lastIndex && numCopied < count; i++) {
                if (m_slots[i].hashCode >= 0) {
                    array[arrayIndex + numCopied] = m_slots[i].value;
                    numCopied++;
                }
            }
        }
 
        /// <summary>
        /// Remove elements that match specified predicate. Returns the number of elements removed
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveWhere(Predicate<T> match) {
            if (match == null) {
                throw new ArgumentNullException("match");
            }
 
            int numRemoved = 0;
            for (int i = 0; i < m_lastIndex; i++) {
                if (m_slots[i].hashCode >= 0) {
                    // cache value in case delegate removes it
                    T value = m_slots[i].value;
                    if (match(value)) {
                        // check again that remove actually removed it
                        if (Remove(value)) {
                            numRemoved++;
                        }
                    }
                }
            }
            return numRemoved;
        }
 
        /// <summary>
        /// Gets the IEqualityComparer that is used to determine equality of keys for 
        /// the HashSet.
        /// </summary>
        public IEqualityComparer<T> Comparer {
            get {
                return m_comparer;
            }
        }
        
        #endregion
 
        #region Helper methods
 
        /// <summary>
        /// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
        /// greater than or equal to capacity.
        /// </summary>
        /// <param name="capacity"></param>
        private void Initialize(int capacity) {
 
            int size = HashSetCopyableHashHelpers.GetPrime(capacity);
 
            m_buckets = new int[size];
            m_slots = new Slot[size];
        }
 
        /// <summary>
        /// Expand to new capacity. New capacity is next prime greater than or equal to suggested 
        /// size. This is called when the underlying array is filled. This performs no 
        /// defragmentation, allowing faster execution; note that this is reasonable since 
        /// AddIfNotPresent attempts to insert new elements in re-opened spots.
        /// </summary>
        /// <param name="sizeSuggestion"></param>
        private void IncreaseCapacity() {
            int newSize = HashSetCopyableHashHelpers.ExpandPrime(m_count);
            if (newSize <= m_count) {
                throw new ArgumentException();
            }
 
            // Able to increase capacity; copy elements to larger array and rehash
            SetCapacity(newSize, false);
        }
 
        /// <summary>
        /// Set the underlying buckets array to size newSize and rehash.  Note that newSize
        /// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
        /// instead of this method.
        /// </summary>
        private void SetCapacity(int newSize, bool forceNewHashCodes) {
            Slot[] newSlots = new Slot[newSize];
            if (m_slots != null) {
                Array.Copy(m_slots, 0, newSlots, 0, m_lastIndex);
            }
 
            if(forceNewHashCodes) {
                for(int i = 0; i < m_lastIndex; i++) {
                    if(newSlots[i].hashCode != -1) {
                        newSlots[i].hashCode = InternalGetHashCode(newSlots[i].value);
                    }
                }
            }
 
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < m_lastIndex; i++) {
                int bucket = newSlots[i].hashCode % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            m_slots = newSlots;
            m_buckets = newBuckets;
        }
 
        /// <summary>
        /// Adds value to HashSet if not contained already
        /// Returns true if added and false if already present
        /// </summary>
        /// <param name="value">value to find</param>
        /// <returns></returns>
        private bool AddIfNotPresent(T value) {
            if (m_buckets == null) {
                Initialize(0);
            }
 
            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % m_buckets.Length;
#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
            int collisionCount = 0;
#endif
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next) {
                if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, value)) {
                    return false;
                }
#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
                collisionCount++;
#endif
            }
 
            int index;
            if (m_freeList >= 0) {
                index = m_freeList;
                m_freeList = m_slots[index].next;
            }
            else {
                if (m_lastIndex == m_slots.Length) {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % m_buckets.Length;
                }
                index = m_lastIndex;
                m_lastIndex++;
            }
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
            m_count++;
            m_version++;
 
#if FEATURE_RANDOMIZED_STRING_HASHING && !FEATURE_NETCORE
            if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(m_comparer)) {
                m_comparer = (IEqualityComparer<T>) HashHelpers.GetRandomizedEqualityComparer(m_comparer);
                SetCapacity(m_buckets.Length, true);
            }
#endif // FEATURE_RANDOMIZED_STRING_HASHING
 
            return true;
        }
 
        // Add value at known index with known hash code. Used only
        // when constructing from another HashSet.
        private void AddValue(int index, int hashCode, T value) {
            int bucket = hashCode % m_buckets.Length;
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
        }
 
        /// <summary>
        /// Checks if this contains of other's elements. Iterates over other's elements and 
        /// returns false as soon as it finds an element in other that's not in this.
        /// Used by SupersetOf, ProperSupersetOf, and SetEquals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool ContainsAllElements(IEnumerable<T> other) {
            foreach (T element in other) {
                if (!Contains(element)) {
                    return false;
                }
            }
            return true;
        }
 
        /// <summary>
        /// Implementation Notes:
        /// If other is a hashset and is using same equality comparer, then checking subset is 
        /// faster. Simply check that each element in this is in other.
        /// 
        /// Note: if other doesn't use same equality comparer, then Contains check is invalid,
        /// which is why callers must take are of this.
        /// 
        /// If callers are concerned about whether this is a proper subset, they take care of that.
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool IsSubsetOfHashSetWithSameEC(HashSet<T> other) {
 
            foreach (T item in this) {
                if (!other.Contains(item)) {
                    return false;
                }
            }
            return true;
        }
 
        /// <summary>
        /// If other is a hashset that uses same equality comparer, intersect is much faster 
        /// because we can use other's Contains
        /// </summary>
        /// <param name="other"></param>
        private void IntersectWithHashSetWithSameEC(HashSet<T> other) {
            for (int i = 0; i < m_lastIndex; i++) {
                if (m_slots[i].hashCode >= 0) {
                    T item = m_slots[i].value;
                    if (!other.Contains(item)) {
                        Remove(item);
                    }
                }
            }
        }
 
        /// <summary>
        /// Used internally by set operations which have to rely on bit array marking. This is like
        /// Contains but returns index in slots array. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int InternalIndexOf(T item) {
            int hashCode = InternalGetHashCode(item);
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next) {
                if ((m_slots[i].hashCode) == hashCode && m_comparer.Equals(m_slots[i].value, item)) {
                    return i;
                }
            }
            // wasn't found
            return -1;
        }
 
        /// <summary>
        /// if other is a set, we can assume it doesn't have duplicate elements, so use this
        /// technique: if can't remove, then it wasn't present in this set, so add.
        /// 
        /// As with other methods, callers take care of ensuring that other is a hashset using the
        /// same equality comparer.
        /// </summary>
        /// <param name="other"></param>
        private void SymmetricExceptWithUniqueHashSet(HashSet<T> other) {
            foreach (T item in other) {
                if (!Remove(item)) {
                    AddIfNotPresent(item);
                }
            }
        }
 
        /// <summary>
        /// Add if not already in hashset. Returns an out param indicating index where added. This 
        /// is used by SymmetricExcept because it needs to know the following things:
        /// - whether the item was already present in the collection or added from other
        /// - where it's located (if already present, it will get marked for removal, otherwise
        /// marked for keeping)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool AddOrGetLocation(T value, out int location) {
            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % m_buckets.Length;
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next) {
                if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, value)) {
                    location = i;
                    return false; //already present
                }
            }
            int index;
            if (m_freeList >= 0) {
                index = m_freeList;
                m_freeList = m_slots[index].next;
            }
            else {
                if (m_lastIndex == m_slots.Length) {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % m_buckets.Length;
                }
                index = m_lastIndex;
                m_lastIndex++;
            }
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
            m_count++;
            m_version++;
            location = index;
            return true;
        }
 
        /// <summary>
        /// Copies this to an array. Used for DebugView
        /// </summary>
        /// <returns></returns>
        internal T[] ToArray() {
            T[] newArray = new T[Count];
            CopyTo(newArray);
            return newArray;
        }
 
        /// <summary>
        /// Internal method used for HashSetEqualityComparer. Compares set1 and set2 according 
        /// to specified comparer.
        /// 
        /// Because items are hashed according to a specific equality comparer, we have to resort
        /// to n^2 search if they're using different equality comparers.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        internal static bool HashSetEquals(HashSetCopyable<T> set1, HashSetCopyable<T> set2, IEqualityComparer<T> comparer) {
            // handle null cases first
            if (set1 == null) {
                return (set2 == null);
            }
            else if (set2 == null) {
                // set1 != null
                return false;
            }
 
            // all comparers are the same; this is faster
            if (AreEqualityComparersEqual(set1, set2)) {
                if (set1.Count != set2.Count) {
                    return false;
                }
                // suffices to check subset
                foreach (T item in set2) {
                    if (!set1.Contains(item)) {
                        return false;
                    }
                }
                return true;
            }
            else {  // n^2 search because items are hashed according to their respective ECs
                foreach (T set2Item in set2) {
                    bool found = false;
                    foreach (T set1Item in set1) {
                        if (comparer.Equals(set2Item, set1Item)) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        return false;
                    }
                }
                return true;
            }
        }
 
        /// <summary>
        /// Checks if equality comparers are equal. This is used for algorithms that can
        /// speed up if it knows the other item has unique elements. I.e. if they're using 
        /// different equality comparers, then uniqueness assumption between sets break.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        private static bool AreEqualityComparersEqual(HashSetCopyable<T> set1, HashSetCopyable<T> set2) {
            return set1.Comparer.Equals(set2.Comparer);
        }
 
        /// <summary>
        /// Workaround Comparers that throw ArgumentNullException for GetHashCode(null).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>hash code</returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int InternalGetHashCode(T item) {
            return m_comparer.GetHashCode(item) & Lower31BitMask;
        }
 
        #endregion
 
        internal struct Slot {
            internal int hashCode;      // Lower 31 bits of hash code, -1 if unused
            internal int next;          // Index of next entry, -1 if last
            internal T value;
        }
 
#if !SILVERLIGHT
        [Serializable()]
        [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
#endif
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator {
            private HashSetCopyable<T> set;
            private int index;
            private int version;
            private T current;
 
            internal Enumerator(HashSetCopyable<T> set) {
                this.set = set;
                index = 0;
                version = set.m_version;
                current = default(T);
            }
 
            public void Dispose() {
            }
 
            public bool MoveNext() {
                if (version != set.m_version) {
                    throw new InvalidOperationException();
                }
 
                while (index < set.m_lastIndex) {
                    if (set.m_slots[index].hashCode >= 0) {
                        current = set.m_slots[index].value;
                        index++;
                        return true;
                    }
                    index++;
                }
                index = set.m_lastIndex + 1;
                current = default(T);
                return false;
            }
 
            public T Current {
                get {
                    return current;
                }
            }
 
            Object System.Collections.IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }
 
            void System.Collections.IEnumerator.Reset() {
                if (version != set.m_version) {
                    throw new InvalidOperationException();
                }
 
                index = 0;
                current = default(T);
            }
        }
    }
    
}