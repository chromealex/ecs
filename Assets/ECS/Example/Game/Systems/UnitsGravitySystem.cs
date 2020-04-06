using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features;
    
    public class UnitsGravitySystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitsGravitySystem").Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);

            { // Gravity
                    
                var gravity = this.world.GetComponent<Unit, UnitGravity>(entity);
                data.position.y -= gravity.gravity * deltaTime;
                if (data.position.y <= state.worldPosition.y) {

                    data.position.y = state.worldPosition.y;

                }
                    
            }

        }

    }
    
}