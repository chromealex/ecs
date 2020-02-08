using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using EntityId = System.Int32;

    internal interface IFilterInternal<TState, in TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        bool OnUpdate(TEntity data);
        bool OnAdd(TEntity data);
        bool OnRemove(Entity entity);

    }

    public interface IFilterBase {
        
        int Count { get; }
        bool Contains(Entity entity);

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

    public class Filter<TState, TEntity> : IFilterInternal<TState, TEntity>, IFilter<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        private Filter() {}

        private Filter(string name) {

            this.name = name;

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

        private string name;
        private INode<TEntity>[] nodes = null;
        private List<INode<TEntity>> tempNodes = new List<INode<TEntity>>();
        private List<INode<TEntity>> tempNodesCustom = new List<INode<TEntity>>();
        private int nodesCount;
        private HashSet<EntityId> data = new HashSet<EntityId>();

        public int Count {
            get {
                return this.data.Count;
            }
        }

        public bool Contains(TEntity data) {

            return this.Contains_INTERNAL(data.entity);

        }

        public bool Contains(Entity entity) {
            
            return this.Contains_INTERNAL(entity);
            
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
            
            filter = new Filter<TState, TEntity>(customName != null ? customName : nameof(filter));
            return filter;

        }

        public override string ToString() {

            return "Name: " + this.name + ", Objects Count: " + this.Count.ToString();

        }

    }

}