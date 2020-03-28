using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitGravity : IRunnableComponent<State, Unit> {

        public float gravity = 9.8f;

        public void AdvanceTick(State state, ref Unit data, float deltaTime, int index) {

            data.position.y -= this.gravity * deltaTime;
            if (data.position.y <= state.worldPosition.y) {

                data.position.y = state.worldPosition.y;

            }

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponentCopyable<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            var otherUnit = ((UnitGravity)other);
            this.gravity = otherUnit.gravity;

        }

    }

}