using ME.ECS;

namespace Prototype.Features {
    
    using TState = PrototypeState;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class BuildingsFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}