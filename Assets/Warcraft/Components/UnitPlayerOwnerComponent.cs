using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;

    public class UnitPlayerOwnerComponent : IComponentCopyable<TState, Warcraft.Entities.UnitEntity> {

        public Entity player;
        
        void IComponentCopyable<TState, Warcraft.Entities.UnitEntity>.CopyFrom(IComponent<TState, Warcraft.Entities.UnitEntity> other) {

            var _other = (UnitPlayerOwnerComponent)other;
            this.player = _other.player;

        }

        void IPoolableRecycle.OnRecycle() {

            this.player = default;

        }

    }
    
}