using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class GoldMineUnitsMaxPerMine : IComponentCopyable<TState, TEntity> {

        public int current;
        public int max;
        
        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (GoldMineUnitsMaxPerMine)other;
            this.current = _other.current;
            this.max = _other.max;

        }

        void IPoolableRecycle.OnRecycle() {

            this.current = default;
            this.max = default;

        }

    }
    
}