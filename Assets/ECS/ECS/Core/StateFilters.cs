using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using EntityId = System.Int32;
    using ME.ECS.Collections;

    internal interface IFilterInternal<TState> where TState : class, IState<TState>, new() {

        bool OnUpdate(Entity entity);
        bool CheckAdd(Entity entity);
        bool CheckRemove(Entity entity);
        bool OnAddComponent(Entity entity);
        bool OnRemoveComponent(Entity entity);

        HashSetCopyable<Entity> GetData();
        List<FilterRequest> GetRequests();
        bool forEachMode { get; set; }
        void Add_INTERNAL(Entity entity);
        bool Remove_INTERNAL(Entity entity);

    }

    public interface IFilterBase : IPoolableSpawn, IPoolableRecycle {

        int id { get; set; }
        int Count { get; }

        void Recycle();
        IFilterBase Clone();
        void CopyFrom(IFilterBase other);

    }

    public interface IFilterNode {

        bool Execute(Entity entity);

    }

    public interface IFilter<TState> : IFilterBase, IEnumerable<Entity> where TState : class, IState<TState>, new() {

        bool Contains(Entity entity);
        
        HashSetCopyable<Entity> GetData();
        new FilterEnumerator<TState> GetEnumerator();

    }

    public interface IFilter<TState, TEntity> : IFilter<TState> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        bool Contains(TEntity entity);

        IFilter<TState, TEntity> Custom(IFilterNode filter);
        IFilter<TState, TEntity> Custom<TFilter>() where TFilter : class, IFilterNode, new();
        IFilter<TState, TEntity> WithComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity>;
        IFilter<TState, TEntity> WithoutComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity>;

        IFilter<TState, TEntity> Push();
        new FilterEnumerator<TState> GetEnumerator();

    }

    public class FiltersStorage : IPoolableRecycle {

        private List<IFilterBase> filters;
        private bool freeze;

        public int Count {
            get {
                return this.filters.Count;
            }
        }

        void IPoolableRecycle.OnRecycle() {

            this.freeze = default;
            
            if (this.filters != null) {

                for (int i = 0, count = this.filters.Count; i < count; ++i) {
                    
                    this.filters[i].Recycle();

                }

                PoolList<IFilterBase>.Recycle(ref this.filters);
                
            }
            
        }

        public void Initialize(int capacity) {

            this.filters = PoolList<IFilterBase>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public List<IFilterBase> GetData() {

            return this.filters;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IFilterBase Get(int id) {

            return this.filters[id - 1];

        }

        public void Register(IFilterBase filter) {

            filter.id = this.filters.Count + 1;
            this.filters.Add(filter);
            
        }

        public void CopyFrom(FiltersStorage other) {
            
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

            if (this.filters == null) {
                
                this.filters = PoolList<IFilterBase>.Spawn(other.filters.Count);
                
            }
            
            for (int i = 0, count = other.filters.Count; i < count; ++i) {

                if (i >= this.filters.Count) {
                    
                    this.filters.Add(other.filters[i].Clone());
                    
                } else {

                    this.filters[i].CopyFrom(other.filters[i]);

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
            this.set.forEachMode = true;
                
        }
 
        public void Dispose() {

            this.set.forEachMode = false;

            var requests = this.set.GetRequests();
            for (int i = 0, count = requests.Count; i < count; ++i) {

                this.set.OnUpdate(requests[i].entity);

            }
            requests.Clear();

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

    internal struct FilterRequest {

        public Entity entity;

    }

    public class Filter<TState, TEntity> : IFilterInternal<TState>, IFilter<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        public Filter() {}

        internal Filter(string name) {

            this.name = name;

        }

        public void Recycle() {
            
            PoolFilters.Recycle(this);
            
        }

        public IFilterBase Clone() {

            var instance = PoolFilters.Spawn<Filter<TState, TEntity>>();
            instance.CopyFrom(this);
            return instance;

        }

        private class ComponentExistsFilterNode<TComponent> : IFilterNode where TComponent : class, IComponent<TState, TEntity> {

            public bool Execute(Entity entity) {
                
                return Worlds<TState>.currentWorld.HasComponent<TEntity, TComponent>(entity) == true;
                
            }

        }

        private class ComponentNotExistsFilterNode<TComponent> : IFilterNode where TComponent : class, IComponent<TState, TEntity> {

            public bool Execute(Entity entity) {
                
                return Worlds<TState>.currentWorld.HasComponent<TEntity, TComponent>(entity) == false;
                
            }

        }

        private const int REQUESTS_CAPACITY = 4;
        private const int NODES_CAPACITY = 4;

        public int id { get; set; }
        private string name;
        private IFilterNode[] nodes;
        private int nodesCount;
        private HashSetCopyable<EntityId> dataContains;
        private HashSetCopyable<Entity> data;
        bool IFilterInternal<TState>.forEachMode { get; set; }
        private List<FilterRequest> requests;
        
        private List<IFilterNode> tempNodes;
        private List<IFilterNode> tempNodesCustom;

        void IPoolableSpawn.OnSpawn() {

            this.requests = PoolList<FilterRequest>.Spawn(Filter<TState, TEntity>.REQUESTS_CAPACITY);
            this.nodes = PoolArray<IFilterNode>.Spawn(Filter<TState, TEntity>.NODES_CAPACITY);
            this.data = PoolHashSetCopyable<Entity>.Spawn();
            this.dataContains = PoolHashSetCopyable<EntityId>.Spawn();

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolList<FilterRequest>.Recycle(ref this.requests);
            PoolArray<IFilterNode>.Recycle(ref this.nodes);
            PoolHashSetCopyable<Entity>.Recycle(ref this.data);
            PoolHashSetCopyable<EntityId>.Recycle(ref this.dataContains);
            
        }

        public void CopyFrom(IFilterBase other) {
            
            this.CopyFrom(other as Filter<TState, TEntity>);
            
        }
        
        public void CopyFrom(Filter<TState, TEntity> other) {

            this.id = other.id;
            this.name = other.name;
            this.nodesCount = other.nodesCount;
            
            ArrayUtils.Copy(other.nodes, ref this.nodes);
            
            if (this.data != null) PoolHashSetCopyable<Entity>.Recycle(ref this.data);
            this.data = PoolHashSetCopyable<Entity>.Spawn(other.data.Count);
            this.data.CopyFrom(other.data);

            if (this.dataContains != null) PoolHashSetCopyable<EntityId>.Recycle(ref this.dataContains);
            this.dataContains = PoolHashSetCopyable<EntityId>.Spawn(other.dataContains.Count);
            this.dataContains.CopyFrom(other.dataContains);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public HashSetCopyable<Entity> GetData() {

            return this.data;

        }
        
        HashSetCopyable<Entity> IFilterInternal<TState>.GetData() {

            return this.data;

        }

        List<FilterRequest> IFilterInternal<TState>.GetRequests() {

            return this.requests;

        }

        public int Count {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.data.Count;
            }
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Contains(TEntity data) {

            var filter = Worlds<TState>.currentWorld.GetFilter<TEntity>(this.id);
            return filter.Contains_INTERNAL(data.entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Contains(Entity entity) {

            var filter = Worlds<TState>.currentWorld.GetFilter<TEntity>(this.id);
            return filter.Contains_INTERNAL(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool Contains_INTERNAL(Entity entity) {

            return this.dataContains.Contains(entity.id);

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

        bool IFilterInternal<TState>.OnUpdate(Entity entity) {

            var cast = (IFilterInternal<TState>)this;
            if (cast.forEachMode == true) {

                this.requests.Add(new FilterRequest() { entity = entity });
                return false;

            }

            var isExists = this.Contains_INTERNAL(entity);
            if (isExists == true) {

                return ((IFilterInternal<TState>)this).CheckRemove(entity);

            } else {

                return ((IFilterInternal<TState>)this).CheckAdd(entity);

            }

        }

        bool IFilterInternal<TState>.OnAddComponent(Entity entity) {

            return ((IFilterInternal<TState>)this).OnUpdate(entity);

        }

        bool IFilterInternal<TState>.OnRemoveComponent(Entity entity) {

            return ((IFilterInternal<TState>)this).OnUpdate(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void IFilterInternal<TState>.Add_INTERNAL(Entity entity) {
            
            this.dataContains.Add(entity.id);
            this.data.Add(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool IFilterInternal<TState>.Remove_INTERNAL(Entity entity) {
            
            var res = this.dataContains.Remove(entity.id);
            if (res == true) {
                    
                this.data.Remove(entity);
                    
            }

            return res;

        }

        bool IFilterInternal<TState>.CheckAdd(Entity entity) {

            for (int i = 0; i < this.nodesCount; ++i) {

                if (this.nodes[i].Execute(entity) == false) {

                    return false;

                }

            }

            var cast = (IFilterInternal<TState>)this;
            cast.Add_INTERNAL(entity);

            return true;

        }

        bool IFilterInternal<TState>.CheckRemove(Entity entity) {

            for (int i = 0; i < this.nodesCount; ++i) {

                if (this.nodes[i].Execute(entity) == false) {

                    return ((IFilterInternal<TState>)this).Remove_INTERNAL(entity);

                }

            }

            return false;

        }

        public IFilter<TState, TEntity> Push() {

            if (Worlds<TState>.currentWorld.HasFilter(this) == false) {

                this.tempNodes.AddRange(this.tempNodesCustom);
                this.nodes = Enumerable.ToArray(this.tempNodes);
                this.nodesCount = this.nodes.Length;
                this.tempNodes.Clear();
                this.tempNodesCustom.Clear();
                Worlds<TState>.currentWorld.Register(this);

            }

            return this;

        }

        public IFilter<TState, TEntity> Custom(IFilterNode filter) {

            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter<TState, TEntity> Custom<TFilter>() where TFilter : class, IFilterNode, new() {

            var filter = new TFilter();
            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter<TState, TEntity> WithComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity> {

            var node = new ComponentExistsFilterNode<TComponent>();
            this.tempNodes.Add(node);
            return this;

        }
        
        public IFilter<TState, TEntity> WithoutComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity> {

            var node = new ComponentNotExistsFilterNode<TComponent>();
            this.tempNodes.Add(node);
            return this;

        }

        public static IFilter<TState, TEntity> Create(ref IFilter<TState, TEntity> filter, string customName = null) {
            
            var f = PoolFilters.Spawn<Filter<TState, TEntity>>();
            f.name = customName != null ? customName : nameof(filter);
            f.tempNodes = new List<IFilterNode>();
            f.tempNodesCustom = new List<IFilterNode>();
            filter = f;
            return filter;

        }

        public static IFilter<TState, TEntity> Create(string customName = null) {

            IFilter<TState, TEntity> filter = null;
            return Filter<TState, TEntity>.Create(ref filter, customName);

        }

        public override string ToString() {

            return "Name: " + this.name + ", Objects Count: " + this.Count.ToString();

        }

    }

}