using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features;
    
    public class UnitsGravitySystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitsGravitySystem").WithStructComponent<UnitGravity>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);

            { // Gravity
                    
                var gravity = this.world.GetData<UnitGravity>(entity);
                data.position.y -= gravity.gravity * deltaTime;
                if (data.position.y <= state.worldPosition.y) {

                    data.position.y = state.worldPosition.y;

                }
                    
            }

        }

    }
    
}