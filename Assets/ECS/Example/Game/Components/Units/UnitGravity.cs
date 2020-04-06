using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitGravity : IComponentCopyable<State, Unit> {

        public float gravity = 9.8f;

        void IPoolableRecycle.OnRecycle() {

            this.gravity = 9.8f;

        }

        void IComponentCopyable<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            var otherUnit = ((UnitGravity)other);
            this.gravity = otherUnit.gravity;

        }

    }

}