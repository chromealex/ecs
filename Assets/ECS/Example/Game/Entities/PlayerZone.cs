using ME.ECS;

namespace ME.Example.Game.Entities {
    
    public struct PlayerZone : IEntity {
        
        public Entity entity { get; set; }

        public UnityEngine.Vector3 position;
        public UnityEngine.Vector3 scale;
        public UnityEngine.Color color;

    }
    
}