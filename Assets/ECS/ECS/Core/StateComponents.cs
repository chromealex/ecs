using System.Collections;
using System.Collections.Generic;
using EntityId = System.Int32;

namespace ME.ECS {

    public interface IComponentsBase {

        IList<IComponentBase> GetData(EntityId entityId);
        IDictionary GetDataOnce();

    }

    public interface IComponents<TState> : IComponentsBase, IPoolableRecycle where TState : class, IState<TState>, new() {

        int Count { get; }

        int RemoveAll(EntityId entityId);
        int RemoveAll<TComponent>(EntityId entityId) where TComponent : class, IComponentBase;
        int RemoveAllOnce<TComponent>(EntityId entityId) where TComponent : class, IComponentOnceBase;
        int RemoveAll<TComponent>() where TComponent : class, IComponentBase;
        int RemoveAllOnce<TComponent>() where TComponent : class, IComponentOnceBase;

        int RemoveAllPredicate<TComponent, TComponentPredicate>(EntityId entityId, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent>;

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
    public class Components<TEntity, TState> : IComponents<TState> where TEntity : struct, IEntity where TState : class, IState<TState>, new() {

        private static class ComponentType<TComponent, TEntityInner, TStateInner> {

            public static int id = -1;

        }

        /*private struct InternalComponentPredicateTrue<TComponent> : IComponentPredicate<TComponent> where TComponent : class, IComponentBase {

            public bool Execute(TComponent data) {
                return true;
            }

        }*/

        public struct Bucket {

            public List<IComponent<TState, TEntity>>[] components;

        }

        private Bucket[] arr; // arr by component type
        //private Dictionary<System.Type, int> typeIds;
        private int typeId;
        
        //private Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dic;
        //private Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> dicOnce;
        private bool freeze;
        private int capacity;

        void IPoolableRecycle.OnRecycle() {

            for (int i = 0; i < this.arr.Length; ++i) {

                ref var bucket = ref this.arr[i];
                if (bucket.components != null) {

                    for (int j = 0; j < bucket.components.Length; ++j) {

                        var list = bucket.components[j];
                        if (list != null) {

                            PoolComponents.Recycle(list);
                            PoolList<IComponent<TState, TEntity>>.Recycle(ref list);
                            bucket.components[j] = null;

                        }

                    }

                    PoolArray<List<IComponent<TState, TEntity>>>.Recycle(ref bucket.components);

                }

            }
            PoolArray<Bucket>.Recycle(ref this.arr);
            
            /*foreach (var item in this.dic) {
                
                PoolComponents.Recycle(item.Value);
                PoolHashSet<IComponent<TState, TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Recycle(ref this.dic);

            foreach (var item in this.dicOnce) {
                
                PoolComponents.Recycle(item.Value);
                PoolHashSet<IComponent<TState, TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Recycle(ref this.dicOnce);*/

            this.freeze = default;
            this.capacity = default;

        }

        public int Count {

            get {

                var count = 0;
                /*foreach (var item in this.dic) {

                    count += item.Value.Count;

                }*/
                for (int i = 0; i < this.arr.Length; ++i) {

                    if (this.arr[i].components != null) count += this.arr[i].components.Length;

                }

                return count;

            }

        }

        public int RemoveAllPredicate<TComponent, TComponentPredicate>(EntityId entityId, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {

            var count = 0;
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components != null && entityId >= 0 && entityId < bucket.components.Length) {

                    ref var list = ref bucket.components[entityId];
                    if (list == null) return 0;
                    
                    for (int i = list.Count - 1; i >= 0; --i) {

                        var tComp = list[i] as TComponent;
                        if (predicate.Execute(tComp) == true) {
                            
                            PoolComponents.Recycle(ref tComp);
                            list.RemoveAt(i);
                            ++count;

                        }

                    }
                    
                }

            }

            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                return this.RemoveAll_INTERNAL<TComponent, TComponentPredicate>(list, predicate);
                
            }*/

            return count;

        }

        public int RemoveAll<TComponent>(EntityId entityId) where TComponent : class, IComponentBase {
            
            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                return this.RemoveAll_INTERNAL<TComponent>(list);
                
            }*/
            
            return this.RemoveAll_INTERNAL<TComponent>(entityId);

        }

        public int RemoveAllOnce<TComponent>(EntityId entityId) where TComponent : class, IComponentOnceBase {
            
            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                return this.RemoveAll_INTERNAL<TComponent>(list);
                
            }*/
            
