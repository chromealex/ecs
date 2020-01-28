using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class PointIncreaseUnits : IComponent<State, Point> {

        public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

            data.unitsCount += data.increaseRate * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) { }

    }

    public class PointIncreaseUnitsOnce : IComponentOnce<State, Point> {

        public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

            data.unitsCount += data.increaseRate * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) { }

    }

}