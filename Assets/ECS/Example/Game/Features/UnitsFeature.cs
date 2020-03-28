using ME.ECS;

namespace ME.Example.Game.Features {
    
    using ME.Example.Game.Systems;
    
    public class UnitsFeature : Feature<State> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.AddSystem<UnitsSystem>();
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}