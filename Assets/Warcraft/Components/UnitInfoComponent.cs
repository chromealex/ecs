using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitInfoComponent : IComponentCopyable<TState, TEntity> {

        public UnitInfo unitInfo;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (UnitInfoComponent)other;
            this.unitInfo = _other.unitInfo;

        }

        void IPoolableRecycle.OnRecycle() {

            this.unitInfo = default;

        }

    }
    
}