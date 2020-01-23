using System.Collections.Generic;

namespace ME.ECS {
    
    public interface IFilter : IPoolableRecycle { }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Filter<T> : IFilter where T : IEntity {

        private List<T> list;
        private bool freeze;

        void IPoolableRecycle.OnRecycle() {
            
            PoolList<T>.Recycle(ref this.list);
            this.freeze = false;

        }

        public int Count {

            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {

                return this.list.Count;

            }

        }
        
        public T this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.list[index];
            }
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            set {
                this.list[index] = value;
            }
        }

        public void Initialize(int capacity) {
            
            this.list = PoolList<T>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Filter<T> other) {
            
            if (this.list != null) PoolList<T>.Recycle(ref this.list);
            this.list = PoolList<T>.Spawn(other.list.Capacity);
            this.list.AddRange(other.list);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public List<T> GetData() {

            return this.list;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetData(List<T> data) {

            if (this.freeze == false && data != null && this.list != data) {

                if (this.list != null) PoolList<T>.Recycle(ref this.list);
                this.list = data;

            }

        }

    }

}