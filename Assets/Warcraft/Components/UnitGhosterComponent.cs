using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitGhosterComponent : IComponentCopyable<TState, TEntity> {

        public ActionsGraphNode actionInfo;
        public bool isValid;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (UnitGhosterComponent)other;
            this.actionInfo = _other.actionInfo;
            this.isValid = _other.isValid;

        }

        void IPoolableRecycle.OnRecycle() {

            this.actionInfo = default;
            this.isValid = default;

        }

    }
    
}