using ME.ECS;

namespace Warcraft.Entities {
    
    public struct PlayerEntity : IEntity {
        
        public Entity entity { get; set; }

        public int index;
        public float goldPercent;
        public float forestPercent;

    }
    
}