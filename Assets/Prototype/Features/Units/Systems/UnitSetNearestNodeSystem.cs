using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitSetNearestNodeSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitSetNearestNodeSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            var nodeInfo = AstarPath.active.GetNearest(entity.GetPosition());
            entity.SetData(new NearestNode() {
                position = (UnityEngine.Vector3)nodeInfo.node.position,
                nodeIndex = nodeInfo.node.NodeIndex
            });

        }

    }
    
}