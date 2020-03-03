using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsSystem : ISystem<TState> {
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {

        }
        
        void ISystem<TState>.Update(TState state, float deltaTime) {}
        
    }
    
}