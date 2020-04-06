using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitFollowFromTo : IComponentCopyable<State, Unit> {

        public Entity from;
        public Entity to;

        void IPoolableRecycle.OnRecycle() {

            this.from = default;
            this.to = default;

        }

        void IComponentCopyable<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            var otherUnit = ((UnitFollowFromTo)other);
            this.from = otherUnit.from;
            this.to = otherUnit.to;
            
        }

    }

}