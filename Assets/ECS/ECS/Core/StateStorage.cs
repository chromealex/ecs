using System.Collections;
using System.Collections.Generic;

namespace ME.ECS {
    
    using ME.ECS.Collections;
    
    public interface IStorage : IPoolableRecycle, IEnumerable {

        int Count {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
        }

        int FromIndex {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
        }

        int ToIndex {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
        }
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        ref RefList<Entity> GetData();
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool IsFree(int index);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class Storage : IStorage {

        public struct StorageEnumerator : IEnumerator<int> {

            private Storage storage;
            private int index;

            public StorageEnumerator(Storage storage) {
                
                this.storage = storage;
                this.index = this.storage.ToIndex;

            }

            public int Current {
                get {
                    return this.index;
                }
            }

            public bool MoveNext() {

                do {
                    --this.index;
                } while (this.storage.IsFree(this.index) == true);
                return this.index >= this.storage.FromIndex;

            }

            public void Reset() {

                this.index = this.storage.ToIndex;

            }

            object IEnumerator.Current {
                get {
                    throw new AllocationException();
                }
            }

            bool IEnumerator.MoveNext() {

                throw new AllocationException();

            }

            int IEnumerator<int>.Current {
                get {
                    return this.index;
                }
            }

            void System.IDisposable.Dispose() {
                
            }

        }

        private RefList<Entity> list;
        private bool freeze;
        internal ArchetypeEntities archetypes;

        void IPoolableRecycle.OnRecycle() {

            PoolClass<ArchetypeEntities>.Recycle(ref this.archetypes);
            
            if (this.list != null) PoolRefList<Entity>.Recycle(ref this.list);
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

        public ref Entity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.list[index];
            }
        }

        public void ApplyPrepared() {
            
            this.list.ApplyPrepared();
            
        }
        
        IEnumerator IEnumerable.GetEnumerator() {

            throw new AllocationException();

        }

        public StorageEnumerator GetEnumerator() {

            return new StorageEnumerator(this);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsFree(int index) {

            return this.list.IsFree(index);

        }

        public void Initialize(int capacity) {
            
            this.list = PoolRefList<Entity>.Spawn(capacity);
            this.archetypes = PoolClass<ArchetypeEntities>.Spawn();

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Storage other) {
            
            this.archetypes.CopyFrom(other.archetypes);
            if (this.list != null) PoolRefList<Entity>.Recycle(ref this.list);
            this.list = PoolRefList<Entity>.Spawn(other.list.Capacity);
            this.list.CopyFrom(other.list);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref RefList<Entity> GetData() {

            return ref this.list;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetData(RefList<Entity> data) {

            if (this.freeze == false && data != null && this.list != data) {

                if (this.list != null) PoolRefList<Entity>.Recycle(ref this.list);
                this.list = data;

            }

        }

        public override string ToString() {
            
            return "Storage Count: " + this.list.ToString();
            
        }

    }

}