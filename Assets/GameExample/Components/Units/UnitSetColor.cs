using ME.ECS;

namespace ME.GameExample.Game.Components {

    using ME.GameExample.Game.Entities;

    public class UnitSetColor : IComponentOnce<GameState, Unit> {

        public UnityEngine.Color color;

        public void AdvanceTick(GameState state, ref Unit data, float deltaTime, int index) {

            data.color = this.color;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, Unit>.CopyFrom(IComponent<GameState, Unit> other) {

            this.color = ((UnitSetColor)other).color;

        }

    }

}