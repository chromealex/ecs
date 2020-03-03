using ME.ECS;

namespace Warcraft.Components.PeasantStates {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class PeasantCarryGold : IComponentCopyable<TState, TEntity> {
        
        public int value;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var _other = (PeasantCarryGold)other;
            this.value = _other.value;

        }

        void IPoolableRecycle.OnRecycle() {

            this.value = default;

        }

    }
    
}