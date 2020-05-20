using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using ME.ECS.Collections;

    public interface IFilterInternal<TState> : IFilter<TState> where TState : class, IState<TState>, new() {

        bool OnUpdate(in Entity entity);
        //bool CheckAdd(in Entity entity);
        //bool CheckRemove(in Entity entity);
        bool OnAddComponent(in Entity entity);
        bool OnRemoveComponent(in Entity entity);
        bool OnRemoveEntity(in Entity entity);

        SortedSet<Entity> GetRequests();
        SortedSet<Entity> GetRequestsRemoveEntity();
        bool forEachMode { get; set; }
        void Add_INTERNAL(in Entity entity);
        bool Remove_INTERNAL(in Entity entity);

    }

    public interface IFilterBase : IPoolableSpawn, IPoolableRecycle {

        int id { get; set; }
        string name { get; }
        int Count { get; }

        void Recycle();
        IFilterBase Clone();
        void CopyFrom(IFilterBase other);
        void Update();

        Archetype GetArchetypeContains();
        Archetype GetArchetypeNotContains();
        int GetNodesCount();

        void SetForEachMode(bool state);
        
        HashSetCopyable<Entity> GetData();

        bool IsEquals(IFilterBase other);

    }

    public interface IFilterNode {

        bool Execute(Entity entity);

    }

    public interface IFilter<TState> : IFilterBase, IEnumerable<Entity> where TState : class, IState<TState>, new() {

        ref Entity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
        }
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool Contains(Entity entity);
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool IsForEntity(in Entity entity);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void ApplyAllRequests();
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        Entity[] GetArray();
        IFilter<TState> Custom(IFilterNode filter);
        IFilter<TState> Custom<TFilter>() where TFilter : class, IFilterNode, new();
        IFilter<TState> WithComponent<TComponent>() where TComponent : class, IComponent;
        IFilter<TState> WithoutComponent<TComponent>() where TComponent : class, IComponent;
        IFilter<TState> WithStructComponent<TComponent>() where TComponent : struct, IStructComponent;
        /*IFilter<TState> WithOneOfStructComponent<T1, T2>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent;
        IFilter<TState> WithOneOfStructComponent<T1, T2, T3>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent
            where T3 : struct, IStructComponent;
        IFilter<TState> WithOneOfStructComponent<T1, T2, T3, T4>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent
            where T3 : struct, IStructComponent
            where T4 : struct, IStructComponent;*/
        IFilter<TState> WithoutStructComponent<TComponent>() where TComponent : struct, IStructComponent;

        IFilter<TState> Push();
        IFilter<TState> Push(ref IFilter<TState> filter);
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        new FilterEnumerator<TState> GetEnumerator();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class FiltersStorage : IPoolableRecycle {

        internal IFilterBase[] filters;
        private bool freeze;
        private int nextId;

        public int Count {
            get {
                return this.filters.Length;
            }
        }

        void IPoolableRecycle.OnRecycle() {

            this.nextId = default;
            this.freeze = default;
            
            if (this.filters != null) {

                for (int i = 0, count = this.filters.Length; i < count; ++i) {
                    
                    if (this.filters[i] == null) continue;
                    this.filters[i].Recycle();

                }

                PoolArray<IFilterBase>.Recycle(ref this.filters);
                
            }
            
        }

        public void Initialize(int capacity) {

            this.filters = PoolArray<IFilterBase>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public IFilterBase[] GetData() {

            return this.filters;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref IFilterBase Get(int id) {

            return ref this.filters[id - 1];

        }

        public IFilterBase GetByHashCode(int hashCode) {

            for (int i = 0; i < this.filters.Length; ++i) {

                var filter = this.filters[i];
                if (filter != null) {

                    if (filter.GetHashCode() == hashCode) {

                        return filter;

                    }

                }

            }
            
            return null;

        }

        public IFilterBase GetFilterEquals(IFilterBase other) {
            
            for (int i = 0; i < this.filters.Length; ++i) {

                var filter = this.filters[i];
                if (filter != null) {

                    if (filter.GetHashCode() == other.GetHashCode() && filter.IsEquals(other) == true) {

                        return filter;

                    }

                }

            }

            return null;

        }

        public void Register(IFilterBase filter) {

            ArrayUtils.Resize(filter.id - 1, ref this.filters);
            this.filters[filter.id - 1] = filter;
            
        }

        public int GetNextId() {

            return this.nextId + 1;

        }

        public int AllocateNextId() {

            return ++this.nextId;

        }

        public void CopyFrom(FiltersStorage other) {

            this.nextId = other.nextId;
            
            /*if (this.filters != null) {

                for (int i = 0, count = this.filters.Count; i < count; ++i) {
                    
                    this.filters[i].Recycle();

                }

                PoolList<IFilterBase>.Recycle(ref this.filters);
                
            }
            this.filters = PoolList<IFilterBase>.Spawn(other.filters.Count);

            for (int i = 0, count = other.filters.Count; i < count; ++i) {
                
                var copy = other.filters[i].Clone();
                this.filters.Add(copy);
                UnityEngine.Debug.Log("Copy filter: " + i + " :: " + other.filters[i].id + " >> " + this.filters.Count + " (" + copy.id + ")");
                
            }*/

            if (this.freeze == true) {

                // Just copy if filters storage is in freeze mode
                if (this.filters == null) {

                    this.filters = PoolArray<IFilterBase>.Spawn(other.filters.Length);

                }

                for (int i = 0, count = other.filters.Length; i < count; ++i) {

                    if (other.filters[i] == null && this.filters[i] == null) {
                        
                        continue;
                        
                    }

                    if (other.filters[i] == null && this.filters[i] != null) {

                        this.filters[i].Recycle();
                        this.filters[i] = null;
                        continue;
                        
                    }

                    if (i >= this.filters.Length || this.filters[i] == null) {

                        this.Register(other.filters[i].Clone());

                    } else {

                        this.filters[i].CopyFrom(other.filters[i]);

                    }

                }

            } else {
                
                // Filters storage is not in a freeze mode, so it is an active state filters
                for (int i = 0, count = other.filters.Length; i < count; ++i) {

                    if (other.filters[i] == null && this.filters[i] == null) {
                        
                        continue;
                        
                    }

                    if (other.filters[i] == null && this.filters[i] != null) {

                        this.filters[i].Recycle();
                        this.filters[i] = null;
                        continue;
                        
                    }

                    if (i >= this.filters.Length || this.filters[i] == null && other.filters[i] != null) {

                        this.Register(other.filters[i].Clone());

                    } else {

                        this.filters[i].CopyFrom(other.filters[i]);

                    }

                }
                
            }

        }

    }

    public struct FilterEnumerator<TState> : IEnumerator<Entity> where TState : class, IState<TState>, new() {
            
        private readonly IFilterInternal<TState> set;
        private HashSetCopyable<Entity>.Enumerator setEnumerator;
            
        internal FilterEnumerator(IFilterInternal<TState> set) {
                
            this.set = set;
            this.setEnumerator = this.set.GetData().GetEnumerator();
            this.set.SetForEachMode(true);

        }
 
        public void Dispose() {

            this.set.SetForEachMode(false);

        }
 
        public bool MoveNext() {
            
            return this.setEnumerator.MoveNext();
                
        }
 
        public Entity Current {
            get {
                return this.setEnumerator.Current;
            }
        }
 
        System.Object System.Collections.IEnumerator.Current {
            get {
                throw new AllocationException();
            }
        }
 
        void System.Collections.IEnumerator.Reset() {
                
        }
    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Filter<TState> : IFilterInternal<TState>, IFilter<TState> where TState : class, IState<TState>, new() {

        public Filter() {}

        internal Filter(string name) {

            this.name = name;

        }

        public void Recycle() {
            
            PoolFilters.Recycle(this);
            
        }

        public void Update() {

            if (Worlds<TState>.currentWorld.ForEachEntity(out RefList<Entity> list) == true) {

                for (int i = list.FromIndex; i < list.SizeCount; ++i) {

                    var entity = list[i];
                    if (list.IsFree(i) == true) continue;
                    
                    ((IFilterInternal<TState>)this).OnUpdate(entity);

                }

            }

        }

        public IFilterBase Clone() {

            var instance = PoolFilters.Spawn<Filter<TState>>();
            instance.CopyFrom(this);
            return instance;

        }

        private const int REQUESTS_CAPACITY = 4;
        private const int NODES_CAPACITY = 4;
        private const int ENTITIES_CAPACITY = 100;

        public int id { get; set; }
        public string name { get; private set; }
        public World<TState> world { get; private set; }
        private IFilterNode[] nodes;
        private Archetype archetypeContains;
        private Archetype archetypeNotContains;
        private int nodesCount;
        private bool[] dataContains;
        private HashSetCopyable<Entity> data;
        bool IFilterInternal<TState>.forEachMode { get; set; }
        private SortedSet<Entity> requests;
        private SortedSet<Entity> requestsRemoveEntity;
        
        private List<IFilterNode> tempNodes;
        private List<IFilterNode> tempNodesCustom;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Entity[] GetArray() {

            var arr = PoolArray<Entity>.Spawn(this.data.Count);
            this.data.CopyTo(arr, 0, arr.Length);
            return arr;

        }

        void IPoolableSpawn.OnSpawn() {

            this.requests = PoolSortedSet<Entity>.Spawn(Filter<TState>.REQUESTS_CAPACITY);
            this.requestsRemoveEntity = PoolSortedSet<Entity>.Spawn(Filter<TState>.REQUESTS_CAPACITY);
            this.nodes = PoolArray<IFilterNode>.Spawn(Filter<TState>.NODES_CAPACITY);
            this.data = PoolHashSetCopyable<Entity>.Spawn();
            this.dataContains = PoolArray<bool>.Spawn(Filter<TState>.ENTITIES_CAPACITY);

            this.id = default;
            this.name = default;
            this.nodesCount = default;
            this.archetypeContains = default;
            this.archetypeNotContains = default;

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<bool>.Recycle(ref this.dataContains);
            PoolHashSetCopyable<Entity>.Recycle(ref this.data);
            PoolArray<IFilterNode>.Recycle(ref this.nodes);
            PoolSortedSet<Entity>.Recycle(ref this.requestsRemoveEntity);
            PoolSortedSet<Entity>.Recycle(ref this.requests);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsForEntity(in Entity entity) {

            ref var previousArchetype = ref this.world.storagesCache.archetypes.GetPrevious(in entity);
            ref var currentArchetype = ref this.world.storagesCache.archetypes.Get(in entity);

            if (previousArchetype.ContainsAll(this.archetypeContains) == true) return true;
            if (previousArchetype.NotContains(this.archetypeNotContains) == true) return true;

            if (currentArchetype.ContainsAll(this.archetypeContains) == true) return true;
            if (currentArchetype.NotContains(this.archetypeNotContains) == true) return true;

            return false;

        }

        public int GetNodesCount() {

            return this.nodesCount;

        }

        public Archetype GetArchetypeContains() {

            return this.archetypeContains;

        }

        public Archetype GetArchetypeNotContains() {

            return this.archetypeNotContains;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetForEachMode(bool state) {

            lock (this) {

                var internalFilter = ((IFilterInternal<TState>)this);
                internalFilter.forEachMode = state;
                if (state == false) {

                    if (Worlds<TState>.currentWorld.currentSystemContext == null) {

                        this.ApplyAllRequests();

                    }
                    
                }

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void ApplyAllRequests() {
            
            var internalFilter = ((IFilterInternal<TState>)this);
            {
                
                var requests = internalFilter.GetRequests();
                foreach (var entity in requests) {

                    internalFilter.OnUpdate(entity);

                }

                requests.Clear();

            }

            {
                
                var requests = internalFilter.GetRequestsRemoveEntity();
                foreach (var entity in requests) {

                    internalFilter.Remove_INTERNAL(entity);

                }

                requests.Clear();

            }

        }

        public void CopyFrom(IFilterBase other) {
            
            this.CopyFrom(other as Filter<TState>);
            
        }
        
        public void CopyFrom(Filter<TState> other) {

            lock (this) {

                this.id = other.id;
                this.name = other.name;
                this.nodesCount = other.nodesCount;

                this.archetypeContains = other.archetypeContains;
                this.archetypeNotContains = other.archetypeNotContains;
                
                ArrayUtils.Copy(other.nodes, ref this.nodes);

                if (this.data != null) PoolHashSetCopyable<Entity>.Recycle(ref this.data);
                this.data = PoolHashSetCopyable<Entity>.Spawn(other.data.Count);
                this.data.CopyFrom(other.data);

                ArrayUtils.Copy(other.dataContains, ref this.dataContains);
                
            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public HashSetCopyable<Entity> GetData() {

            return this.data;

        }
        
        SortedSet<Entity> IFilterInternal<TState>.GetRequests() {

            return this.requests;

        }

        SortedSet<Entity> IFilterInternal<TState>.GetRequestsRemoveEntity() {

            return this.requestsRemoveEntity;

        }

        public int Count {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.data.Count;
            }
        }

        public ref Entity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.data.Get(index);
            }
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Contains(Entity entity) {

            var filter = Worlds<TState>.currentWorld.GetFilter(this.id);
            return filter.Contains_INTERNAL(entity.id);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool Contains_INTERNAL(int entityId) {

            if (entityId < 0 || entityId >= this.dataContains.Length) return false;
            return this.dataContains[entityId];

        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() {

            return ((IEnumerable<Entity>)this.data).GetEnumerator();

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {

            return ((System.Collections.IEnumerable)this.data).GetEnumerator();

        }

        public FilterEnumerator<TState> GetEnumerator() {

            return new FilterEnumerator<TState>(this);

        }

        bool IFilterInternal<TState>.OnUpdate(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        bool IFilterInternal<TState>.OnAddComponent(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        bool IFilterInternal<TState>.OnRemoveComponent(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool OnUpdate_INTERNAL(in Entity entity) {

            lock (this) {

                if (Worlds<TState>.currentWorld.currentSystemContext != null) {

                    lock (Worlds<TState>.currentWorld.currentSystemContextFiltersUsed) {

                        if (Worlds<TState>.currentWorld.currentSystemContextFiltersUsed.ContainsKey(this.id) == false) {

                            Worlds<TState>.currentWorld.currentSystemContextFiltersUsed.Add(this.id, this);

                        }

                    }

                    if (this.requests.Contains(entity) == false) this.requests.Add(entity);
                    return false;

                }

                var cast = (IFilterInternal<TState>)this;
                if (cast.forEachMode == true) {

                    if (this.requests.Contains(entity) == false) this.requests.Add(entity);
                    return false;

                }

                var isExists = this.Contains_INTERNAL(entity.id);
                if (isExists == true) {

                    return this.CheckRemove(in entity);

                } else {

                    return this.CheckAdd(in entity);

                }

            }

        }

        bool IFilterInternal<TState>.OnRemoveEntity(in Entity entity) {

            lock (this) {

                if (Worlds<TState>.currentWorld.currentSystemContext != null) {

                    lock (Worlds<TState>.currentWorld.currentSystemContextFiltersUsed) {

                        if (Worlds<TState>.currentWorld.currentSystemContextFiltersUsed.ContainsKey(this.id) == false) {

                            Worlds<TState>.currentWorld.currentSystemContextFiltersUsed.Add(this.id, this);

                        }

                    }

                    if (this.requestsRemoveEntity.Contains(entity) == false) this.requestsRemoveEntity.Add(entity);
                    return false;

                }
                
                var cast = (IFilterInternal<TState>)this;
                if (cast.forEachMode == true) {

                    if (this.requestsRemoveEntity.Contains(entity) == false) this.requestsRemoveEntity.Add(entity);
                    return false;

                }

                return ((IFilterInternal<TState>)this).Remove_INTERNAL(entity);

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void IFilterInternal<TState>.Add_INTERNAL(in Entity entity) {

            ArrayUtils.Resize(entity.id, ref this.dataContains);
            this.dataContains[entity.id] = true;
            this.data.Add(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool IFilterInternal<TState>.Remove_INTERNAL(in Entity entity) {

            var idx = entity.id;
            if (idx < 0 || idx >= this.dataContains.Length) return false;
            
            ref var res = ref this.dataContains[idx];
            if (res == true) {

                res = false;
                this.data.Remove(entity);
                return true;

            }

            return false;

        }

        /*[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool ContainsAllNodes(Entity entity) {
            
            for (int i = 0; i < this.nodesCount; ++i) {

                if (this.nodes[i].Execute(entity) == false) {

                    return false;

                }

            }

            return true;

        }*/

        private bool CheckAdd(in Entity entity) {

            // If entity doesn't exist in cache - try to add if entity's archetype fit with contains & notContains
            ref var entArchetype = ref this.world.storagesCache.archetypes.Get(in entity);
            if (entArchetype.ContainsAll(this.archetypeContains) == false) return false;
            if (entArchetype.NotContains(this.archetypeNotContains) == false) return false;

            if (this.nodesCount > 0) {

                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(entity) == false) {

                        return false;

                    }

                }

            }

            var cast = (IFilterInternal<TState>)this;
            cast.Add_INTERNAL(entity);

            return true;

        }

        private bool CheckRemove(in Entity entity) {

            // If entity already exists in cache - try to remove if entity's archetype doesn't fit with contains & notContains
            ref var entArchetype = ref this.world.storagesCache.archetypes.Get(in entity);
            var allContains = entArchetype.ContainsAll(this.archetypeContains);
            var allNotContains = entArchetype.NotContains(this.archetypeNotContains);
            
            if (this.nodesCount > 0) {

                var isFail = false;
                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(entity) == false) {

                        isFail = true;
                        break;

                    }

                }

                if (isFail == false) return false;

            }
            
            if (allContains == true && allNotContains == true) return false;

            return ((IFilterInternal<TState>)this).Remove_INTERNAL(entity);

        }

        public bool IsEquals(IFilterBase filter) {

            if (this.GetArchetypeContains() == filter.GetArchetypeContains() &&
                this.GetArchetypeNotContains() == filter.GetArchetypeNotContains() &&
                this.GetType() == filter.GetType() &&
                this.GetNodesCount() == filter.GetNodesCount()) {

                return true;

            }

            return false;

        }

        public override int GetHashCode() {

            var hashCode = this.GetType().GetHashCode() ^ this.archetypeContains.GetHashCode() ^ this.archetypeNotContains.GetHashCode();
            for (int i = 0; i < this.nodesCount; ++i) {

                hashCode ^= this.nodes[i].GetType().GetHashCode();

            }
            
            return hashCode;

        }

        public IFilter<TState> Push() {

            IFilter<TState> filter = null;
            return this.Push(ref filter);

        }

        public IFilter<TState> Push(ref IFilter<TState> filter) {

            var world = Worlds<TState>.currentWorld;
            var nextId = world.filtersStorage.GetNextId();
            if (world.HasFilter(nextId) == false) {

                this.tempNodes.AddRange(this.tempNodesCustom);
                this.nodes = this.tempNodes.OrderBy(x => x.GetType().GetHashCode()).ToArray();
                this.nodesCount = this.nodes.Length;
                this.tempNodes.Clear();
                this.tempNodesCustom.Clear();
                
                var existsFilter = (IFilter<TState>)world.GetFilterEquals(this);
                if (existsFilter != null) {

                    filter = existsFilter;
                    this.Recycle();
                    return existsFilter;

                } else {

                    this.id = world.filtersStorage.AllocateNextId();

                    filter = this;
                    this.world = world;
                    world.Register(this);

                }

            } else {

                UnityEngine.Debug.LogWarning(string.Format("World #{0} already has filter {1}!", world.id, this));

            }

            return this;

        }

        public IFilter<TState> Custom(IFilterNode filter) {

            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter<TState> Custom<TFilter>() where TFilter : class, IFilterNode, new() {

            var filter = new TFilter();
            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter<TState> WithComponent<TComponent>() where TComponent : class, IComponent {

            //var node = new ComponentExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeContains.Add<TComponent>();
            return this;

        }
        
        public IFilter<TState> WithoutComponent<TComponent>() where TComponent : class, IComponent {

            //var node = new ComponentNotExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeNotContains.Add<TComponent>();
            return this;

        }

        public IFilter<TState> WithStructComponent<TComponent>() where TComponent : struct, IStructComponent {

            //var node = new ComponentStructExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeContains.Add<TComponent>();
            return this;

        }

        /*public IFilter<TState> WithOneOfStructComponent<T1, T2, T3, T4>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent
            where T3 : struct, IStructComponent
            where T4 : struct, IStructComponent {

            var node = new OneOfStructExistsFilterNode<T1, T2, T3, T4>();
            this.tempNodes.Add(node);
            return this;

        }

        public IFilter<TState> WithOneOfStructComponent<T1, T2, T3>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent
            where T3 : struct, IStructComponent {

            var node = new OneOfStructExistsFilterNode<T1, T2, T3>();
            this.tempNodes.Add(node);
            return this;

        }

        public IFilter<TState> WithOneOfStructComponent<T1, T2>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent {

            var node = new OneOfStructExistsFilterNode<T1, T2>();
            this.tempNodes.Add(node);
            return this;

        }*/

        public IFilter<TState> WithoutStructComponent<TComponent>() where TComponent : struct, IStructComponent {

            //var node = new ComponentStructNotExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeNotContains.Add<TComponent>();
            return this;

        }

        public static IFilter<TState> Create(string customName = null) {

            var f = PoolFilters.Spawn<Filter<TState>>();
            f.name = customName;
            f.tempNodes = new List<IFilterNode>();
            f.tempNodesCustom = new List<IFilterNode>();
            return f;

        }

        public override string ToString() {

            return "Name: " + this.name + " (" + this.id.ToString() + ")";

        }

    }

}