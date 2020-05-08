using ME.ECS;

namespace Prototype.Features.Units.Data {
    
    using TState = PrototypeState;
    
    [UnityEngine.CreateAssetMenu()]
    public class UnitData : UnityEngine.ScriptableObject {

        [UnityEngine.HeaderAttribute("Header")]
        public int index;

        [UnityEngine.HeaderAttribute("Data")]
        public Prototype.Features.Units.Views.UnitView viewSource;
        public BulletData bulletData;
        public float health;
        public float rotationSpeed;
        public float movementSpeed;
        public float accelerationSpeed;
        public float slowdownSpeed;
        public float attackRange;
        public float attackSpeed;

    }

}