using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class InputFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.AddModule<Warcraft.Modules.MouseInputModule>();
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}