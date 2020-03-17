using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class AIFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {

            //this.AddSystem<Warcraft.Systems.AISystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}