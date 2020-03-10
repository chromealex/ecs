using ME.ECS;

namespace Warcraft.Entities {
    
    public struct DebugEntity : IEntity {
        
        public Entity entity { get; set; }

        public UnityEngine.Vector2Int pos;
        public UnityEngine.Vector3 worldPos;
        public string data;

    }
    
}