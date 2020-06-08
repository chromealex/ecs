using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace ME.ECS.Collections {

    [System.Serializable]
    public readonly struct BufferArray<T> : System.IEquatable<BufferArray<T>> {

        public readonly T[] arr;
        public readonly int Length;
        public readonly bool isEmpty;

        public Enumerator GetEnumerator() {
            
            return new Enumerator(this);
            
        }
        
        public struct Enumerator : IEnumerator<T> {

            private BufferArray<T> bufferArray;
            private int index;
            
            public Enumerator(BufferArray<T> bufferArray) {

                this.bufferArray = bufferArray;
                this.index = 0;

            }

            object IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            public T Current {
                get {
                    return this.bufferArray[this.index];
                }
            }

            public bool MoveNext() {

                ++this.index;
                if (this.index >= this.bufferArray.Length) return false;
                return true;

            }

            public void Reset() {}

            public void Dispose() {}
            
        }
        
        public static BufferArray<T> Empty {
            get {
                return new BufferArray<T>(null, 0);
            }
        }

        public static BufferArray<T> From(T[] arr) {

            var length = arr.Length;
            var buffer = PoolArray<T>.Spawn(length);
            ArrayUtils.Copy(new BufferArray<T>(arr, length), ref buffer);
            
            return buffer;
            
        }

        public static BufferArray<T> From(IList<T> arr) {

            var length = arr.Count;
            var buffer = PoolArray<T>.Spawn(length);
            ArrayUtils.Copy(arr, ref buffer);
            
            return buffer;
            
        }

        internal BufferArray(T[] arr, int length) {

            this.arr = arr;
            this.Length = length;
            this.isEmpty = (length == 0 || arr == null);

        }

        public BufferArray<T> Dispose() {

            var arr = this.arr;
            PoolArray<T>.Recycle(ref arr);
            return new BufferArray<T>(null, 0);

        }

        public ref T this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.arr[index];
            }
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BufferArray<T> e1, BufferArray<T> e2) {

            return e1.arr == e2.arr;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BufferArray<T> e1, BufferArray<T> e2) {

            return !(e1 == e2);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(BufferArray<T> other) {

            return this == other;

        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {

            if (this.arr == null) return 0;
            return this.arr.GetHashCode();
            
        }

        public override string ToString() {

            var content = string.Empty;
            for (int i = 0; i < this.Length; ++i) {
                content += "[" + i + "] " + this[i] + "\n";
            } 
            return "BufferArray<>[" + this.Length + "]:\n" + content;
            
        }

    }
    
}
