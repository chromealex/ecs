using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Systems;
    
    public class PeasantsFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<PeasantsSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}