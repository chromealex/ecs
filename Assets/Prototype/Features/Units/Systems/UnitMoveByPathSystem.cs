using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitMoveByPathSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitMoveByPathSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().WithComponent<Path>().WithStructComponent<PathTraverse>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var path = this.world.GetComponent<Unit, Path>(entity).path;
            var pathIndex = entity.GetData<PathTraverse>().index;
            var targetNode = path[pathIndex + 1];

            entity.SetData(new MoveToTarget() { value = targetNode });

            if (pathIndex + 1 < path.Length) {
                
                // Accelerate
                entity.SetData(new SlowdownDistance() { value = 0f });
                
            } else {
                
                // Slowdown
                entity.SetData(new SlowdownDistance() { value = 3f });
                
            }

        }

    }
    
}