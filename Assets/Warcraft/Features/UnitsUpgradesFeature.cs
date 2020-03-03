using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class UnitsUpgradesFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.AddSystem<Warcraft.Systems.UnitsUpgradesSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}