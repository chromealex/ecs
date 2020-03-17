namespace ME.ECS {

    public static class EntityTypesCounter {

        public static int counter = -1;

    }

    public static class EntityTypes<TEntity> where TEntity : IEntity {

        public static int typeId = -1;
        public static int capacity = 100;

    }

    [System.Serializable]
    public struct Entity : System.IEquatable<Entity> {

        public readonly int id;
        public int storageIdx;

        public static Entity Empty {
            get {
                return new Entity(0, 0);
            }
        }

        internal Entity(EntityId id, int storageIdx) {

            this.id = id;
            this.storageIdx = storageIdx;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        internal static Entity Create<TEntity>(in EntityId id) where TEntity : IEntity {

            return Entity.Create<TEntity>(id, noCheck: false);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        internal static Entity Create<TEntity>(in EntityId id, bool noCheck) where TEntity : IEntity {

            if (noCheck == false && id <= 0) {
                
                throw new System.ArgumentOutOfRangeException("id", "Couldn't create entity with negative or zero id!");
                
            }

            //var typeId = WorldUtilities.GetKey<TEntity>();
            var entity = new Entity(id, -1);
            return entity;

        }
        
        public static bool operator ==(Entity e1, Entity e2) {

            return e1.id == e2.id && e1.storageIdx == e2.storageIdx;

        }

        public static bool operator !=(Entity e1, Entity e2) {

            return !(e1 == e2);

        }

        public bool Equals(Entity other) {

            return this == other;

        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }

        public override int GetHashCode() {
            
            return this.id ^ this.storageIdx;
            
        }

        public override string ToString() {
            
            return "Entity Id: " + this.id.ToString() + " Storage Index: " + this.storageIdx.ToString();
            
        }

    }

}