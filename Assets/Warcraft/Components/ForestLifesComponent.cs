using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.ForestEntity;
    
    public class ForestLifesComponent : IComponentCopyable<TState, TEntity> {

        public int lifes;
        
        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (ForestLifesComponent)other;
            this.lifes = _other.lifes;

        }

        void IPoolableRecycle.OnRecycle() {

            this.lifes = default;

        }

    }
    
}