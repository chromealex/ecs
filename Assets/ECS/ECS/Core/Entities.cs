using EntityId = System.Int32;

namespace ME.ECS {
    
    [System.Serializable]
    public struct Entity {

        public EntityId id;
        public int typeId;

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public static Entity Create<TEntity>(in EntityId id) where TEntity : IEntity {

            var entity = new Entity();
            entity.id = id;
            entity.typeId = WorldUtilities.GetKey<TEntity>();
            return entity;

        }

    }

}