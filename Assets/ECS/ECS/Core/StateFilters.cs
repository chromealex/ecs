using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using EntityId = System.Int32;

    internal interface IFilterInternal<TState, in TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        bool OnUpdate(TEntity data);
        bool OnAdd(TEntity data);
        bool OnRemove(Entity entity);

    }

    public interface IFilterBase : IPoolableSpawn, IPoolableRecycle {

        int id { get; set; }
        int Count { get; }

        IFilterBase Clone();

    }

    public interface INode<in TEntity> where TEntity : struct, IEntity {

        bool Execute(TEntity data);

    }

    public interface IFilter<TState, TEntity> : IFilterBase, IEnumerable<KeyValuePair<EntityId, TEntity>> where TState : class, IState<TState> where TEntity : struct, IEntity {

        bool Contains(TEntity data);

        IFilter<TState, TEntity> Custom(INode<TEntity> filter);
        IFilter<TState, TEntity> Custom<TFilter>() where TFilter : class, INode<TEntity>, new();
        IFilter<TState, TEntity> WithComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity>;
        IFilter<TState, TEntity> WithoutComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity>;

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

    public class Filter<TState, TEntity> : IFilterInternal<TState, TEntity>, IFilter<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        public Filter() {}

        internal Filter(string name) {

            this.name = name;

        }

        public IFilterBase Clone() {

            var instance = PoolFilters.Spawn<Filter<TState, TEntity>>();
            instance.CopyFrom(this);
            return instance;

        }

        private class ComponentExistsNode<TComponent> : INode<TEntity> where TComponent : class, IComponent<TState, TEntity> {

            public bool Execute(TEntity data) {
                
                return Worlds<TState>.currentWorld.HasComponent<TEntity, TComponent>(data.entity) == true;
                
            }

        }

        private class ComponentNotExistsNode<TComponent> : INode<TEntity> where TComponent : class, IComponent<TState, TEntity> {

            public bool Execute(TEntity data) {
                
                return Worlds<TState>.currentWorld.HasComponent<TEntity, TComponent>(data.entity) == false;
                
            }

        }

        public int id { get; set; }
        private string name;
        private INode<TEntity>[] nodes = null;
        private int nodesCount;
        private HashSet<EntityId> data;

        private List<INode<TEntity>> tempNodes;
        private List<INode<TEntity>> tempNodesCustom;

        void IPoolableSpawn.OnSpawn() {
            
            this.nodes = PoolArray<INode<TEntity>>.Spawn(1000);
            this.data = PoolHashSet<EntityId>.Spawn();

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<INode<TEntity>>.Recycle(ref this.nodes);
            PoolHashSet<EntityId>.Recycle(ref this.data);
            
        }

        public void CopyFrom(Filter<TState, TEntity> other) {

            this.id = other.id;
            this.name = other.name;
            this.nodesCount = other.nodesCount;
            
            if (this.nodes != null) PoolArray<INode<TEntity>>.Recycle(ref this.nodes);
            this.nodes = PoolArray<INode<TEntity>>.Spawn(other.nodes.Length);
            for (int i = 0; i < this.nodes.Length; ++i) {

                this.nodes[i] = other.nodes[i];

            }
            
            if (this.data != null) PoolHashSet<EntityId>.Recycle(ref this.data);
            this.data = PoolHashSet<EntityId>.Spawn(other.data.Count);
            foreach (var item in other.data) {

                this.data.Add(item);

            }

        }

        public int Count {
            get {
                return this.data.Count;
            }
        }

        public bool Contains(TEntity data) {

            var filter = Worlds<TState>.currentWorld.GetFilter<TEntity>(this.id);
            return filter.Contains_INTERNAL(data.entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool Contains_INTERNAL(Entity entity) {

            return this.data.Contains(entity.id);

        }

        IEnumerator<KeyValuePair<EntityId, TEntity>> IEnumerable<KeyValuePair<EntityId, TEntity>>.GetEnumerator() {

            return ((IEnumerable<KeyValuePair<EntityId, TEntity>>)this.data).GetEnumerator();

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {

            return ((System.Collections.IEnumerable)this.data).GetEnumerator();

        }

        bool IFilterInternal<TState, TEntity>.OnUpdate(TEntity data) {

            var isExists = this.data.Contains(data.entity.id);
            if (isExists == true) {

                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(data) == false) {

                        return ((IFilterInternal<TState, TEntity>)this).OnRemove(data.entity);

                    }

                }

            } else {

                return ((IFilterInternal<TState, TEntity>)this).OnAdd(data);

            }

            return false;

        }

        bool IFilterInternal<TState, TEntity>.OnAdd(TEntity data) {

            for (int i = 0; i < this.nodesCount; ++i) {

                if (this.nodes[i].Execute(data) == false) {

                    return false;

                }

            }

            this.data.Add(data.entity.id);
            return true;

        }

        bool IFilterInternal<TState, TEntity>.OnRemove(Entity entity) {

            return this.data.Remove(entity.id);
            
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

        public IFilter<TState, TEntity> Custom(INode<TEntity> filter) {

            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter<TState, TEntity> Custom<TFilter>() where TFilter : class, INode<TEntity>, new() {

            var filter = new TFilter();
            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter<TState, TEntity> WithComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity> {

            var node = new ComponentExistsNode<TComponent>();
            this.tempNodes.Add(node);
            return this;

        }
        
        public IFilter<TState, TEntity> WithoutComponent<TComponent>() where TComponent : class, IComponent<TState, TEntity> {

            var node = new ComponentNotExistsNode<TComponent>();
            this.tempNodes.Add(node);
            return this;

        }

        public static IFilter<TState, TEntity> Create(ref IFilter<TState, TEntity> filter, string customName = null) {
            
            var f = PoolFilters.Spawn<Filter<TState, TEntity>>();
            f.name = customName != null ? customName : nameof(filter);
            f.tempNodes = new List<INode<TEntity>>();
            f.tempNodesCustom = new List<INode<TEntity>>();
            filter = f;
            return filter;

        }

        public override string ToString() {

            return "Name: " + this.name + ", Objects Count: " + this.Count.ToString();

        }

    }

}