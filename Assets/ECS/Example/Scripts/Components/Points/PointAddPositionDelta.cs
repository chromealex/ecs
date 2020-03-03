using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class PointAddPositionDelta : IRunnableComponentOnce<State, Point> {

        public UnityEngine.Vector3 positionDelta;

        public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

            data.position += this.positionDelta * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponentCopyable<State, Point>.CopyFrom(IComponent<State, Point> other) {

            this.positionDelta = ((PointAddPositionDelta)other).positionDelta;

        }

    }

}