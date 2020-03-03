using ME.ECS;

namespace Warcraft.Entities {
    
    public struct SelectionEntity : IEntity {
        
        public Entity entity { get; set; }

        public Entity unitEntity;

    }
    
    public struct SelectionRectEntity : IEntity {
        
        public Entity entity { get; set; }

    }
    
}