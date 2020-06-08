namespace ME.ECS {

    public struct ComponentTypesCounter {

        public static int counter = -1;

    }

    public struct ComponentTypes<TComponent> where TComponent : IStructComponent {

        public static readonly byte _;
        public static int typeId = -1;

    }

    public static partial class EntityExtensions {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Entity RemoveData<TComponent>(this Entity entity) where TComponent : struct, IStructComponent {
            
            Worlds.currentWorld.RemoveData<TComponent>(entity);
            return entity;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool HasData<TComponent>(this Entity entity) where TComponent : struct, IStructComponent {
            
            return Worlds.currentWorld.HasData<TComponent>(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool HasDataBit(this Entity entity, in int bit) {
            
            return Worlds.currentWorld.HasDataBit(entity, bit);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ref TComponent GetData<TComponent>(this in Entity entity, bool createIfNotExists = true) where TComponent : struct, IStructComponent {
            
            return ref Worlds.currentWorld.GetData<TComponent>(in entity, createIfNotExists);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Entity SetData<TComponent>(this in Entity entity, in TComponent data) where TComponent : struct, IStructComponent {
            
            Worlds.currentWorld.SetData(in entity, in data);
            return entity;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Entity ValidateData<TComponent>(this in Entity entity) where TComponent : struct, IStructComponent {
            
            Worlds.currentWorld.ValidateData<TComponent>(in entity);
            return entity;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<TComponent>(this Entity entity)
            where TComponent : class, IComponent
        {
        
            return Worlds.currentWorld.HasComponent<TComponent>(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TComponent GetComponent<TComponent>(this Entity entity)
            where TComponent : class, IComponent
        {
        
            return Worlds.currentWorld.GetComponent<TComponent>(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TComponent AddComponent<TComponent>(this Entity entity)
            where TComponent : class, IComponent, new()
        {
        
            return Worlds.currentWorld.AddComponent<TComponent>(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TComponent AddOrGetComponent<TComponent>(this Entity entity)
            where TComponent : class, IComponent, new()
        {
        
            return Worlds.currentWorld.AddOrGetComponent<TComponent>(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponents<TComponent>(this Entity entity)
            where TComponent : class, IComponent
        {
        
            Worlds.currentWorld.RemoveComponents<TComponent>(entity);
            
        }

    }

    [System.Serializable]
    public readonly struct Entity : System.IEquatable<Entity>, System.IComparable<Entity> {

        public readonly int id;
        public readonly int version;
        
        public static Entity Empty {
            get {
                return new Entity(0, 0);
            }
        }

        public Entity(string name) {

            var entity = Worlds.currentWorld.AddEntity(name);
            this.id = entity.id;
            this.version = entity.version;

        }
        
        internal Entity(int id, int version) {

            this.id = id;
            this.version = version;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        internal static Entity Create(in int id) {

            return Entity.Create(id, noCheck: false);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        internal static Entity Create(in int id, bool noCheck) {

            if (noCheck == false && id <= 0) {
                
                throw new System.ArgumentOutOfRangeException("id", "Couldn't create entity with negative or zero id!");
                
            }

            //var typeId = WorldUtilities.GetKey<TEntity>();
            var entity = new Entity(id, -1);
            return entity;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Entity e1, Entity e2) {

            return e1.id == e2.id && e1.version == e2.version;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Entity e1, Entity e2) {

            return !(e1 == e2);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(Entity other) {

            return this == other;

        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Entity other) {

            return this.id.CompareTo(other.id);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            
            return this.id ^ this.version;
            
        }

        public override string ToString() {
            
            return "Entity Id: " + this.id.ToString() + " Version: " + this.version.ToString();
            
        }

    }

}