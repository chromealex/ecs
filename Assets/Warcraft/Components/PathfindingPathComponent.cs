using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class PathfindingPathComponent : IComponentCopyable<TState, TEntity> {

        public System.Collections.Generic.List<Pathfinding.GraphNode> nodes;
        public int index;
        public UnityEngine.Vector2 from;
        public UnityEngine.Vector2 to;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PathfindingPathComponent)other;
            if (this.nodes != null) PoolList<Pathfinding.GraphNode>.Recycle(ref this.nodes);
            this.nodes = PoolList<Pathfinding.GraphNode>.Spawn(_other.nodes.Capacity);
            this.nodes.AddRange(_other.nodes);
            this.index = _other.index;
            this.from = _other.@from;
            this.to = _other.@to;

        }

        void IPoolableRecycle.OnRecycle() {
            
            if (this.nodes != null) PoolList<Pathfinding.GraphNode>.Recycle(ref this.nodes);
            this.index = default;
            this.from = default;
            this.to = default;

        }

    }
    
}