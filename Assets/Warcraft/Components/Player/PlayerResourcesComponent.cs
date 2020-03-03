using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerResourcesComponent : IComponentCopyable<TState, TEntity> {

        public ResourcesStorage resources;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PlayerResourcesComponent)other;
            this.resources = _other.resources;

        }

        void IPoolableRecycle.OnRecycle() {

            this.resources = default;

        }

    }
    
}