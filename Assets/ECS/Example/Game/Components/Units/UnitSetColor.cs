using ME.ECS;

namespace ME.Example.Game.Components {

    using ME.Example.Game.Entities;

    public class UnitSetColor : IComponentCopyable<State, Unit> {

        public UnityEngine.Color color;

        void IPoolableRecycle.OnRecycle() {

            this.color = default;

        }

        void IComponentCopyable<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

            this.color = ((UnitSetColor)other).color;

        }

    }

}