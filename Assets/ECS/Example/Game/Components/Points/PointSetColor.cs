using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class PointSetColor : IRunnableComponentOnce<State, Point> {

        public UnityEngine.Color color;

        public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

            data.color = this.color;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponentCopyable<State, Point>.CopyFrom(IComponent<State, Point> other) {

            this.color = ((PointSetColor)other).color;

        }

    }

}