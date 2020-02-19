using System.Collections;
using System.Collections.Generic;

namespace ME.ECS {
    
    using ME.ECS.Collections;
    
    public interface IStorage : IPoolableRecycle {

        int Count { get; }
        IRefList GetData();

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Storage<TEntity> : IStorage, IEnumerable<int> where TEntity : struct, IEntity {

        private class StorageEnumerator : IEnumerator<int> {

            private Storage<TEntity> storage;
            private int index;

            public void SetInfo(Storage<TEntity> storage) {
                
                this.storage = storage;
                this.index = this.storage.ToIndex;

            }

            object IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            bool IEnumerator.MoveNext() {

                do {
                    --this.index;
                } while (this.storage.IsFree(this.index) == true);
                return this.index >= this.storage.FromIndex;

            }

            void IEnumerator.Reset() {

                this.index = this.storage.ToIndex;

            }

            int IEnumerator<int>.Current {
                get {
                    return this.index;
                }
            }

            void System.IDisposable.Dispose() {
                
                PoolClass<StorageEnumerator>.Recycle(this);
                
            }

        }

        private RefList<TEntity> list;
        private bool freeze;

        void IPoolableRecycle.OnRecycle() {
            
            PoolRefList<TEntity>.Recycle(ref this.list);
            this.freeze = false;

        }

        public int Count {

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {

                return this.list.SizeCount;

            }

        }

        public int FromIndex {

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {

                return this.list.FromIndex;

            }

        }

        public int ToIndex {

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {

                return this.list.SizeCount;

            }

        }

        public ref TEntity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.list[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {

            throw new AllocationException();

        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator() {

            var instance = PoolClass<StorageEnumerator>.Spawn();
            instance.SetInfo(this);
            return instance;

        }

        public bool IsFree(int index) {

            return this.list.IsFree(index);

        }

        public void Initialize(int capacity) {
            
            this.list = PoolRefList<TEntity>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Storage<TEntity> other) {
            
            if (this.list != null) PoolRefList<TEntity>.Recycle(ref this.list);
            this.list = PoolRefList<TEntity>.Spawn(other.list.Capacity);
            this.list.CopyFrom(other.list);

        }

        IRefList IStorage.GetData() {

            return this.list;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetData(RefList<TEntity> data) {

            if (this.freeze == false && data != null && this.list != data) {

                if (this.list != null) PoolRefList<TEntity>.Recycle(ref this.list);
                this.list = data;

            }

        }

        public override string ToString() {
            
            return "Storage Count: " + this.list.ToString();
            
        }

    }

}