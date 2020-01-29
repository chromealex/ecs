using ME.ECS;

namespace ME.GameExample.Game.Components {

    using ME.GameExample.Game.Entities;

    public class PointSetColor : IComponentOnce<GameState, Point> {

        public UnityEngine.Color color;

        public void AdvanceTick(GameState state, ref Point data, float deltaTime, int index) {

            data.color = this.color;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, Point>.CopyFrom(IComponent<GameState, Point> other) {

            this.color = ((PointSetColor)other).color;

        }

    }

}