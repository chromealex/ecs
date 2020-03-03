using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitSpeedComponent : IComponentCopyable<TState, TEntity> {

        public float speed;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var _other = (UnitSpeedComponent)other;
            this.speed = _other.speed;

        }

        void IPoolableRecycle.OnRecycle() {

            this.speed = default;

        }

    }
    
}