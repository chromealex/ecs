using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class PointAddPositionDelta : IComponentCopyable<State, Point> {

        public UnityEngine.Vector3 positionDelta;

        void IPoolableRecycle.OnRecycle() {

            this.positionDelta = default;

        }

        void IComponentCopyable<State, Point>.CopyFrom(IComponent<State, Point> other) {

            this.positionDelta = ((PointAddPositionDelta)other).positionDelta;

        }

    }

}