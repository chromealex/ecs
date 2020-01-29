using ME.ECS;

namespace ME.GameExample.Game.Components {

    using ME.GameExample.Game.Entities;
    
    public class PointAddPositionDelta : IComponentOnce<GameState, Point> {

        public UnityEngine.Vector3 positionDelta;

        public void AdvanceTick(GameState state, ref Point data, float deltaTime, int index) {

            data.position += this.positionDelta * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, Point>.CopyFrom(IComponent<GameState, Point> other) {

            this.positionDelta = ((PointAddPositionDelta)other).positionDelta;

        }

    }

}