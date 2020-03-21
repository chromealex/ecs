using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = ME.Example.Game.Entities.Point;
    using Components;
    
    public class PointsSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-PointsSystemFilter").Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<TEntity>(entity);
            this.world.RunComponents(ref data, deltaTime, 0);

        }

    }
    
}