using ME.ECS;

namespace Warcraft.Entities {

    public struct UnitEntity : IEntity {
        
        public Entity entity { get; set; }

        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2Int entrance;
        public UnityEngine.Vector2Int size;
        public UnityEngine.Quaternion rotation;

        public float workingTimer;
        public Entity jobTarget;
        public int jobTargetType;
        public UnityEngine.Vector2 jobTargetPos;

    }
    
}