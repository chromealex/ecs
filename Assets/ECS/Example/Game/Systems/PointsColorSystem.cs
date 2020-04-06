using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = Entities.Point;
    using Entities; using Components; using Modules; using Systems; using Features;
    
    public class PointsColorSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-PointsColorSystem").WithComponent<PointSetColor>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Point>(entity);
            data.color = this.world.GetComponent<Point, PointSetColor>(entity).color;
            this.world.RemoveComponents<Point, PointSetColor>(entity);

        }

    }
    
}