using ME.ECS;

namespace Warcraft.Components.CharacterStates {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class CharacterIdleState : IComponentCopyable<TState, TEntity> {

        public Pathfinding.GraphNode idleNode;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var _other = (CharacterIdleState)other;
            this.idleNode = _other.idleNode;

        }

        void IPoolableRecycle.OnRecycle() {

            this.idleNode = default;

        }

    }
    
}