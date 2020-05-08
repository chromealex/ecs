using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class SquadSwapSystem : ISystemFilter<TState> {

        private UnitsFeature unitsFeature;
        private IFilter<TState, TEntity> allSquads;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.unitsFeature = this.world.GetFeature<UnitsFeature>();
            Filter<TState, TEntity>.Create(ref this.allSquads, "Filter-SquadSwapSystem-All").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().Push();
            
        }
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        public IFilter<TState> filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-SquadSwapSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().WithStructComponent<PathTraverse>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            var pathComp = this.world.GetComponent<Unit, Path>(entity);
            var pathIndex = entity.GetData<PathTraverse>().index;
            
            if (pathIndex + 2 >= pathComp.path.Length) {

                var targetNode = pathComp.nodes[pathComp.nodes.Length - 1];
                
                var i = 0;
                foreach (var squad in this.allSquads) {

                    if (squad != entity &&
                        squad.GetData<Owner>().value == entity.GetData<Owner>().value &&
                        squad.HasData<PathTraverse>() == false &&
                        squad.GetData<TargetNode>().nodeIndex == targetNode.NodeIndex) {

                        this.unitsFeature.Push(squad, this.allSquads, i++);
                        
                    }

                }

            }

        }

    }
    
}