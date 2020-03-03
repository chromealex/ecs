using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class BuildingInProgressCountAtWorkPlace : IComponentCopyable<TState, TEntity> {

        public int count;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (BuildingInProgressCountAtWorkPlace)other;
            this.count = _other.count;

        }

        void IPoolableRecycle.OnRecycle() {

            this.count = default;

        }

    }

}