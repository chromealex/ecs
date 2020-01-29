using ME.ECS;

namespace ME.GameExample.Game.Entities {
    
    public struct Explosion : IEntity {
        
        public Entity entity { get; set; }

        public UnityEngine.Vector3 position;
        public float range;
        public float damage;
        public float lifetime;

    }
    
}