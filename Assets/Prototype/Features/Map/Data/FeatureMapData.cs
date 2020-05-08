using ME.ECS;

namespace Prototype.Features.Map.Data {
    
    using TState = PrototypeState;
    
    [UnityEngine.CreateAssetMenu()]
    public class FeatureMapData : UnityEngine.ScriptableObject {

        public Prototype.Features.Map.Views.MapView mapView;

    }

}