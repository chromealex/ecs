using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitFollowFromTo : IComponent<State, Unit> {

        public Entity from;
        public Entity to;

        public void AdvanceTick(State state, ref Unit data, float deltaTime, int index) {

            Point toData;
            if (this.GetEntityData(this.to.id, out toData) == true) {

                var toPos = toData.position;
                data.position = UnityEngine.Vector3.MoveTowards(data.position, toPos, data.speed * deltaTime);
                data.position.y = toPos.y;

            }

        }

        void IPoolableRecycle.OnRecycle() {}

        void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            var otherUnit = ((UnitFollowFromTo)other);
            this.from = otherUnit.from;
            this.to = otherUnit.to;
            
        }

    }

}