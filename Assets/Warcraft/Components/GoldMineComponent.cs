using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class GoldMineComponent : IComponentCopyable<TState, TEntity> {

        public int capacity;
        
        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var _other = (GoldMineComponent)other;
            this.capacity = _other.capacity;

        }

        void IPoolableRecycle.OnRecycle() {

            this.capacity = default;

        }

    }
    
}