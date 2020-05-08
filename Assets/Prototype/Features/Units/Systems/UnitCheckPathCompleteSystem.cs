using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitCheckPathCompleteSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitCheckPathCompleteSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<IsSquad>()
                                          .WithComponent<Path>()
                                          .WithStructComponent<PathTraverse>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            var pathComp = this.world.GetComponent<Unit, Path>(entity);
            var path = pathComp.path;
            var pathTraverse = entity.GetData<PathTraverse>();

            if (pathTraverse.index + 1 >= path.Length) {

                entity.RemoveData<PathTraverse>();
                this.world.RemoveComponents<TEntity, Path>(entity);
                entity.SetData(new Speed() { value = 0f });
                entity.SetData(new IsPathComplete());
                entity.RemoveData<IsPlayerCommand>();
                entity.RemoveData<MoveToTarget>();

            }
            
        }

    }
    
}