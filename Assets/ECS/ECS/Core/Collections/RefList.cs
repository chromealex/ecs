using System.Linq;

namespace ME.ECS.Collections {
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class RefList<T> : IPoolableRecycle, IPoolableSpawn {
        
        private BufferArray<T> arr;
        // TODO: Add int[] next array which determine next index in arr (useful for enumeration)
        private int count;
        private int size;
        private int capacity;
        private int fromIndex;
        private QueueCopyable<int> freePeek;
        private HashSetCopyable<int> free;
        private HashSetCopyable<int> freePrepared;
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
            if (this.freePeek == null) this.freePeek = PoolQueueCopyable<int>.Spawn(capacity);
            this.freePeek.Clear();
            if (this.free == null) this.free = PoolHashSetCopyable<int>.Spawn(capacity);
            this.free.Clear();
            if (this.freePrepared == null) this.freePrepared = PoolHashSetCopyable<int>.Spawn(capacity);
            this.freePrepared.Clear();
            this.Resize_INTERNAL(capacity);

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<T>.Recycle(ref this.arr);
            PoolQueueCopyable<int>.Recycle(ref this.freePeek);
            PoolHashSetCopyable<int>.Recycle(ref this.free);
            PoolHashSetCopyable<int>.Recycle(ref this.freePrepared);
            
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
                return ref this.arr.arr[index];
            }
        }

        public TData Get<TData>(int index) {

            return (TData)(object)this.arr.arr[index];

        }

        public void Set<TData>(int index, TData data) {

            this.arr.arr[index] = (T)(object)data;

        }

        public void SetData(int index, T data) {

            this.arr.arr[index] = data;

        }

        public void Clear() {

            for (int i = 0; i < this.size; ++i) {

                if (this.free.Contains(i) == false) {
                    
                    this.free.Add(i);
                    this.freePeek.Enqueue(i);
                    
                }
                
            }

            this.freePrepared.Clear();

            if (this.size > 0) System.Array.Clear(this.arr.arr, 0, this.size);
            this.size = 0;
            this.fromIndex = 0;
            this.count = 0;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int PeekFree() {

            return this.freePeek.Peek();

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
                this.freePeek.Dequeue();
                this.free.Remove(nextIndex);
                
            } else {
                
                this.Resize_INTERNAL(this.size + 1);
                nextIndex = this.PeekFree();
                this.freePeek.Dequeue();
                this.free.Remove(nextIndex);

            }
            
            this.arr.arr[nextIndex] = data;
            this.UpdateFromTo(nextIndex);
            ++this.count;
            
            return nextIndex;
            
        }

        public void ApplyPrepared() {

            foreach (var item in this.freePrepared) {

                this.free.Add(item);
                this.freePeek.Enqueue(item);

            }
            
            this.freePrepared.Clear();
            
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

                this.freePrepared.Add(index);
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

            ArrayUtils.Copy(in other.arr, ref this.arr);
            
            if (this.freePrepared != null) PoolHashSetCopyable<int>.Recycle(ref this.freePrepared);
            this.freePrepared = PoolHashSetCopyable<int>.Spawn(other.freePrepared.Count);
            this.freePrepared.CopyFrom(other.freePrepared);

            if (this.freePeek != null) PoolQueueCopyable<int>.Recycle(ref this.freePeek);
            this.freePeek = PoolQueueCopyable<int>.Spawn(other.freePeek.Count);
            this.freePeek.CopyFrom(other.freePeek);

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

                ArrayUtils.Resize(this.capacity, ref this.arr);
                
                for (int i = oldCapacity; i < this.capacity; ++i) {

                    this.free.Add(i);
                    this.freePeek.Enqueue(i);
                    
                }

            }

        }

        public override string ToString() {
            
            return "RefList(Used: " + this.UsedCount.ToString() + ", Size: [" + this.FromIndex.ToString() + "-" + this.SizeCount.ToString() + "], Capacity: " + this.Capacity.ToString() + ") ";
            
        }

    }

}