using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class UnitsQueueFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.AddSystem<Warcraft.Systems.UnitsQueueSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}