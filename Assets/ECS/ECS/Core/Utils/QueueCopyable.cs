namespace ME.ECS.Collections {

    using System;
    using System.Collections.Generic;

    // A simple Queue of generic objects.  Internally it is implemented as a 
    // circular buffer, so Enqueue can be O(n).  Dequeue is O(1).
    [System.Runtime.InteropServices.ComVisible(false)]
    public class QueueCopyable<T> :
        IPoolableSpawn,
        IPoolableRecycle,
        System.Collections.ICollection,
        IReadOnlyCollection<T> where T : struct {

        private T[] array;
        private int head; // First valid element in the queue
        private int tail; // Last valid element in the queue
        private int size; // Number of elements.
        private int version;
        private int capacity;

        private const int MINIMUM_GROW = 4;
        private const int SHRINK_THRESHOLD = 32;
        private const int GROW_FACTOR = 200; // double each time
        private const int DEFAULT_CAPACITY = 4;
        private static T[] emptyArray = new T[0];

        public void CopyFrom(QueueCopyable<T> other) {

            if (this.array != null) {
                PoolArray<T>.Recycle(ref this.array);
            }

            this.array = PoolArray<T>.Spawn(other.array.Length);
            for (var i = 0; i < this.array.Length; ++i) {

                this.array[i] = other.array[i];

            }

            this.head = other.head;
            this.tail = other.tail;
            this.size = other.size;
            this.version = other.version;
            this.capacity = other.capacity;

        }

        void IPoolableSpawn.OnSpawn() {

            this.array = PoolArray<T>.Spawn(this.capacity);

        }

        void IPoolableRecycle.OnRecycle() {

            PoolArray<T>.Recycle(ref this.array);

        }

        public QueueCopyable() {
            this.array = QueueCopyable<T>.emptyArray;
        }

        public QueueCopyable(int capacity) {

            this.capacity = capacity;
            this.array = PoolArray<T>.Spawn(capacity);
            this.head = 0;
            this.tail = 0;
            this.size = 0;
        }

        public int Count {
            get {
                return this.size;
            }
        }

        bool System.Collections.ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        Object System.Collections.ICollection.SyncRoot {
            get {
                return this;
            }
        }

        public void Clear() {
            if (this.head < this.tail) {
                Array.Clear(this.array, this.head, this.size);
            } else {
                Array.Clear(this.array, this.head, this.array.Length - this.head);
                Array.Clear(this.array, 0, this.tail);
            }

            this.head = 0;
            this.tail = 0;
            this.size = 0;
            this.version++;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            var arrayLen = array.Length;
            var numToCopy = arrayLen - arrayIndex < this.size ? arrayLen - arrayIndex : this.size;
            if (numToCopy == 0) {
                return;
            }

            var firstPart = this.array.Length - this.head < numToCopy ? this.array.Length - this.head : numToCopy;
            Array.Copy(this.array, this.head, array, arrayIndex, firstPart);
            numToCopy -= firstPart;
            if (numToCopy > 0) {
                Array.Copy(this.array, 0, array, arrayIndex + this.array.Length - this.head, numToCopy);
            }
        }

        void System.Collections.ICollection.CopyTo(Array array, int index) {
            var arrayLen = array.Length;

            var numToCopy = arrayLen - index < this.size ? arrayLen - index : this.size;
            if (numToCopy == 0) {
                return;
            }

            try {
                var firstPart = this.array.Length - this.head < numToCopy ? this.array.Length - this.head : numToCopy;
                Array.Copy(this.array, this.head, array, index, firstPart);
                numToCopy -= firstPart;

                if (numToCopy > 0) {
                    Array.Copy(this.array, 0, array, index + this.array.Length - this.head, numToCopy);
                }
            } catch (ArrayTypeMismatchException) { }
        }

        public void Enqueue(T item) {
            if (this.size == this.array.Length) {
                var newcapacity = (int)((long)this.array.Length * QueueCopyable<T>.GROW_FACTOR / 100);
                if (newcapacity < this.array.Length + QueueCopyable<T>.MINIMUM_GROW) {
                    newcapacity = this.array.Length + QueueCopyable<T>.MINIMUM_GROW;
                }

                this.SetCapacity(newcapacity);
            }

            this.array[this.tail] = item;
            this.tail = (this.tail + 1) % this.array.Length;
            this.size++;
            this.version++;
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            throw new AllocationException();
            //return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new AllocationException();
            //return new Enumerator(this);
        }

        public T Dequeue() {
            var removed = this.array[this.head];
            this.array[this.head] = default(T);
            this.head = (this.head + 1) % this.array.Length;
            this.size--;
            this.version++;
            return removed;
        }

        public T Peek() {
            return this.array[this.head];
        }

        public bool Contains(T item) {
            var index = this.head;
            var count = this.size;

            var c = EqualityComparer<T>.Default;
            while (count-- > 0) {
                if (c.Equals(this.array[index], item)) {
                    return true;
                }

                index = (index + 1) % this.array.Length;
            }

            return false;
        }

        internal T GetElement(int i) {
            return this.array[(this.head + i) % this.array.Length];
        }

        private void SetCapacity(int capacity) {
            var newarray = PoolArray<T>.Spawn(capacity);
            if (this.size > 0) {
                if (this.head < this.tail) {
                    Array.Copy(this.array, this.head, newarray, 0, this.size);
                } else {
                    Array.Copy(this.array, this.head, newarray, 0, this.array.Length - this.head);
                    Array.Copy(this.array, 0, newarray, this.array.Length - this.head, this.tail);
                }
            }

            if (this.array != null) {
                PoolArray<T>.Recycle(ref this.array);
            }

            this.array = newarray;
            this.head = 0;
            this.tail = this.size == capacity ? 0 : this.size;
            this.version++;
        }

        public void TrimExcess() {
            var threshold = (int)(this.array.Length * 0.9d);
            if (this.size < threshold) {
                this.SetCapacity(this.size);
            }
        }

        // Implements an enumerator for a Queue.  The enumerator uses the
        // internal version number of the list to ensure that no modifications are
        // made to the list while an enumeration is in progress.
        public struct Enumerator : IEnumerator<T> {

            private QueueCopyable<T> _q;
            private int index; // -1 = not started, -2 = ended/disposed
            private T currentElement;

            internal Enumerator(QueueCopyable<T> q) {
                this._q = q;
                this.index = -1;
                this.currentElement = default(T);
            }

            public void Dispose() {
                this.index = -2;
                this.currentElement = default(T);
            }

            public bool MoveNext() {

                if (this.index == -2) {
                    return false;
                }

                this.index++;

                if (this.index == this._q.size) {
                    this.index = -2;
                    this.currentElement = default(T);
                    return false;
                }

                this.currentElement = this._q.GetElement(this.index);
                return true;
            }

            public T Current {
                get {
                    return this.currentElement;
                }
            }

            Object System.Collections.IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            void System.Collections.IEnumerator.Reset() {
                this.index = -1;
                this.currentElement = default(T);
            }

        }

    }

}