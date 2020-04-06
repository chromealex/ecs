using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class PointSetColor : IComponentCopyable<State, Point> {

        public UnityEngine.Color color;

        void IPoolableRecycle.OnRecycle() {

            this.color = default;

        }

        void IComponentCopyable<State, Point>.CopyFrom(IComponent<State, Point> other) {

            this.color = ((PointSetColor)other).color;

        }

    }

}