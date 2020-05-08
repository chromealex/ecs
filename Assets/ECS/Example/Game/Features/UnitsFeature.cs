using ME.ECS;

namespace ME.Example.Game.Features {
    
    using ME.Example.Game.Systems;
    
    public class UnitsFeature : Feature<State> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.AddSystem<UnitsReachPointSystem>();
            this.AddSystem<UnitsColorSystem>();
            this.AddSystem<UnitsDeathSystem>();
            this.AddSystem<UnitsFollowSystem>();
            this.AddSystem<UnitsGravitySystem>();
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}