using System.Collections;
using System.Collections.Generic;
using EntityId = System.Int32;

namespace ME.ECS {

    public interface IComponentsBase {

        IDictionary GetData();
        IDictionary GetDataOnce();

    }

    public interface IComponents<TState> : IComponentsBase, IPoolableRecycle where TState : class, IState<TState> {

        int Count { get; }
        int CountOnce { get; }

        void RemoveAll(EntityId entityId);
        void RemoveAll<TComponent>(EntityId entityId) where TComponent : class, IComponentBase;
        void RemoveAllOnce<TComponent>(EntityId entityId) where TComponent : class, IComponentOnceBase;
        void RemoveAll<TComponent>() where TComponent : class, IComponentBase;
        void RemoveAllOnce<TComponent>() where TComponent : class, IComponentOnceBase;

        void RemoveAllPredicate<TComponent, TComponentPredicate>(EntityId entityId, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent>;

    }

    public static class ComponentExtensions {

        public static bool GetEntityData<TState, TEntity, TEntitySource>(this IComponent<TState, TEntitySource> _, Entity entity, out TEntity data) where TEntity : struct, IEntity where TEntitySource : struct, IEntity where TState : class, IState<TState>, new() {

            ref var world = ref Worlds<TState>.currentWorld;
            return world.GetEntityData(entity, out data);

        }

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Components<TEntity, TState> : IComponents<TState> where TEntity : struct, IEntity where TState : class, IState<TState> {

        private struct InternalComponentPredicateTrue<TComponent> : IComponentPredicate<TComponent> where TComponent : class, IComponentBase {

            public bool Execute(TComponent data) {
                return true;
            }

        }

        private Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dic;
        private Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dicOnce;
        private bool freeze;
        private int capacity;

        void IPoolableRecycle.OnRecycle() {

            foreach (var item in this.dic) {
                
                PoolComponents.Recycle(item.Value);
                PoolHashSet<IComponent<TState, TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Recycle(ref this.dic);

            foreach (var item in this.dicOnce) {
                
                PoolComponents.Recycle(item.Value);
                PoolHashSet<IComponent<TState, TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Recycle(ref this.dicOnce);

            this.freeze = false;
            this.capacity = 0;

        }

        public int Count {

            get {

                var count = 0;
                foreach (var item in this.dic) {

                    count += item.Value.Count;

                }

                return count;

            }

        }

        public int CountOnce {

            get {

                var count = 0;
                foreach (var item in this.dicOnce) {

                    count += item.Value.Count;

                }

                return count;

            }

        }

        public void RemoveAllPredicate<TComponent, TComponentPredicate>(EntityId entityId, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                this.RemoveAll_INTERNAL<TComponent, TComponentPredicate>(list, predicate);
                
            }
            
        }

        public void RemoveAll<TComponent>(EntityId entityId) where TComponent : class, IComponentBase {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                this.RemoveAll_INTERNAL<TComponent>(list);
                
            }
            
        }

        public void RemoveAllOnce<TComponent>(EntityId entityId) where TComponent : class, IComponentOnceBase {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                this.RemoveAll_INTERNAL<TComponent>(list);
                
            }
            
        }

        public void RemoveAll<TComponent>() where TComponent : class, IComponentBase {

            this.RemoveAll_INTERNAL<TComponent>(this.dic);

        }
        
