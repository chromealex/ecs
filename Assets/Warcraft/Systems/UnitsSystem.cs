using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {

        }
        
        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {}
        
    }
    
}