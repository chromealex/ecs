using ME.ECS;

namespace Prototype.Features.Units.Data {
    
    using TState = PrototypeState;
    
    [UnityEngine.CreateAssetMenu()]
    public class SquadData : UnityEngine.ScriptableObject {

        [UnityEngine.HeaderAttribute("Header")]
        public int index;

        [UnityEngine.HeaderAttribute("Data")]
        public int squadCount;
        public UnitData unitData;
        public float rotationSpeed;
        public float movementSpeed;
        public float accelerationSpeed;
        public float slowdownSpeed;
        public float pickNextPointDistance;
        public int attackRangeInNodes;

    }

}