        public void RemoveAllOnce<TComponent>() where TComponent : class, IComponentOnceBase {

            this.RemoveAll_INTERNAL<TComponent>(this.dicOnce);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RemoveAll_INTERNAL<TComponent>(Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dic) where TComponent : class, IComponentBase {
            
            foreach (var item in dic) {

                var list = item.Value;
                this.RemoveAll_INTERNAL<TComponent>(list);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RemoveAll_INTERNAL<TComponent>(HashSet<IComponent<TState, TEntity>> list) where TComponent : class, IComponentBase {
            
            this.RemoveAll_INTERNAL<TComponent, InternalComponentPredicateTrue<TComponent>>(list, new InternalComponentPredicateTrue<TComponent>());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RemoveAll_INTERNAL<TComponent, TComponentPredicate>(HashSet<IComponent<TState, TEntity>> list, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {
            
            var ienum = list.GetEnumerator();
            while (ienum.MoveNext() == true) {

                var listItem = ienum.Current;
                if (listItem is TComponent listItemComponent) {

                    if (predicate.Execute(listItemComponent) == true) {
                        
                        PoolComponents.Recycle(listItemComponent);
                        list.Remove(listItem);
                        ienum.Dispose();
                        ienum = list.GetEnumerator();

                    }

                }
                    
            }
            ienum.Dispose();
            
        }

        public void RemoveAll(EntityId entityId) {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                PoolComponents.Recycle(list);
                list.Clear();

            }
            
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                PoolComponents.Recycle(list);
                list.Clear();

            }

        }

        public void Add(EntityId entityId, IComponent<TState, TEntity> data) {

            var dic = this.GetDictionary(data);
            
            HashSet<IComponent<TState, TEntity>> list;
            if (dic.TryGetValue(entityId, out list) == true) {

                list.Add(data);

            } else {

                list = PoolHashSet<IComponent<TState, TEntity>>.Spawn(this.capacity);
                list.Add(data);
                dic.Add(entityId, list);
                
            }

        }
        
        private ref Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> GetDictionary(IComponent<TState, TEntity> data) {

            if (data is IComponentOnceBase) {

                return ref this.dicOnce;

            } else {

                return ref this.dic;

            }

        }

        public TComponent GetFirst<TComponent>(EntityId entityId) where TComponent : class, IComponent<TState, TEntity> {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {
                    
                    if (listItem is TComponent item) return item;
                    
                }

            }

            return null;

        }

        public TComponent GetFirstOnce<TComponent>(EntityId entityId) where TComponent : class, IComponent<TState, TEntity> {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {
                    
                    if (listItem is TComponent item) return item;
                    
                }

            }

            return null;

        }

        public void ForEach<TComponent>(EntityId entityId, List<TComponent> output) where TComponent : class, IComponent<TState, TEntity> {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {
                    
                    if (listItem is TComponent item) output.Add(item);
                    
                }

            }

        }

        public bool ContainsOnce<TComponent>(EntityId entityId) where TComponent : IComponentOnce<TState, TEntity> {

            HashSet<IComponent<TState, TEntity>> list;
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {

                    if (listItem is TComponent) return true;

                }
                
            }
            
            return false;

        }

        public bool ContainsOnce(EntityId entityId, IComponentOnce<TState, TEntity> data) {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                return list.Contains(data);

            }

            return false;

        }

        public bool Contains<TComponent>(EntityId entityId) where TComponent : IComponent<TState, TEntity> {

            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {

                    if (listItem is TComponent) return true;

                }
                
            }
            
            return false;

        }

        public bool Contains(EntityId entityId, IComponent<TState, TEntity> data) {
            
            HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                return list.Contains(data);

            }

            return false;

        }

        public Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> GetData() {

            return this.dic;

        }

        public Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> GetDataOnce() {

            return this.dicOnce;

        }

        IDictionary IComponentsBase.GetData() {

            return this.dic;

        }

        IDictionary IComponentsBase.GetDataOnce() {

            return this.dicOnce;

        }

        public void Initialize(int capacity) {

            this.capacity = capacity;
            this.dic = PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Spawn(capacity);
            this.dicOnce = PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Components<TEntity, TState> other) {
            
            this.CopyFrom_INTERNAL(ref this.dic, other.dic);
            this.CopyFrom_INTERNAL(ref this.dicOnce, other.dicOnce);
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void CopyFrom_INTERNAL(ref Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dicDest, Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dicSource) {
            
            if (dicDest != null) {

                foreach (var item in dicDest) {

                    PoolComponents.Recycle(item.Value);
                    PoolHashSet<IComponent<TState, TEntity>>.Recycle(item.Value);

                }
                PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Recycle(ref dicDest);
                
            }
            
            dicDest = PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Spawn(this.capacity);
            foreach (var item in dicSource) {
                
                var newList = PoolHashSet<IComponent<TState, TEntity>>.Spawn(item.Value.Count);
                //UnityEngine.Debug.Log("CopyState for " + typeof(TEntity) + ", list: " + newList.Count + " << " + item.Value.Count);
                foreach (var element in item.Value) {
                    
                    var comp = (IComponent<TState, TEntity>)PoolComponents.Spawn(element.GetType());
                    if (comp == null) {
                        
                        comp = (IComponent<TState, TEntity>)System.Activator.CreateInstance(element.GetType());
                        PoolInternalBase.CallOnSpawn(comp);
                        
                    }
                    newList.Add(element);
                    
                }

                dicDest.Add(item.Key, newList);
                
            }

        }

    }

}