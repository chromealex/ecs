using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerSelectedUnitComponent : IComponentCopyable<TState, TEntity> {

        public Entity unit;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PlayerSelectedUnitComponent)other;
            this.unit = _other.unit;

        }

        void IPoolableRecycle.OnRecycle() {

            this.unit = default;

        }

    }
    
}