using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class PointIncreaseUnits : IRunnableComponent<State, Point> {

        public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

            data.unitsCount += data.increaseRate * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponentCopyable<State, Point>.CopyFrom(IComponent<State, Point> other) { }

    }

    public class PointIncreaseUnitsOnce : IRunnableComponentOnce<State, Point> {

        public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

            data.unitsCount += data.increaseRate * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponentCopyable<State, Point>.CopyFrom(IComponent<State, Point> other) { }

    }

}