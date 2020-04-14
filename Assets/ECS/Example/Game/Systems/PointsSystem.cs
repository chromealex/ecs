using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = ME.Example.Game.Entities.Point;
    using Components;
    
    public class PointsSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}

        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-PointsSystemFilter").WithStructComponent<PointAddPositionDelta>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<TEntity>(entity);
            var positionDelta = this.world.GetData<PointAddPositionDelta>(entity).positionDelta;
            data.position += positionDelta;
            this.world.RemoveData<PointAddPositionDelta>(entity);

        }

    }
    
}