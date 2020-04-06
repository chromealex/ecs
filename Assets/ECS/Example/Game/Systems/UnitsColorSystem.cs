using ME.ECS;

namespace ME.Example.Game.Systems {

    using TState = State;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features;
    
    public class UnitsColorSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitsColorSystem").WithComponent<UnitSetColor>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);

            var colorComp = this.world.GetComponent<Unit, UnitSetColor>(entity);
            data.color = colorComp.color;
            this.world.RemoveComponents<Unit, UnitSetColor>(entity);

        }

    }
    
}