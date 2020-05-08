using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitBuildPathSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitBuildPathSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().WithStructComponent<BuildPathToTarget>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var nodeInfo = AstarPath.active.GetNearest(entity.GetPosition());
            var toPos = entity.GetData<BuildPathToTarget>().value;
            var path = Pathfinding.ABPath.Construct((UnityEngine.Vector3)nodeInfo.node.position, toPos);
            AstarPath.StartPath(path);
            AstarPath.BlockUntilCalculated(path);

            var pathComp = this.world.AddOrGetComponent<TEntity, Path>(entity);
            pathComp.path = path.vectorPath.ToArray();
            pathComp.nodes = path.path.ToArray();

            entity.SetData(new PathTraverse() { index = -1 });
            
            entity.RemoveData<BuildPathToTarget>();

        }

    }
    
}