            return this.RemoveAll_INTERNAL<TComponent>(entityId);

        }

        public int RemoveAll<TComponent>() where TComponent : class, IComponentBase {

            return this.RemoveAll_INTERNAL<TComponent>();

        }
        
        public int RemoveAllOnce<TComponent>() where TComponent : class, IComponentOnceBase {

            return this.RemoveAll_INTERNAL<TComponent>();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int RemoveAll_INTERNAL<TComponent>() where TComponent : class, IComponentBase {

            var count = 0;
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null) return 0;
                
                count += bucket.components.Length;
                System.Array.Clear(bucket.components, 0, bucket.components.Length);

            }

            return count;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int RemoveAll_INTERNAL<TComponent>(EntityId entityId) where TComponent : class, IComponentBase {

            var count = 0;
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components != null && entityId >= 0 && entityId < bucket.components.Length) {

                    var list = bucket.components[entityId];
                    if (list == null) return 0;
                    
                    for (int i = list.Count - 1; i >= 0; --i) {

                        var tComp = list[i] as TComponent;
                        PoolComponents.Recycle(ref tComp);
                        list.RemoveAt(i);
                        ++count;

                    }
                    
                }

            }
            
            return count;

            /*var count = 0;
            if (list.Count > 0) {

                var ienum = list.GetEnumerator();
                while (ienum.MoveNext() == true) {

                    var listItem = ienum.Current;
                    if (listItem is TComponent listItemComponent) {

                        PoolComponents.Recycle(listItemComponent);
                        list.Remove(listItem);
                        ienum.Dispose();
                        ienum = list.GetEnumerator();
                        ++count;

                    }

                }
                ienum.Dispose();

            }

            return count;*/

        }

        /*[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int RemoveAll_INTERNAL<TComponent, TComponentPredicate>(HashSet<IComponent<TState, TEntity>> list, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {

            var count = 0;
            var ienum = list.GetEnumerator();
            while (ienum.MoveNext() == true) {

                var listItem = ienum.Current;
                if (listItem is TComponent listItemComponent) {

                    if (predicate.Execute(listItemComponent) == true) {
                        
                        PoolComponents.Recycle(listItemComponent);
                        list.Remove(listItem);
                        ienum.Dispose();
                        ienum = list.GetEnumerator();
                        ++count;

                    }

                }
                    
            }
            ienum.Dispose();

            return count;

        }*/

        public int RemoveAll(EntityId entityId) {

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

            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                count += list.Count;
                PoolComponents.Recycle(list);
                list.Clear();

            }
            
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                count += list.Count;
                PoolComponents.Recycle(list);
                list.Clear();

            }*/

            return count;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int GetTypeId<TComponent>() {

            if (ComponentType<TComponent, TEntity, TState>.id < 0) {

                ComponentType<TComponent, TEntity, TState>.id = this.typeId++;

            }

            return ComponentType<TComponent, TEntity, TState>.id;

            /*
            var typeId = ComponentType<TComponent>.id;
            if (this.typeIds.TryGetValue(type, out var id) == true) {

                return id;

            } else {

                this.typeIds.Add(type, this.typeId);
                return this.typeId++;

            }*/

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Add_INTERNAL(int typeId, EntityId entityId, IComponent<TState, TEntity> data) {

            while (typeId >= this.arr.Length) {
                
                System.Array.Resize(ref this.arr, this.arr.Length * 2);
                
            }

            ref var bucket = ref this.arr[typeId];
            if (bucket.components == null) bucket.components = PoolArray<List<IComponent<TState, TEntity>>>.Spawn(entityId + 1);
            while (entityId >= bucket.components.Length) {
                
                System.Array.Resize(ref bucket.components, bucket.components.Length * 2);
                
            }

            if (bucket.components[entityId] == null) bucket.components[entityId] = PoolList<IComponent<TState, TEntity>>.Spawn(1);
            bucket.components[entityId].Add(data);

        }

        public void Add<TComponent>(EntityId entityId, IComponent<TState, TEntity> data) where TComponent : class, IComponent<TState, TEntity> {

            var typeId = this.GetTypeId<TComponent>();
            this.Add_INTERNAL(typeId, entityId, data);
            
            /*var dic = this.GetDictionary(data);
            
            HashSet<IComponent<TState, TEntity>> list;
            if (dic.TryGetValue(entityId, out list) == true) {

                list.Add(data);

            } else {

                list = PoolHashSet<IComponent<TState, TEntity>>.Spawn(this.capacity);
                list.Add(data);
                dic.Add(entityId, list);
                
            }*/

        }
        
        /*private ref Dictionary<EntityId, HashSet<IComponent<TState, TEntity>>> GetDictionary(IComponent<TState, TEntity> data) {

            if (data is IComponentOnceBase) {

                return ref this.dicOnce;

            } else {

                return ref this.dic;

            }

        }*/

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private TComponent Get_INTERNAL<TComponent>(EntityId entityId) where TComponent : class, IComponent<TState, TEntity> {
            
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return null;
                
                var list = bucket.components[entityId];
                if (list.Count > 0) return list[0] as TComponent;

            }

            return null;

        }

        public TComponent GetFirst<TComponent>(EntityId entityId) where TComponent : class, IComponent<TState, TEntity> {
            
            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {
                    
                    if (listItem is TComponent item) return item;
                    
                }

            }*/

            return this.Get_INTERNAL<TComponent>(entityId);

        }

        public TComponent GetFirstOnce<TComponent>(EntityId entityId) where TComponent : class, IComponentOnce<TState, TEntity> {
            
            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {
                    
                    if (listItem is TComponent item) return item;
                    
                }

            }*/

            return this.Get_INTERNAL<TComponent>(entityId);

        }

        public List<IComponent<TState, TEntity>> ForEach<TComponent>(EntityId entityId) where TComponent : class, IComponent<TState, TEntity> {
            
            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {
                    
                    if (listItem is TComponent item) output.Add(item);
                    
                }

            }*/
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return null;
                
                return bucket.components[entityId];
                
            }

            return null;

        }

        public bool ContainsOnce<TComponent>(EntityId entityId) where TComponent : IComponentOnce<TState, TEntity> {

            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dicOnce.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {

                    if (listItem is TComponent) return true;

                }
                
            }*/
            
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return false;
                
                return bucket.components[entityId].Count > 0;

            }
            
            return false;

        }

        public bool Contains<TComponent>(EntityId entityId) where TComponent : IComponent<TState, TEntity> {

            /*HashSet<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entityId, out list) == true) {

                foreach (var listItem in list) {

                    if (listItem is TComponent) return true;

                }
                
            }*/
            
            var typeId = this.GetTypeId<TComponent>();
            if (typeId >= 0 && typeId < this.arr.Length) {

                ref var bucket = ref this.arr[typeId];
                if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length || bucket.components[entityId] == null) return false;

                return bucket.components[entityId].Count > 0;

            }
            
            return false;

        }

        IList<IComponentBase> IComponentsBase.GetData(EntityId entityId) {

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

        public void Initialize(int capacity) {

            this.capacity = capacity;
            this.arr = PoolArray<Bucket>.Spawn(capacity);
            //this.typeIds = PoolDictionary<System.Type, int>.Spawn(capacity);
            
            //this.dic = PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Spawn(capacity);
            //this.dicOnce = PoolDictionary<EntityId, HashSet<IComponent<TState, TEntity>>>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Components<TEntity, TState> other) {

            this.typeId = other.typeId;
            
            // Clean up current array
            for (int i = 0; i < this.arr.Length; ++i) {

                ref var bucket = ref this.arr[i];
                if (bucket.components != null) {

                    for (int j = 0; j < bucket.components.Length; ++j) {

                        var list = bucket.components[j];
                        if (list != null) {

                            PoolComponents.Recycle(list);
                            PoolList<IComponent<TState, TEntity>>.Recycle(ref list);
                            bucket.components[j] = null;

                        }

                    }

                    PoolArray<List<IComponent<TState, TEntity>>>.Recycle(ref bucket.components);

                }

            }
            PoolArray<Bucket>.Recycle(ref this.arr);
            
            // Clone other array
            this.arr = PoolArray<Bucket>.Spawn(other.arr.Length);
            for (int i = 0; i < other.arr.Length; ++i) {

                ref var otherBucket = ref other.arr[i];
                if (otherBucket.components != null) {

                    this.arr[i].components = PoolArray<List<IComponent<TState, TEntity>>>.Spawn(otherBucket.components.Length);
                    for (int j = 0; j < otherBucket.components.Length; ++j) {

                        ref var otherList = ref otherBucket.components[j];
                        if (otherList != null) {

                            var list = this.arr[i].components[j] = PoolList<IComponent<TState, TEntity>>.Spawn(otherList.Capacity);
                            for (int k = 0; k < otherList.Count; ++k) {

                                var element = otherList[k];
                                var type = element.GetType();
                                var comp = (IComponent<TState, TEntity>)PoolComponents.Spawn(type);
                                if (comp == null) {

                                    comp = (IComponent<TState, TEntity>)System.Activator.CreateInstance(type);
                                    PoolInternalBase.CallOnSpawn(comp);

                                }

                                if (comp is IComponentCopyable<TState, TEntity> compCopyable) compCopyable.CopyFrom(element);

                                list.Add(comp);

                            }

                        } else {

                            if (this.arr[i].components[j] != null) PoolComponents.Recycle(this.arr[i].components[j]);
                            this.arr[i].components[j] = null;

                        }

                    }

                } else {

                    this.arr[i].components = null;

                }

            }

            // Old copy dic by dic
            //this.CopyFrom_INTERNAL(ref this.dic, other.dic);
            //this.CopyFrom_INTERNAL(ref this.dicOnce, other.dicOnce);
            
        }
        
        /*[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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

                    var type = element.GetType();
                    var comp = (IComponent<TState, TEntity>)PoolComponents.Spawn(type);
                    if (comp == null) {
                        
                        comp = (IComponent<TState, TEntity>)System.Activator.CreateInstance(type);
                        PoolInternalBase.CallOnSpawn(comp);
                        
                    }
                    if (comp is IComponentCopyable<TState, TEntity> compCopyable) compCopyable.CopyFrom(element);
                    newList.Add(comp);

                }

                dicDest.Add(item.Key, newList);
                
            }

        }*/

    }

}