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

            return Entity.Create<TEntity>(id, noCheck: false);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        internal static Entity Create<TEntity>(in EntityId id, bool noCheck) where TEntity : IEntity {

            if (noCheck == false && id <= 0) {
                
                throw new System.ArgumentOutOfRangeException("id", "Couldn't create entity with negative value!");
                
            }

            var entity = new Entity();
            entity.id = id;
            entity.typeId = WorldUtilities.GetKey<TEntity>();
            return entity;

        }

        public override string ToString() {
            
            return "Entity Id: " + this.id.ToString() + " (type: " + this.typeId.ToString() + ")";
            
        }

    }

}