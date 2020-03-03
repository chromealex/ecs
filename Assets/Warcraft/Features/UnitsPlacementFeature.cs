using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class UnitsPlacementFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<Warcraft.Systems.UnitsPlacementSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}