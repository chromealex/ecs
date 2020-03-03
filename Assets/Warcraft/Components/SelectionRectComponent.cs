using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.SelectionRectEntity;
    
    public class SelectionRectComponent : IComponentCopyable<TState, TEntity> {
        
        public UnityEngine.Vector2 worldPosition;
        public UnityEngine.Vector2 size;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (SelectionRectComponent)other;
            this.worldPosition = _other.worldPosition;
            this.size = _other.size;

        }

        void IPoolableRecycle.OnRecycle() {

            this.worldPosition = default;
            this.size = default;

        }

    }
    
}