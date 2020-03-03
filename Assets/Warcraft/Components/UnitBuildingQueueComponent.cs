using ME.ECS;

namespace Warcraft.Components {

    using TState = WarcraftState;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class UnitBuildingQueueComponent : IComponentCopyable<TState, TEntity> {

        public System.Collections.Generic.List<Entity> units;
        
        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var _other = (UnitBuildingQueueComponent)other;
            if (this.units != null) PoolList<Entity>.Recycle(ref this.units);
            if (_other.units != null) {

                this.units = PoolList<Entity>.Spawn(_other.units.Capacity);
                this.units.AddRange(_other.units);

            } else {

                this.units = null;

            }

        }

        void IPoolableRecycle.OnRecycle() {

            if (this.units != null) PoolList<Entity>.Recycle(ref this.units);
            
        }

    }

}