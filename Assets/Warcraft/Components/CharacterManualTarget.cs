using ME.ECS;

namespace Warcraft.Components {

    using TState = Warcraft.WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;

    public class CharacterTarget : IComponentCopyable<TState, TEntity> {

        public TEntity target;
        public float delay;
        public float timer;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (CharacterTarget)other;
            this.target = _other.target;
            this.delay = _other.delay;
            this.timer = _other.timer;

        }

        void IPoolableRecycle.OnRecycle() {

            this.target = default;
            this.delay = default;
            this.timer = default;

        }

    }

    public class CharacterAutoTarget : IComponentCopyable<TState, TEntity> {

        public UnityEngine.Vector2 target;
        public float targetRange;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (CharacterAutoTarget)other;
            this.target = _other.target;
            this.targetRange = _other.targetRange;

        }

        void IPoolableRecycle.OnRecycle() {

            this.target = default;
            this.targetRange = default;
            
        }

    }

    public class CharacterManualTarget : IComponentCopyable<TState, TEntity> {

        public UnityEngine.Vector2 target;
        public float targetRange;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (CharacterManualTarget)other;
            this.target = _other.target;
            this.targetRange = _other.targetRange;

        }

        void IPoolableRecycle.OnRecycle() {

            this.target = default;
            this.targetRange = default;

        }

    }

}