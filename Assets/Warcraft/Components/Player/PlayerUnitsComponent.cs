using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerUnitsComponent : IComponentCopyable<TState, TEntity> {

        public ulong units;

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var _other = (PlayerUnitsComponent)other;
            this.units = _other.units;

        }

        void IPoolableRecycle.OnRecycle() {

            this.units = default;

        }

        public void AddUnit(int unitTypeId) {

            this.units |= (ulong)(long)(1 << unitTypeId);

        }
        
        public void RemoveUnit(int unitTypeId) {
            
            this.units &= ~(ulong)(1 << unitTypeId);
            
        }
        
        public bool HasUnit(int unitTypeId) {
            
            return (this.units & (ulong)(1 << unitTypeId)) != 0;
            
        }

    }
    
}