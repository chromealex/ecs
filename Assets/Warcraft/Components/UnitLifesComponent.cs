using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitLifesComponent : IComponentCopyable<TState, TEntity> {

        public int lifes;
        
        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (UnitLifesComponent)other;
            this.lifes = _other.lifes;

        }

        void IPoolableRecycle.OnRecycle() {

            this.lifes = default;

        }

    }
    
}