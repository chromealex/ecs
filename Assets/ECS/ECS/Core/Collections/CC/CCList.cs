namespace ME.ECS.Collections {
    
    using System;
    using System.Threading;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class CCList<T> : ThreadSafeList<T>, IPoolableRecycle {

        private static readonly int[] sizes;
        private static readonly int[] counts;

        static CCList() {
            CCList<T>.sizes = new int[32];
            CCList<T>.counts = new int[32];

            var size = 1;
            var count = 1;
            for (var i = 0; i < CCList<T>.sizes.Length; i++) {
                CCList<T>.sizes[i] = size;
                CCList<T>.counts[i] = count;

                if (i < CCList<T>.sizes.Length - 1) {
                    size *= 2;
                    count += size;
                }
            }
        }

        private int index;
        private int fuzzyCount;
        private int count;
        internal readonly T[][] array;

        public CCList() {
            this.array = new T[32][];
        }

        public void ClearNoCC() {
            
            // do we really need to clean it up?
            for (int i = 0; i < this.array.Length; ++i) {
                
                ArrayUtils.Clear(this.array[i]);
                
            }
            
            this.index = 0;
            this.count = 0;
            this.fuzzyCount = 0;
            
        }

        public void OnRecycle() {
            
            for (int i = 0; i < this.array.Length; ++i) {

                if (this.array[i] != null) {
                    
                    PoolArray<T>.Release(ref this.array[i]);
                    
                }
                
            }

            this.index = 0;
            this.count = 0;
            this.fuzzyCount = 0;

        }
        
        public void InitialCopyOf(CCList<T> other) {

            for (int i = 0; i < other.array.Length; ++i) {

                if (other.array[i] != null) {
                    
                    if (this.array[i] != null) PoolArray<T>.Release(ref this.array[i]);
                    this.array[i] = PoolArray<T>.Claim(other.array[i].Length);
                    
                } else {
                    
                    PoolArray<T>.Release(ref this.array[i]);
                    
                }
                
                ArrayUtils.Clear(this.array[i]);
                
            }
            
            this.index = other.index;
            this.count = other.count;
            this.fuzzyCount = other.fuzzyCount;

        }

        public override T this[int index] {
            get {
                
                if (index < 0 || index >= this.count) {
                    
                    throw new ArgumentOutOfRangeException("index");
                    
                }

                var arrayIndex = CCList<T>.GetArrayIndex(index + 1);
                if (arrayIndex > 0) {
                    index -= ((1 << arrayIndex) - 1);
                }

                return this.array[arrayIndex][index];
                
            }
            set {
                throw new NotSupportedException();
            }
        }

        public override int Count {
            get {
                return this.count;
            }
        }

        public void RemoveAt(int index) {
            
            if (index < 0 || index >= this.count) {
                    
                throw new ArgumentOutOfRangeException("index");
                    
            }
            
            var arrayIndex = CCList<T>.GetArrayIndex(index + 1);
            if (arrayIndex > 0) {
                index -= ((1 << arrayIndex) - 1);
            }

            --this.index;
            --this.count;
            --this.fuzzyCount;
            var arr = this.array[arrayIndex];
            System.Array.Copy(arr, index + 1, arr, index, arr.Length - 1 - index);
            arr[arr.Length - 1] = default;

        }

        public override void Add(T element) {
            var index = Interlocked.Increment(ref this.index) - 1;
            var adjustedIndex = index;

            var arrayIndex = CCList<T>.GetArrayIndex(index + 1);
            if (arrayIndex > 0) {
                adjustedIndex -= CCList<T>.counts[arrayIndex - 1];
            }

            if (this.array[arrayIndex] == null) {
                var arrayLength = CCList<T>.sizes[arrayIndex];
                Interlocked.CompareExchange(ref this.array[arrayIndex], PoolArray<T>.Claim(arrayLength), null);
            }

            this.array[arrayIndex][adjustedIndex] = element;

            var count = this.count;
            var fuzzyCount = Interlocked.Increment(ref this.fuzzyCount);
            if (fuzzyCount == index + 1) {
                Interlocked.CompareExchange(ref this.count, fuzzyCount, count);
            }
        }

        public override void CopyTo(T[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            var count = this.count;
            if (array.Length - index < count) {
                throw new ArgumentException("There is not enough available space in the destination array.");
            }

            var arrayIndex = 0;
            var elementsRemaining = count;
            while (elementsRemaining > 0) {
                var source = this.array[arrayIndex++];
                var elementsToCopy = Math.Min(source.Length, elementsRemaining);
                var startIndex = count - elementsRemaining;

                Array.Copy(source, 0, array, startIndex, elementsToCopy);

                elementsRemaining -= elementsToCopy;
            }
        }

        private static int GetArrayIndex(int count) {
            var arrayIndex = 0;

            if ((count & 0xFFFF0000) != 0) {
                count >>= 16;
                arrayIndex |= 16;
            }

            if ((count & 0xFF00) != 0) {
                count >>= 8;
                arrayIndex |= 8;
            }

            if ((count & 0xF0) != 0) {
                count >>= 4;
                arrayIndex |= 4;
            }

            if ((count & 0xC) != 0) {
                count >>= 2;
                arrayIndex |= 2;
            }

            if ((count & 0x2) != 0) {
                count >>= 1;
                arrayIndex |= 1;
            }

            return arrayIndex;
        }

        #region "Protected methods"
        protected override bool IsSynchronizedBase {
            get {
                return false;
            }
        }
        #endregion

    }

}