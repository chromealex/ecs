using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features;
    
    public class UnitsDeathSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitsDeathSystem").Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);
            if (data.lifes <= 0) {

                ++state.unitsCount;
                this.world.RemoveEntity<Unit>(entity);

            }

        }

    }
    
}