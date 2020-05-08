using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class SquadSwapOnCompleteSystem : ISystemFilter<TState> {

        private UnitsFeature unitsFeature;
        private IFilter<TState, TEntity> allSquads;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            this.unitsFeature = this.world.GetFeature<UnitsFeature>();
            Filter<TState, TEntity>.Create(ref this.allSquads, "Filter-SquadSwapOnCompleteSystem-All").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().WithoutStructComponent<PathTraverse>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-SquadSwapOnCompleteSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<IsSquad>()
                                          .WithoutStructComponent<BuildPathToTarget>()
                                          .WithStructComponent<IsPathComplete>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            if (entity.HasData<BuildPathToTarget>() == true) return;

            var i = 0;
            foreach (var squad in this.allSquads) {

                if (squad != entity &&
                    squad.HasData<BuildPathToTarget>() == false &&
                    squad.GetData<Owner>().value == entity.GetData<Owner>().value &&
                    squad.GetData<TargetNode>().nodeIndex == entity.GetData<TargetNode>().nodeIndex) {

                    this.unitsFeature.Push(squad, this.allSquads, i++);

                }

            }

        }

    }
    
}