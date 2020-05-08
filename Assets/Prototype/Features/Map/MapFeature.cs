using ME.ECS;

namespace Prototype.Features {
    
    using TState = PrototypeState;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class MapFeature : Feature<TState> {

        public Prototype.Features.Map.Data.FeatureMapData resourcesData { get; private set; }

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            this.resourcesData = UnityEngine.Resources.Load<Prototype.Features.Map.Data.FeatureMapData>("FeatureMapData");

            var viewId = this.world.RegisterViewSource<Entities.Map>(this.resourcesData.mapView);

            // Create map
            var map = this.world.AddEntity(new Entities.Map());
            this.world.InstantiateView<Entities.Map>(viewId, map);
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}