using ME.ECS;

namespace Prototype.Features.Players.Data {
    
    using TState = PrototypeState;
    
    [UnityEngine.CreateAssetMenu()]
    public class FeaturePlayersData : UnityEngine.ScriptableObject {

        public Prototype.Features.Players.Views.SelectorView selectorBegin;
        public Prototype.Features.Players.Views.SelectorView selectorTarget;

    }

}