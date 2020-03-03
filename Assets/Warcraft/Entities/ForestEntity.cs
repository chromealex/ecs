using ME.ECS;
using UnityEngine;

namespace Warcraft.Entities {
    
    public struct ForestEntity : IEntity {
        
        public Entity entity { get; set; }

        public Vector2Int position;

    }
    
}