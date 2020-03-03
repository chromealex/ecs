using ME.ECS;

namespace Warcraft.Entities {
    
    public struct CameraEntity : IEntity {
        
        public Entity entity { get; set; }

        public UnityEngine.Vector3 position;
        public UnityEngine.Vector2 cameraSize;

    }
    
}