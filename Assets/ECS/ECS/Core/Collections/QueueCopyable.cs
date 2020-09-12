namespace ME.ECS.Collections {

    using System;
    using System.Collections.Generic;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class QueueCopyable<T> : IPoolableSpawn, IPoolableRecycle where T : struct {

        private BufferArray<T> innerArray;
        private int head;
        private int tail;
        public int Count { get; private set; }
        public int Capacity { get; private set; }

        public QueueCopyable() : this(8) { }

        public QueueCopyable(int capacity) {
            this.Capacity = capacity;
            this.head = 0;
            this.tail = 0;
            this.Count = 0;
            this.innerArray = PoolArray<T>.Spawn(this.Capacity);
        }

        public void OnSpawn() {

            this.innerArray = PoolArray<T>.Spawn(this.Capacity);
            this.head = 0;
            this.tail = 0;
            this.Count = 0;

        }

        public void OnRecycle() {

            PoolArray<T>.Recycle(ref this.innerArray);
            this.head = 0;
            this.tail = 0;
            this.Count = 0;

        }

        public void CopyFrom(QueueCopyable<T> other) {

            ArrayUtils.Copy(other.innerArray, ref this.innerArray);
            this.head = other.head;
            this.tail = other.tail;
            this.Count = other.Count;
            this.Capacity = other.Capacity;

        }

        public void Enqueue(T item) {
            if (this.tail == this.head) {
                this.SetCapacity(this.Count + 1);
            }

            this.innerArray.arr[this.tail++] = item;
            if (this.tail == this.Capacity) {
                this.tail = 0;
            }

            this.Count++;
        }

        public T Dequeue() {
            var ret = this.innerArray.arr[this.head];
            this.innerArray.arr[this.head] = default(T);
            this.head++;
            if (this.head == this.Capacity) {
                this.head = 0;
            }

            this.Count--;
            return ret;
        }

        public void RemoveHead() {
            this.innerArray.arr[this.head] = default(T);
            this.head++;
            if (this.head == this.Capacity) {
                this.head = 0;
            }

            this.Count--;
        }

        public T Peek() {
            return this.innerArray.arr[this.head];
        }

        public T PeekTail() {
            var tailIndex = this.tail - 1;
            if (tailIndex < 0) {
                tailIndex = this.Capacity - 1;
            }

            return this.innerArray.arr[tailIndex];
        }

        public void SetCapacity(int min) {
            if (this.Capacity < min) {
                var prevLength = this.Capacity;
                this.Capacity *= 2;
                if (this.Capacity < min) {
                    this.Capacity = min;
                }

                var newArray = PoolArray<T>.Spawn(this.Capacity);
                if (this.tail > this.head) { // If we are not wrapped around...
                    Array.Copy(this.innerArray.arr, this.head, newArray.arr, 0, this.Count); // ...take from head to head+Count and copy to beginning of new array
                } else if (this.Count > 0) { // Else if we are wrapped around... (tail == head is ambiguous - could be an empty buffer or a full one)
                    Array.Copy(this.innerArray.arr, this.head, newArray.arr, 0, prevLength - this.head); // ...take head to end and copy to beginning of new array
                    Array.Copy(this.innerArray.arr, 0, newArray.arr, prevLength - this.head, this.tail); // ...take beginning to tail and copy after previously copied elements
                }

                this.head = 0;
                this.tail = this.Count;
                if (this.innerArray.isNotEmpty == true) {
                    PoolArray<T>.Recycle(ref this.innerArray);
                }

                this.innerArray = newArray;
            }
        }

        public void Clear() {
            ArrayUtils.Clear(this.innerArray);
            this.FastClear();
        }

        public void FastClear() {
            this.Count = 0;
            this.tail = 0;
            this.head = 0;
        }

        public BufferArray<T> ToArray() {
            var result = PoolArray<T>.Spawn(this.Count);
            if (this.tail > this.head) {
                Array.Copy(this.innerArray.arr, this.head, result.arr, 0, this.Count);
            } else if (this.Count > 0) {
                Array.Copy(this.innerArray.arr, this.head, result.arr, 0, this.Capacity - this.head);
                Array.Copy(this.innerArray.arr, 0, result.arr, this.Capacity - this.head, this.tail);
            }

            return result;
        }

    }

}