using ME.ECS;

namespace Prototype.Features.Units.Components {
    
    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    
    public class Path : IComponentCopyable<TState, TEntity> {
        
        public UnityEngine.Vector3[] path;
        public Pathfinding.GraphNode[] nodes;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (Path)other;
            ArrayUtils.Copy(_other.path, ref this.path); 
            ArrayUtils.Copy(_other.nodes, ref this.nodes); 

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<UnityEngine.Vector3>.Recycle(ref this.path);
            if (this.nodes != null) PoolArray<Pathfinding.GraphNode>.Recycle(ref this.nodes);
            
        }

    }
    
}