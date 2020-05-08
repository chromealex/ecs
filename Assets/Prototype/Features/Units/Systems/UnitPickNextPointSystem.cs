using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitPickNextPointSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitPickNextPointSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().WithComponent<Path>().WithStructComponent<PathTraverse>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            var pathComp = this.world.GetComponent<Unit, Path>(entity);
            var path = pathComp.path;
            var pathTraverse = entity.GetData<PathTraverse>();
            if (pathTraverse.index + 1 >= path.Length) return;
            
            var targetPathPoint = path[pathTraverse.index + 1];
            var rng = entity.GetData<PickNextPointDistance>().value;
            var position = entity.GetPosition();
            if ((position - targetPathPoint).sqrMagnitude <= rng * rng) {

                ++pathTraverse.index;
                entity.SetData(new TargetNode() { nodeIndex = AstarPath.active.GetNearest(targetPathPoint).node.NodeIndex });
                entity.SetData(pathTraverse);

            }
            
        }

    }
    
}