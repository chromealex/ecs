using ME.ECS;

namespace ME.GameExample.Game.Components {

    using ME.GameExample.Game.Entities;

    public class UnitGravity : IComponent<GameState, Unit> {

        public float gravity = 9.8f;

        public void AdvanceTick(GameState state, ref Unit data, float deltaTime, int index) {

            data.position.y -= this.gravity * deltaTime;
            if (data.position.y <= state.worldPosition.y) {

                data.position.y = state.worldPosition.y;

            }

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, Unit>.CopyFrom(IComponent<GameState, Unit> other) {

            var otherUnit = ((UnitGravity)other);
            this.gravity = otherUnit.gravity;

        }

    }

}