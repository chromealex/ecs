using ME.ECS;

namespace ME.GameExample.Game.Components {

    using ME.GameExample.Game.Entities;

    public class PointIncreaseUnits : IComponent<GameState, Point> {

        public void AdvanceTick(GameState state, ref Point data, float deltaTime, int index) {

            data.unitsCount += data.increaseRate * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, Point>.CopyFrom(IComponent<GameState, Point> other) { }

    }

    public class PointIncreaseUnitsOnce : IComponentOnce<GameState, Point> {

        public void AdvanceTick(GameState state, ref Point data, float deltaTime, int index) {

            data.unitsCount += data.increaseRate * deltaTime;

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<GameState, Point>.CopyFrom(IComponent<GameState, Point> other) { }

    }

}