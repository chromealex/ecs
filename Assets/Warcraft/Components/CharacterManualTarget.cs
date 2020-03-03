using ME.ECS;

namespace Warcraft.Components {

    using TState = Warcraft.WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class CharacterManualTarget : IComponentCopyable<TState, TEntity> {

        public UnityEngine.Vector2 target;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (CharacterManualTarget)other;
            this.target = _other.target;

        }

        void IPoolableRecycle.OnRecycle() {

            this.target = default;

        }

    }

}