using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class UnitsMovementFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<Warcraft.Systems.UnitsMovementSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}