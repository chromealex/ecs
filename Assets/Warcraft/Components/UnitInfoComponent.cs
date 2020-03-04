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

    public class UnitCostComponent : IComponentCopyable<TState, TEntity> {

        public ResourcesStorage cost;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (UnitCostComponent)other;
            this.cost = _other.cost;

        }

        void IPoolableRecycle.OnRecycle() {

            this.cost = default;

        }

    }

}