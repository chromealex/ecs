using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class DebugFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.AddSystem<Warcraft.Systems.DebugSystem>();
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}