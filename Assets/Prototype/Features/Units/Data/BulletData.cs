using ME.ECS;

namespace Prototype.Features.Units.Data {
    
    using TState = PrototypeState;
    
    [UnityEngine.CreateAssetMenu()]
    public class BulletData : UnityEngine.ScriptableObject {

        [UnityEngine.HeaderAttribute("Data")]
        public Prototype.Features.Units.Views.BulletView viewSource;
        public float damage;
        public float rotationSpeed;
        public float movementSpeed;
        public float accelerationSpeed;
        public float slowdownSpeed;
        
    }

}