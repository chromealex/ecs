
namespace ME.ECS {

    using ME.ECS.Collections;

    public class IsBitmask : System.Attribute {}

    public enum ComponentLifetime : byte {

        Infinite = 0,
        
        EndOfTick = 1,
        PlayNextTick = 2,
        
        EndOfFrame = 3,
        PlayNextFrame = 4,

    }

    public interface IStructComponent {

    }

    public interface IStructRegistryBase {

        IStructComponent GetObject(Entity entity);
        void SetObject(Entity entity, IStructComponent data);
        void RemoveObject(Entity entity);
        bool HasType(System.Type type);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class StructRegistryBase : IStructRegistryBase, IPoolableRecycle {
        
        public World world;
        
        public abstract bool HasType(System.Type type);
        public abstract IStructComponent GetObject(Entity entity);
        public abstract void SetObject(Entity entity, IStructComponent data);
        public abstract void RemoveObject(Entity entity);

        public abstract void UseLifetimeStep(World world, in byte step);
        
        public abstract void CopyFrom(StructRegistryBase other);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public abstract void Validate(in Entity entity);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public abstract bool Has(in Entity entity);
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public abstract bool Remove(in Entity entity, bool clearAll = false);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public abstract void OnRecycle();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class StructComponents<TComponent> : StructRegistryBase where TComponent : struct, IStructComponent {

        internal BufferArray<TComponent> components;
        internal BufferArray<byte> componentsStates;

        public override void OnRecycle() {
            
            PoolArray<TComponent>.Recycle(ref this.components);
            PoolArray<byte>.Recycle(ref this.componentsStates);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void UseLifetimeStep(World world, in byte step) {
            
            for (int i = 0; i < this.componentsStates.Length; ++i) {
                
                ref var state = ref this.componentsStates.arr[i];
                if (state == 0) continue;
                
                if (state - 1 == step) {
                    
                    state = 0;
                    
                    ref var entity = ref world.GetEntityById(in i);
                    this.components.arr[i] = default;
                    if (world.currentState.filters.HasInFilters<TComponent>() == true) world.currentState.storage.archetypes.Remove<TComponent>(in entity);
                    --world.currentState.structComponents.count;
                    world.RemoveComponentFromFilter(in entity);
                    
                }
                
            }
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void Validate(in Entity entity) {

            var index = entity.id;
            if (ArrayUtils.WillResize(in index, ref this.components) == true) {

                ArrayUtils.Resize(in index, ref this.components);
                ArrayUtils.Resize(in index, ref this.componentsStates);

            }

            this.world.currentState.storage.archetypes.Validate(in entity);
            
        }

        public override bool HasType(System.Type type) {

            return this.components.arr.GetType().GetElementType() == type;

        }
        
        public override IStructComponent GetObject(Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            var bucket = this.components.arr[index];
            var bucketState = this.componentsStates.arr[index];
            if (bucketState > 0) return bucket;

            return null;

        }

        public override void SetObject(Entity entity, IStructComponent data) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucket = ref this.components.arr[index];
            ref var bucketState = ref this.componentsStates.arr[index];
            bucket = (TComponent)data;
            if (bucketState == 0) {

                bucketState = 1;

                this.world.currentState.storage.archetypes.Set<TComponent>(in entity);
                
            }

        }

        public override void RemoveObject(Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates.arr[index];
            if (bucketState > 0) {
            
                ref var bucket = ref this.components.arr[index];
                bucket = default;
                bucketState = 0;
            
                this.world.currentState.storage.archetypes.Remove<TComponent>(in entity);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override bool Has(in Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            //var index = entity.id;
            //this.CheckResize(in index);
            return this.componentsStates.arr[entity.id] > 0;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent Get(in Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            //var index = entity.id;
            //this.CheckResize(in index);
            return ref this.components.arr[entity.id];
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Set(in Entity entity, in TComponent data) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates.arr[index];
            ref var bucket = ref this.components.arr[index];
            bucket = data;
            if (bucketState == 0) {

                bucketState = 1;

                this.world.currentState.storage.archetypes.Set<TComponent>(in entity);
                return true;

            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override bool Remove(in Entity entity, bool clearAll = false) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates.arr[index];
            if (bucketState > 0) {
            
                ref var bucket = ref this.components.arr[index];
                bucket = default;
                bucketState = 0;
            
                if (clearAll == true) {
                    
                    this.world.currentState.storage.archetypes.RemoveAll<TComponent>(in entity);
                    
                } else {

                    this.world.currentState.storage.archetypes.Remove<TComponent>(in entity);

                }

                return true;

            }

            return false;

        }

        public override void CopyFrom(StructRegistryBase other) {

            var _other = (StructComponents<TComponent>)other;
            ArrayUtils.Copy(in _other.components, ref this.components);
            ArrayUtils.Copy(in _other.componentsStates, ref this.componentsStates);

        }

    }

    public interface IStructComponentsContainer {

        BufferArray<StructRegistryBase> GetAllRegistries();

    }
    
    /*#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif*/
    public struct StructComponentsContainer : IStructComponentsContainer {

        internal BufferArray<StructRegistryBase> list;
        internal int count;
        private bool isCreated;

        public bool IsCreated() {

            return this.isCreated;

        }

        public void Initialize(bool freeze) {
            
            ArrayUtils.Resize(100, ref this.list);
            this.isCreated = true;

        }

        public BufferArray<StructRegistryBase> GetAllRegistries() {

            return this.list;

        }

        public int Count {
            get {
                return this.count;
            }
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void OnEntityCreate(in Entity entity) {

            // Update all known structs
            for (int i = 0, length = this.list.Length; i < length; ++i) {

                var item = this.list[i];
                if (item != null) item.Validate(in entity);

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void RemoveAll(in Entity entity) {
            
            for (int i = 0, length = this.list.Length; i < length; ++i) {

                var item = this.list[i];
                if (item != null) {

                    item.Remove(in entity, clearAll: true);

                }

            }
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void Validate<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(in code);
            var reg = (StructComponents<TComponent>)this.list[code];
            reg.Validate(in entity);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void Validate<TComponent>() where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(in code);
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Validate<TComponent>(in int code) where TComponent : struct, IStructComponent {

            if (ArrayUtils.WillResize(code, ref this.list) == true) {

                ArrayUtils.Resize(code, ref this.list);
                
            }
            
            if (this.list[code] == null) {

                var instance = (StructRegistryBase)PoolRegistries.Spawn(typeof(StructRegistryBase));
                if (instance == null) instance = PoolClass<StructComponents<TComponent>>.Spawn();//new StructComponents<TComponent>();
                instance.world = Worlds.currentWorld;
                this.list[code] = instance;

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasBit(in Entity entity, in int bit) {

            if (bit < 0 || bit >= this.list.Length) return false;
            var reg = this.list[bit];
            if (reg == null) return false;
            return reg.Has(in entity);
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Has<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TComponent>)this.list[code];
            return reg.Has(in entity);
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent Get<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TComponent>)this.list[code];
            return ref reg.Get(in entity);
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Set<TComponent>(in Entity entity, TComponent data) where TComponent : struct, IStructComponent {
            
            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TComponent>)this.list[code];
            if (reg.Set(in entity, data) == true) {
            
                ++this.count;
                return true;

            }

            return false;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Remove<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TComponent>)this.list[code];
            if (reg.Remove(in entity) == true) {

                --this.count;
                return true;

            }

            return false;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void OnRecycle() {
            
            for (int i = 0; i < this.list.Length; ++i) {
                
                if (this.list[i] != null) {
                    
                    this.list[i].OnRecycle();
                    PoolRegistries.Recycle(this.list[i]);
                    this.list[i] = null;

                }
                
            }
            
            PoolArray<StructRegistryBase>.Recycle(ref this.list);
            
            this.count = default;
            this.isCreated = default;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void CopyFrom(StructComponentsContainer other) {

            this.OnRecycle();
            
            this.count = other.count;
            this.isCreated = other.isCreated;
            
            this.list = PoolArray<StructRegistryBase>.Spawn(other.list.Length);
            for (int i = 0; i < other.list.Length; ++i) {

                if (other.list[i] != null) {

                    var type = other.list[i].GetType();
                    var comp = (StructRegistryBase)PoolRegistries.Spawn(type);
                    if (comp == null) {
                        
                        comp = (StructRegistryBase)System.Activator.CreateInstance(type);
                        
                    }

                    this.list[i] = comp;
                    this.list[i].CopyFrom(other.list[i]);

                }

            }

        }

    }

    public partial interface IWorldBase {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool HasDataBit(in Entity entity, in int bit);
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool HasData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        ref TComponent GetData<TComponent>(in Entity entity, bool createIfNotExists = true) where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void SetData<TComponent>(in Entity entity, in TComponent data) where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void SetData<TComponent>(in Entity entity, in TComponent data, bool updateFilters) where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void RemoveData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool HasSharedData<TComponent>() where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        ref TComponent GetSharedData<TComponent>() where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void SetSharedData<TComponent>(in TComponent data) where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void RemoveSharedData<TComponent>() where TComponent : struct, IStructComponent;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void ValidateData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent;

        ref StructComponentsContainer GetStructComponents();
        
        void Register(ref StructComponentsContainer componentsContainer, bool freeze, bool restore);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World {

        public ref StructComponentsContainer GetStructComponents() {

            return ref this.currentState.structComponents;

        }

        partial void OnSpawnStructComponents() {

        }

        partial void OnRecycleStructComponents() {

        }

        partial void CreateEntityPlugin1(Entity entity) {

            this.currentState.structComponents.OnEntityCreate(in entity);

        }

        partial void DestroyEntityPlugin1(Entity entity) {

            this.currentState.structComponents.RemoveAll(in entity);

        }

        public void Register(ref StructComponentsContainer componentsContainer, bool freeze, bool restore) {

            if (componentsContainer.IsCreated() == false) {

                componentsContainer = new StructComponentsContainer(); //PoolClass<StructComponentsContainer>.Spawn();
                componentsContainer.Initialize(freeze);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasSharedData<TComponent>() where TComponent : struct, IStructComponent {

            return this.HasData<TComponent>(in this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent GetSharedData<TComponent>() where TComponent : struct, IStructComponent {
            
            return ref this.GetData<TComponent>(in this.sharedEntity, createIfNotExists: true);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetSharedData<TComponent>(in TComponent data) where TComponent : struct, IStructComponent {
            
            this.SetData(in this.sharedEntity, data);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveSharedData<TComponent>() where TComponent : struct, IStructComponent {
            
            this.RemoveData<TComponent>(in this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasDataBit(in Entity entity, in int bit) {

            return this.currentState.structComponents.HasBit(in entity, in bit);

        }

        public void ValidateData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            this.currentState.structComponents.Validate<TComponent>(in entity);
            if (this.currentState.filters.HasInFilters<TComponent>() == true && this.HasData<TComponent>(in entity) == true) {
                
                this.currentState.storage.archetypes.Set<TComponent>(in entity);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            ref var r = ref this.currentState.structComponents.list.arr[WorldUtilities.GetComponentTypeId<TComponent>()];
            var reg = (StructComponents<TComponent>)r;
            return reg.Has(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent GetData<TComponent>(in Entity entity, bool createIfNotExists = true) where TComponent : struct, IStructComponent {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif

            // Inline all manually
            ref var r = ref this.currentState.structComponents.list.arr[WorldUtilities.GetComponentTypeId<TComponent>()];
            var reg = (StructComponents<TComponent>)r;
            ref var state = ref reg.componentsStates.arr[entity.id];
            if (state == 0 && createIfNotExists == true) {

                state = 1;
                if (this.currentState.filters.HasInFilters<TComponent>() == true) this.currentState.storage.archetypes.Set<TComponent>(in entity);
                ++this.currentState.structComponents.count;
                this.AddComponentToFilter(entity);
                
            }

            return ref reg.components.arr[entity.id];
            
        }

        /// <summary>
        /// Lifetime default is Infinite
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref byte SetData<TComponent>(in Entity entity, in TComponent data) where TComponent : struct, IStructComponent {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif
            
            // Inline all manually
            ref var r = ref this.currentState.structComponents.list.arr[WorldUtilities.GetComponentTypeId<TComponent>()];
            var reg = (StructComponents<TComponent>)r;
            reg.components.arr[entity.id] = data;
            ref var state = ref reg.componentsStates.arr[entity.id];
            if (state == 0) {

                state = 1;
                if (this.currentState.filters.HasInFilters<TComponent>() == true) this.currentState.storage.archetypes.Set<TComponent>(in entity);
                ++this.currentState.structComponents.count;
                this.AddComponentToFilter(entity);

            }

            return ref state;

        }

        private interface ITask {

            void Execute();
            void Recycle();

        }
        private class NextFrameTask<TComponent> : ITask where TComponent : struct, IStructComponent {

            public Entity entity;
            public TComponent data;
            public World world;
            public ComponentLifetime lifetime;
            
            public void Execute() {

                this.world.SetData(in this.entity, in this.data, this.lifetime);

            }

            public void Recycle() {

                this.world = null;
                this.data = default;
                this.entity = default;
                PoolClass<NextFrameTask<TComponent>>.Recycle(this);
                
            }

        }

        public void PlayTasksForFrame() {

            if (this.nextFrameTasks.Length > 0) {

                for (int i = 0; i < this.nextFrameTasks.Length; ++i) {

                    ref var task = ref this.nextFrameTasks[i];
                    task.Execute();
                    task.Recycle();

                }

                PoolArray<ITask>.Recycle(ref this.nextFrameTasks);

            }
            
        }

        public void PlayTasksForTick() {
            
            if (this.nextTickTasks.Length > 0) {

                for (int i = 0; i < this.nextTickTasks.Length; ++i) {

                    ref var task = ref this.nextTickTasks[i];
                    task.Execute();
                    task.Recycle();

                }

                PoolArray<ITask>.Recycle(ref this.nextTickTasks);

            }
            
        }
        
        private BufferArray<ITask> nextFrameTasks;
        private BufferArray<ITask> nextTickTasks;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetData<TComponent>(in Entity entity, in TComponent data, in ComponentLifetime lifetime) where TComponent : struct, IStructComponent {

            if (lifetime == ComponentLifetime.PlayNextFrame ||
                lifetime == ComponentLifetime.PlayNextTick) {

                var task = PoolClass<NextFrameTask<TComponent>>.Spawn();
                task.world = this;
                task.entity = entity;
                task.data = data;

                if (lifetime == ComponentLifetime.PlayNextFrame) {

                    task.lifetime = ComponentLifetime.EndOfFrame;
                    
                    var idx = this.nextFrameTasks.Length;
                    ArrayUtils.Resize(in idx, ref this.nextFrameTasks);
                    this.nextFrameTasks[idx] = task;

                } else if (lifetime == ComponentLifetime.PlayNextTick) {
                    
                    task.lifetime = ComponentLifetime.EndOfTick;

                    var idx = this.nextTickTasks.Length;
                    ArrayUtils.Resize(in idx, ref this.nextTickTasks);
                    this.nextTickTasks[idx] = task;

                }

            } else {

                ref var state = ref this.SetData(in entity, in data);

                if (lifetime == ComponentLifetime.Infinite) return;
                state = (byte)(lifetime + 1);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UseLifetimeStep(ComponentLifetime step) {

            var bStep = (byte)step;
            for (int i = 0; i < this.currentState.structComponents.list.Length; ++i) {

                ref var reg = ref this.currentState.structComponents.list.arr[i];
                if (reg == null) continue;
                
                reg.UseLifetimeStep(this, bStep);

            }
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {
                
                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif
            
            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {
                
                EmptyEntityException.Throw();
                
            }
            #endif
            
            var reg = (StructComponents<TComponent>)this.currentState.structComponents.list.arr[WorldUtilities.GetComponentTypeId<TComponent>()];
            ref var state = ref reg.componentsStates.arr[entity.id];
            if (state > 0) {
                
                state = 0;
                reg.components.arr[entity.id] = default;
                if (this.currentState.filters.HasInFilters<TComponent>() == true) this.currentState.storage.archetypes.Remove<TComponent>(in entity);
                --this.currentState.structComponents.count;
                this.RemoveComponentFromFilter(in entity);
                
            }
            
        }

    }

}