using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = Entities.PlayerZone;
    using Entities; using Components; using Modules; using Systems; using Features;
    
    public class PlayerZonesSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-PlayerZonesSystem").Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

        }

    }
    
}