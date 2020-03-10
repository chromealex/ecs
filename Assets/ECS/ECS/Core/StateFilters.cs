using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using EntityId = System.Int32;
    using ME.ECS.Collections;

    internal interface IFilterInternal<TState> where TState : class, IState<TState> {

        bool OnUpdate(Entity entity);
        bool OnAdd(Entity entity);
        bool OnRemove(Entity entity);
        bool OnAddComponent(Entity entity);
        bool OnRemoveComponent(Entity entity);

    }

    public interface IFilterBase : IPoolableSpawn, IPoolableRecycle {

        int id { get; set; }
        int Count { get; }

        IFilterBase Clone();

    }

    public interface IFilterNode {

        bool Execute(Entity entity);

    }

    public interface IFilter<TState, TEntity> : IFilterBase, IEnumerable<Entity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        bool Contains(TEntity entity);
        bool Contains(Entity entity);

        IFilter<TState, TEntity> Custom(IFilterNode filter);
        IFilter<TState, TEntity> Custom<TFilter>() where TFilter : class, IFilterNode, new();
        IFilter<TState, TEntity> WithComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity>;
        IFilter<TState, TEntity> WithoutComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity>;

        HashSetCopyable<Entity> GetData();
        
        IFilter<TState, TEntity> Push();

    }

    public class FiltersStorage {

        private List<IFilterBase> filters;
        private bool freeze;

        public int Count {
            get {
                return this.filters.Count;
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

            if (this.filters != null) {

                for (int i = 0, count = this.filters.Count; i < count; ++i) {
                    
                    PoolFilters.Recycle(this.filters[i]);
                    
                }

                PoolList<IFilterBase>.Recycle(ref this.filters);
                
            }
            this.filters = PoolList<IFilterBase>.Spawn(other.filters.Count);

            for (int i = 0, count = other.filters.Count; i < count; ++i) {
                
                this.filters.Add(other.filters[i].Clone());
                
            }

        }

    }

    public class Filter<TState, TEntity> : IFilterInternal<TState>, IFilter<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        public Filter() {}

        internal Filter(string name) {

            this.name = name;

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

        public int id { get; set; }
        private string name;
        private IFilterNode[] nodes = null;
        private int nodesCount;
        private HashSetCopyable<EntityId> dataContains;
        private HashSetCopyable<Entity> data;

        private List<IFilterNode> tempNodes;
        private List<IFilterNode> tempNodesCustom;

        void IPoolableSpawn.OnSpawn() {
            
            this.nodes = PoolArray<IFilterNode>.Spawn(1000);
            this.data = PoolHashSetCopyable<Entity>.Spawn();
            this.dataContains = PoolHashSetCopyable<EntityId>.Spawn();

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<IFilterNode>.Recycle(ref this.nodes);
            PoolHashSetCopyable<Entity>.Recycle(ref this.data);
            PoolHashSetCopyable<EntityId>.Recycle(ref this.dataContains);
            
        }

        public void CopyFrom(Filter<TState, TEntity> other) {

            this.id = other.id;
            this.name = other.name;
            this.nodesCount = other.nodesCount;
            
            if (this.nodes != null) PoolArray<IFilterNode>.Recycle(ref this.nodes);
            this.nodes = PoolArray<IFilterNode>.Spawn(other.nodes.Length);
            for (int i = 0; i < this.nodes.Length; ++i) {

                this.nodes[i] = other.nodes[i];

            }
            
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

        public HashSetCopyable<Entity>.Enumerator GetEnumerator() {

            return this.data.GetEnumerator();

        }

        bool IFilterInternal<TState>.OnUpdate(Entity entity) {

            var isExists = this.Contains_INTERNAL(entity);
            if (isExists == true) {

                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(entity) == false) {

                        return ((IFilterInternal<TState>)this).OnRemove(entity);

                    }

                }

            } else {

                return ((IFilterInternal<TState>)this).OnAdd(entity);

            }

            return false;

        }

        bool IFilterInternal<TState>.OnAddComponent(Entity entity) {

            return ((IFilterInternal<TState>)this).OnUpdate(entity);

        }

        bool IFilterInternal<TState>.OnRemoveComponent(Entity entity) {

            return ((IFilterInternal<TState>)this).OnUpdate(entity);

        }

        bool IFilterInternal<TState>.OnAdd(Entity entity) {

            for (int i = 0; i < this.nodesCount; ++i) {

                if (this.nodes[i].Execute(entity) == false) {

                    return false;

                }

            }

            this.dataContains.Add(entity.id);
            this.data.Add(entity);
            return true;

        }

        bool IFilterInternal<TState>.OnRemove(Entity entity) {

            var res = this.dataContains.Remove(entity.id);
            if (res == true) this.data.Remove(entity);
            return res;

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

        public override string ToString() {

            return "Name: " + this.name + ", Objects Count: " + this.Count.ToString();

        }

    }

}