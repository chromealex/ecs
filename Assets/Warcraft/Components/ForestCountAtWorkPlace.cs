using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.ForestEntity;
    
    public class ForestCountAtWorkPlace : IComponentCopyable<TState, TEntity> {

        public int count;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (ForestCountAtWorkPlace)other;
            this.count = _other.count;

        }

        void IPoolableRecycle.OnRecycle() {

            this.count = default;

        }

    }

}