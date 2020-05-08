using ME.ECS;

namespace Prototype.Entities {
    
    public struct Player : IEntity {
        
        public Entity entity { get; set; }

        public int actorId;

    }
    
}