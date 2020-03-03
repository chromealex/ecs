using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitBuildingProgress : IComponentCopyable<TState, TEntity> {

        public float time;
        public float progress;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (UnitBuildingProgress)other;
            this.time = _other.time;
            this.progress = _other.progress;

        }

        void IPoolableRecycle.OnRecycle() {

            this.time = default;
            this.progress = default;

        }

    }
    
}