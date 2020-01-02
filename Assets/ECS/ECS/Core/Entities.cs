using EntityId = System.Int32;

namespace ME.ECS {
    
    public struct Entity {

        public EntityId id;
        public int typeId;

        public static Entity Create<TEntity>(EntityId id) where TEntity : IEntity {

            var entity = new Entity();
            entity.id = id;
            entity.typeId = WorldUtilities.GetKey<TEntity>();
            return entity;

        }

    }

}