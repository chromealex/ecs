namespace ME.ECS.Collections {
    
    using System.Collections.Generic;

    public interface IRefList {

        int Capacity { get; }
        int UsedCount { get; }
        int FromIndex { get; }
        int SizeCount { get; }
        void Clear();
        bool RemoveAt(int index);
        int GetNextIndex();

        bool IsFree(int index);
        
        TData Get<TData>(int index);
        void Set<TData>(int index, TData data);

    }

    public class RefList<T> : IRefList, IPoolableRecycle, IPoolableSpawn {
        
        private T[] arr;
        // TODO: Add int[] next array which determine next index in arr (useful for enumeration)
        private int count;
        private int size;
        private int capacity;
        private int fromIndex;
        private HashSetCopyable<int> free;
        private int initCapacity;

        public RefList() : this(4) {}

        public RefList(int capacity) {

            this.initCapacity = capacity;
            this.Init(capacity);
            
        }
        
        void IPoolableSpawn.OnSpawn() {

            var capacity = this.initCapacity;
            this.Init(capacity);
            
        }

        private void Init(int capacity) {
            
            if (capacity < 0) capacity = 0;

            this.count = 0;
            this.size = 0;
            this.capacity = -1;
            this.fromIndex = 0;
            if (this.free == null) this.free = PoolHashSetCopyable<int>.Spawn(capacity);
            this.free.Clear();
            this.Resize_INTERNAL(capacity);

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<T>.Recycle(ref this.arr);
            PoolHashSetCopyable<int>.Recycle(ref this.free);
            
        }

        public int UsedCount {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.count;
            }
        }

        public int FromIndex {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.fromIndex;
            }
        }

        public int SizeCount {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.size;
            }
        }

        public int Capacity {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.capacity;
            }
        }

        public ref T this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.arr[index];
            }
        }

        public TData Get<TData>(int index) {

            return (TData)(object)this.arr[index];

        }

        public void Set<TData>(int index, TData data) {

            this.arr[index] = (T)(object)data;

        }

        public void SetData(int index, T data) {

            this.arr[index] = data;

        }

        public void Clear() {

            for (int i = 0; i < this.size; ++i) {
                
                if (this.free.Contains(i) == false) this.free.Add(i);
                
            }

            if (this.size > 0) System.Array.Clear(this.arr, 0, this.size);
            this.size = 0;
            this.fromIndex = 0;
            this.count = 0;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int PeekFree() {
            
            var ienum = this.free.GetEnumerator();
            ienum.MoveNext();
            var peek = ienum.Current;
            ienum.Dispose();
            return peek;

        }

        public int GetNextIndex() {
            
            int nextIndex = -1;
            if (this.free.Count > 0) {

                nextIndex = this.PeekFree();
                
            } else {

                nextIndex = this.size;
                
            }

            return nextIndex;

        }

        public int Add(T data) {
            
            int nextIndex = -1;
            if (this.free.Count > 0) {
                
                nextIndex = this.PeekFree();
                this.free.Remove(nextIndex);

            } else {
                
                nextIndex = this.size;
                
            }
            
            this.Resize_INTERNAL(nextIndex + 1);
            this.arr[nextIndex] = data;
            this.UpdateFromTo(nextIndex);
            ++this.count;
            
            return nextIndex;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateFromTo(int focusIndex) {

            this.size = (focusIndex >= this.size ? focusIndex + 1 : this.size);
            this.fromIndex = (focusIndex < this.fromIndex ? focusIndex : this.fromIndex);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsFree(int index) {

            return this.free.Contains(index);

        }

        public bool RemoveAt(int index) {

            if (this.IsFree(index) == false) {

                this.arr[index] = default;
                this.free.Add(index);
                --this.count;

                if (index == this.size - 1) {

                    while (index >= 0 && this.IsFree(index) == true) {

                        --index;
                        --this.size;

                    }
                    
                }

                var fromIndex = 0;
                while (fromIndex < this.size - 1 && this.IsFree(fromIndex) == true) {

                    ++fromIndex;

                }

                if (fromIndex >= this.size - 1) {

                    fromIndex = 0;

                }
                this.fromIndex = fromIndex;

                return true;

            }

            return false;

        }

        public void CopyFrom(RefList<T> other) {

            PoolArray<T>.Copy(other.arr, ref this.arr);
            
            if (this.free != null) PoolHashSetCopyable<int>.Recycle(ref this.free);
            this.free = PoolHashSetCopyable<int>.Spawn(other.free.Count);
            this.free.CopyFrom(other.free);

            this.size = other.size;
            this.capacity = other.capacity;
            this.count = other.count;
            this.fromIndex = other.fromIndex;
            this.initCapacity = other.initCapacity;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Resize_INTERNAL(int newLength) {

            if (newLength > this.capacity) {
                
                var oldCapacity = this.capacity;
                if (this.capacity > 0) {
                    
                    this.capacity *= 2;
                    
                } else {

                    this.capacity = newLength;
                    oldCapacity = 0;

                }

                PoolArray<T>.Resize(this.capacity - 1, ref this.arr);
                
                for (int i = oldCapacity; i < this.capacity; ++i) {

                    this.free.Add(i);

                }

            }

        }

        public override string ToString() {
            
            return "RefList(Used: " + this.UsedCount.ToString() + ", Size: [" + this.FromIndex.ToString() + "-" + this.SizeCount.ToString() + "], Capacity: " + this.Capacity.ToString() + ") ";
            
        }

    }

}