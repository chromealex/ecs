using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitGravity : IComponent<State, Unit> {

        public float gravity = 9.8f;

        public void AdvanceTick(State state, ref Unit data, float deltaTime, int index) {

            data.position.y -= this.gravity * deltaTime;
            if (data.position.y <= 0f) {

                data.position.y = 0f;

            }

        }

        void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            var otherUnit = ((UnitGravity)other);
            this.gravity = otherUnit.gravity;

        }

    }

}