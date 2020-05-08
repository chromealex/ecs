using ME.ECS;

namespace Prototype.Features.Input.Data {
    
    using TState = PrototypeState;
    
    [UnityEngine.CreateAssetMenu()]
    public class FeatureInputData : UnityEngine.ScriptableObject {

        public UnityEngine.LayerMask groundMask;
        public float dragThreshold;

    }

}