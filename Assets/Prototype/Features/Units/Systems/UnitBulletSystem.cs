using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitBulletSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitBulletSystem")
                                          .WithStructComponent<IsBullet>()
                                          .WithStructComponent<IsMoveToTargetComplete>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var target = entity.GetData<AttackTarget>().entity;
            
            var hit = target.GetData<Hit>();
            hit.value += entity.GetData<Damage>().value;
            target.SetData(hit);

            this.world.RemoveEntity<Unit>(entity);

        }

    }
    
}