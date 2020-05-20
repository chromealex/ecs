using System.Collections;
using System.Collections.Generic;

namespace ME.ECS {

    public interface IComponentsBase {

        IList<IComponentBase> GetData(int entityId);
        IDictionary GetDataOnce();

    }

    public interface IComponents<TState> : IComponentsBase, IPoolableRecycle where TState : class, IState<TState>, new() {

        int Count { get; }

        int RemoveAll(int entityId);
        int RemoveAll<TComponent>(int entityId) where TComponent : class, IComponent;
        int RemoveAll<TComponent>() where TComponent : class, IComponent;

        int RemoveAllPredicate<TComponent, TComponentPredicate>(int entityId, TComponentPredicate predicate) where TComponent : class, IComponent where TComponentPredicate : IComponentPredicate<TComponent>;

        void Add<TComponent>(int entityId, TComponent data) where TComponent : class, IComponent;
        bool Contains<TComponent>(int entityId) where TComponent : class, IComponent;
        List<IComponent> ForEach<TComponent>(int entityId) where TComponent : class, IComponent;
        TComponent GetFirst<TComponent>(int entityId) where TComponent : class, IComponent;

        void CopyFrom(Components<TState> other);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Components<TState> : IComponents<TState> where TState : class, IState<TState>, new() {

        private static class ComponentType<TComponent, TStateInner> {

            public static int id = -1;

        }

        public struct Bucket {

            public List<IComponent>[] components;

        }

        private Bucket[] arr; // arr by component type
        private static int typeId;
        
        private bool freeze;
        private int capacity;

        public void Initialize(int capacity) {

            this.arr = PoolArray<Bucket>.Spawn(capacity);
            
        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        void IPoolableRecycle.OnRecycle() {

            this.CleanUp_INTERNAL();
            
            this.freeze = default;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void CleanUp_INTERNAL() {

            if (this.arr == null) return;
            
            for (int i = 0; i < this.arr.Length; ++i) {

                ref var bucket = ref this.arr[i];
                if (bucket.components != null) {

                    for (int j = 0; j < bucket.components.Length; ++j) {

                        var list = bucket.components[j];
                        if (list != null) {

                            PoolComponents.Recycle(list);
                            PoolList<IComponent>.Recycle(ref list);
                            bucket.components[j] = null;

                        }

                    }

                    PoolArray<List<IComponent>>.Recycle(ref bucket.components);

                }

            }
            PoolArray<Bucket>.Recycle(ref this.arr);
            
        }

        public int Count {

            get {

                var count = 0;
                for (int i = 0; i < this.arr.Length; ++i) {

                    if (this.arr[i].components != null) count += this.arr[i].components.Length;

                }

                return count;

            }

        }

        public int RemoveAllPredicate<TComponent, TComponentPredicate>(int entityId, TComponentPredicate predicate) where TComponent : class, IComponent where TComponentPredicate : IComponentPredicate<TComponent> {

            var count = 0;
            var typeId = Components<TState>.GetTypeId<TComponent>();
            if (typeId < 0 || typeId >= this.arr.Length) return count;

            ref var bucket = ref this.arr[typeId];
            if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length) return count;

            ref var list = ref bucket.components[entityId];
            if (list == null) return count;
            
            for (int i = list.Count - 1; i >= 0; --i) {

                var tComp = list[i] as TComponent;
                if (predicate.Execute(tComp) == false) continue;
                            
                PoolComponents.Recycle(ref tComp);
                list.RemoveAt(i);
                ++count;

            }

            return count;

        }

        public int RemoveAll<TComponent>(int entityId) where TComponent : class, IComponent {
            
            return this.RemoveAll_INTERNAL<TComponent>(entityId);

        }

        public int RemoveAll<TComponent>() where TComponent : class, IComponent {

            return this.RemoveAll_INTERNAL<TComponent>();

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int RemoveAll_INTERNAL<TComponent>() where TComponent : class, IComponentBase {

            var count = 0;
            for (int j = 0; j < this.arr.Length; ++j) {

                ref var bucket = ref this.arr[j];
                if (bucket.components == null) continue;
                
                foreach (var componentList in bucket.components) {

                    var list = componentList;
                    if (list == null) continue;

                    for (int i = list.Count - 1; i >= 0; --i) {

                        if (list[i] is TComponent tComp) {

                            PoolComponents.Recycle(ref tComp);
                            list.RemoveAt(i);
                            ++count;

                        }

                    }

                }
                
            }
            
            return count;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int RemoveAll_INTERNAL<TComponent>(int entityId) where TComponent : class, IComponentBase {

            var count = 0;
            var typeId = Components<TState>.GetTypeId<TComponent>();
            if (typeId < 0 || typeId >= this.arr.Length) return count;

            ref var bucket = ref this.arr[typeId];
            if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length) return count;

            var list = bucket.components[entityId];
            if (list == null) return 0;
                    
            for (int i = list.Count - 1; i >= 0; --i) {

                var tComp = list[i] as TComponent;
                PoolComponents.Recycle(ref tComp);
                list.RemoveAt(i);
                ++count;

            }

            return count;

        }

        public int RemoveAll(int entityId) {

            var count = 0;
            for (int i = 0; i < this.arr.Length; ++i) {

                ref var bucket = ref this.arr[i];
                if (bucket.components != null && entityId >= 0 && entityId < bucket.components.Length) {

                    ref var list = ref bucket.components[entityId];
                    if (list == null) return 0;
                    
                    count += list.Count;
                    PoolComponents.Recycle(list);
                    list.Clear();
                    
                }

            }

            return count;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int GetTypeId<TComponent>() {

            if (ComponentType<TComponent, TState>.id < 0) {

                ComponentType<TComponent, TState>.id = Components<TState>.typeId++;

            }

            return ComponentType<TComponent, TState>.id;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Add_INTERNAL(int typeId, int entityId, IComponent data) {

            ArrayUtils.Resize(typeId, ref this.arr);
            
            ref var bucket = ref this.arr[typeId];
            ArrayUtils.Resize(entityId, ref bucket.components);

            if (bucket.components[entityId] == null) bucket.components[entityId] = PoolList<IComponent>.Spawn(1);
            bucket.components[entityId].Add(data);
            
        }

        public void Add<TComponent>(int entityId, TComponent data) where TComponent : class, IComponent {

            var typeId = Components<TState>.GetTypeId<TComponent>();
            this.Add_INTERNAL(typeId, entityId, data);
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private TComponent Get_INTERNAL<TComponent>(int entityId) where TComponent : class, IComponent {
            
            var typeId = Components<TState>.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return null;
                
                var list = bucket.components[entityId];
                if (list.Count > 0) return list[0] as TComponent;

            }

            return null;

        }

        public TComponent GetFirst<TComponent>(int entityId) where TComponent : class, IComponent {
            
            return this.Get_INTERNAL<TComponent>(entityId);

        }

        public Bucket[] GetAllBuckets() {
            
            return this.arr;

        }

        public List<IComponent> ForEach<TComponent>(int entityId) where TComponent : class, IComponent {
            
            var typeId = Components<TState>.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return null;
                
                return bucket.components[entityId];
                
            }

            return null;

        }

        public bool Contains<TComponent>(int entityId) where TComponent : class, IComponent {

            var typeId = Components<TState>.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return false;

                return bucket.components[entityId].Count > 0;

            }
            
            return false;

        }

        IList<IComponentBase> IComponentsBase.GetData(int entityId) {

            var list = new List<IComponentBase>();
            foreach (var bucket in this.arr) {
                
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) continue;

                list.AddRange(bucket.components[entityId]);

            }

            return list;

        }

        IDictionary IComponentsBase.GetDataOnce() {

            return null;

        }

        public void CopyFrom(Components<TState> other) {

            // Clean up current array
            this.CleanUp_INTERNAL();
            
            // Clone other array
            this.arr = PoolArray<Bucket>.Spawn(other.arr.Length);
            for (int i = 0; i < other.arr.Length; ++i) {

                ref var otherBucket = ref other.arr[i];
                if (otherBucket.components != null) {

                    this.arr[i].components = PoolArray<List<IComponent>>.Spawn(otherBucket.components.Length);
                    for (int j = 0; j < otherBucket.components.Length; ++j) {

                        ref var otherList = ref otherBucket.components[j];
                        if (otherList != null) {

                            this.arr[i].components[j] = PoolList<IComponent>.Spawn(otherList.Capacity);
                            for (int k = 0, count = otherList.Count; k < count; ++k) {

                                var element = otherList[k];
                                var type = element.GetType();
                                var comp = (IComponent)PoolComponents.Spawn(type);
                                if (comp == null) {

                                    comp = (IComponent)System.Activator.CreateInstance(type);
                                    PoolInternalBase.CallOnSpawn(comp);

                                }

                                if (comp is IComponentCopyable<TState> compCopyable) compCopyable.CopyFrom((IComponentCopyable<TState>)element);

                                this.arr[i].components[j].Add(comp);

                            }

                        } else {

                            //if (this.arr[i].components[j] != null) PoolComponents.Recycle(this.arr[i].components[j]);
                            this.arr[i].components[j] = null;

                        }

                    }

                } else {

                    this.arr[i].components = null;

                }

            }

        }
        
    }

}