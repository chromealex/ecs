using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitInSquadMoveSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitInSquadMoveSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<IsUnit>()
                                          .WithStructComponent<Squad>()
                                          .WithoutStructComponent<AttackTarget>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var squad = entity.GetData<Squad>();
            var offset = FormationUtils.GetPosition(squad.index, squad.entity.GetData<SquadSize>().value);
            var targetLocalPosition = new UnityEngine.Vector3(offset.x, 0f, offset.y);

            var center = squad.entity.GetPosition();
            var targetPosition = center + squad.entity.GetRotation() * targetLocalPosition;

            entity.SetData(new MoveToTarget() { value = targetPosition });

        }

    }
    
}