using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    using ME.ECS.Collections;
    
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
    public abstract class StructRegistryBase<TState> : IStructRegistryBase, IPoolableRecycle where TState : class, IState<TState>, new() {
        
        public World<TState> world;
        
        public abstract bool HasType(System.Type type);
        public abstract IStructComponent GetObject(Entity entity);
        public abstract void SetObject(Entity entity, IStructComponent data);
        public abstract void RemoveObject(Entity entity);
        
        public abstract void CopyFrom(StructRegistryBase<TState> other);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public abstract void Validate(in Entity entity);
        
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
    public class StructComponents<TState, TComponent> : StructRegistryBase<TState> where TComponent : struct, IStructComponent where TState : class, IState<TState>, new() {

        private static object lockObject = new object();
        
        //private const int COMPONENTS_CAPACITY = 100;

        internal TComponent[] components;
        internal bool[] componentsStates;

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void Validate(in Entity entity) {

            var index = entity.id;
            if (ArrayUtils.WillResize(in index, ref this.components) == true) {

                ArrayUtils.Resize(in index, ref this.components);
                ArrayUtils.Resize(in index, ref this.componentsStates);

            }

        }

        /*[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int GetBucketId(in int entityId, out int index) {

            var bucketId = entityId / StructComponents<TComponent>.COMPONENTS_CAPACITY;
            index = entityId % StructComponents<TComponent>.COMPONENTS_CAPACITY;
            
            if (ArrayUtils.WillResize(bucketId, ref this.components) == true) {

                lock (this) {

                    ArrayUtils.Resize(bucketId, ref this.components);
                    ArrayUtils.Resize(bucketId, ref this.componentsStates);
                    
                }

            }
            
            ref var bucket = ref this.components[bucketId];
            if (bucket == null || bucket.Length == 0) {

                lock (this) {

                    this.components[bucketId] = PoolArray<TComponent>.Spawn(StructComponents<TComponent>.COMPONENTS_CAPACITY);
                    this.componentsStates[bucketId] = PoolArray<bool>.Spawn(StructComponents<TComponent>.COMPONENTS_CAPACITY);

                }

            }

            return bucketId;

        }*/

        public override bool HasType(System.Type type) {

            return this.components.GetType().GetElementType() == type;

        }
        
        public override IStructComponent GetObject(Entity entity) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            var bucket = this.components[index];
            var bucketState = this.componentsStates[index];
            if (bucketState == true) return bucket;

            return null;

        }

        public override void SetObject(Entity entity, IStructComponent data) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucket = ref this.components[index];
            ref var bucketState = ref this.componentsStates[index];
            bucket = (TComponent)data;
            if (bucketState == false) {

                bucketState = true;

                this.world.storagesCache.archetypes.Set<TComponent>(in entity);
                
            }

        }

        public override void RemoveObject(Entity entity) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates[index];
            if (bucketState == true) {
            
                ref var bucket = ref this.components[index];
                bucket = default;
                bucketState = false;
            
                this.world.storagesCache.archetypes.Remove<TComponent>(in entity);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Has(in Entity entity) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            //var index = entity.id;
            //this.CheckResize(in index);
            return this.componentsStates[entity.id];
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent Get(in Entity entity) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            //var index = entity.id;
            //this.CheckResize(in index);
            return ref this.components[entity.id];
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Set(in Entity entity, in TComponent data) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates[index];
            ref var bucket = ref this.components[index];
            bucket = data;
            if (bucketState == false) {

                bucketState = true;

                this.world.storagesCache.archetypes.Set<TComponent>(in entity);
                return true;

            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override bool Remove(in Entity entity, bool clearAll = false) {

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates[index];
            if (bucketState == true) {
            
                ref var bucket = ref this.components[index];
                bucket = default;
                bucketState = false;

                if (clearAll == true) {
                    
                    this.world.storagesCache.archetypes.RemoveAll<TComponent>(in entity);
                    
                } else {

                    this.world.storagesCache.archetypes.Remove<TComponent>(in entity);

                }

                return true;

            }

            return false;

        }

        public override void CopyFrom(StructRegistryBase<TState> other) {

            var _other = (StructComponents<TState, TComponent>)other;
            ArrayUtils.Copy(_other.components, ref this.components);
            ArrayUtils.Copy(_other.componentsStates, ref this.componentsStates);

        }

        public override void OnRecycle() {
            
            if (this.components != null) PoolArray<TComponent>.Recycle(ref this.components);
            if (this.componentsStates != null) {

                PoolArray<bool>.Recycle(ref this.componentsStates);

            }

        }

    }

    public interface IStructComponentsContainer {

        IStructRegistryBase[] GetAllRegistries();

    }
    
    /*#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif*/
    public struct StructComponentsContainer<TState> : IStructComponentsContainer where TState : class, IState<TState>, new() {

        internal StructRegistryBase<TState>[] list;
        internal int count;
        private bool isCreated;

        public bool IsCreated() {

            return this.isCreated;

        }

        public void Initialize(bool freeze) {
            
            ArrayUtils.Resize(100, ref this.list);
            this.isCreated = true;

        }

        public IStructRegistryBase[] GetAllRegistries() {

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
        public void RemoveAll(in Entity entity, bool clearAll = false) {
            
            for (int i = 0, length = this.list.Length; i < length; ++i) {

                var item = this.list[i];
                if (item != null) {

                    item.Remove(in entity, clearAll);

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
            var reg = (StructComponents<TState, TComponent>)this.list[code];
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

                lock (this.list) {
                    
                    ArrayUtils.Resize(code, ref this.list);
                    
                }

            }
            
            if (this.list[code] == null) {

                lock (this.list) {

                    var instance = (StructRegistryBase<TState>)PoolComponents.Spawn(typeof(StructRegistryBase<TState>));
                    if (instance == null) instance = PoolClass<StructComponents<TState, TComponent>>.Spawn();//new StructComponents<TComponent>();
                    instance.world = Worlds<TState>.currentWorld;
                    this.list[code] = instance;

                }

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Has<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TState, TComponent>)this.list[code];
            return reg.Has(in entity);
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent Get<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TState, TComponent>)this.list[code];
            return ref reg.Get(in entity);
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Set<TComponent>(in Entity entity, TComponent data) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TState, TComponent>)this.list[code];
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
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            //this.Validate<TComponent>(in code);
            var reg = (StructComponents<TState, TComponent>)this.list[code];
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
            
            if (this.list != null) {
                
                for (int i = 0; i < this.list.Length; ++i) {
                    
                    if (this.list[i] != null) {
                        
                        this.list[i].OnRecycle();
                        PoolRegistries.Recycle(this.list[i]);
                        this.list[i] = null;

                    }
                    
                }
                
                PoolArray<StructRegistryBase<TState>>.Recycle(ref this.list);
                
            }

            this.count = default;
            this.isCreated = default;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void CopyFrom(StructComponentsContainer<TState> other) {

            this.OnRecycle();
            
            this.count = other.count;
            this.isCreated = other.isCreated;
            
            this.list = PoolArray<StructRegistryBase<TState>>.Spawn(other.list.Length);
            for (int i = 0; i < other.list.Length; ++i) {

                if (other.list[i] != null) {

                    var type = other.list[i].GetType();
                    var comp = (StructRegistryBase<TState>)PoolRegistries.Spawn(type);
                    if (comp == null) {
                        
                        comp = (StructRegistryBase<TState>)System.Activator.CreateInstance(type);
                        
                    }

                    this.list[i] = comp;
                    this.list[i].CopyFrom(other.list[i]);

                }

            }

        }

    }

    public partial interface IWorldBase {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool HasData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        ref TComponent GetData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent;
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

    }

    public partial interface IWorld<TState> {
        
        ref StructComponentsContainer<TState> GetStructComponents();
        
        void Register(ref StructComponentsContainer<TState> componentsContainer, bool freeze, bool restore);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World<TState> {

        private StructComponentsContainer<TState> componentsStructCache;

        public ref StructComponentsContainer<TState> GetStructComponents() {

            return ref this.componentsStructCache;

        }

        partial void OnSpawnStructComponents() {

            this.componentsStructCache = new StructComponentsContainer<TState>();//PoolClass<StructComponentsContainer>.Spawn();

        }

        partial void OnRecycleStructComponents() {

            //PoolClass<StructComponentsContainer>.Recycle(ref this.componentsStructCache);
            this.componentsStructCache.OnRecycle();

        }

        partial void CreateEntityPlugin1(Entity entity) {

            this.componentsStructCache.OnEntityCreate(in entity);

        }

        partial void DestroyEntityPlugin1(Entity entity) {

            this.componentsStructCache.RemoveAll(in entity, clearAll: true);

        }

        void IWorld<TState>.Register(ref StructComponentsContainer<TState> componentsContainer, bool freeze, bool restore) {

            if (componentsContainer.IsCreated() == false) {

                componentsContainer = new StructComponentsContainer<TState>(); //PoolClass<StructComponentsContainer>.Spawn();
                componentsContainer.Initialize(freeze);

            }

            if (freeze == false) {

                this.componentsStructCache = componentsContainer;

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasSharedData<TComponent>() where TComponent : struct, IStructComponent {

            return this.HasData<TComponent>(in this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent GetSharedData<TComponent>() where TComponent : struct, IStructComponent {
            
            return ref this.GetData<TComponent>(in this.sharedEntity);

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
        public bool HasData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            return this.componentsStructCache.Has<TComponent>(in entity);

        }

        public void ValidateData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            this.componentsStructCache.Validate<TComponent>(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TComponent GetData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            // Inline all manually
            ref var r = ref this.componentsStructCache.list[WorldUtilities.GetComponentTypeId<TComponent>()];
            var reg = (StructComponents<TState, TComponent>)r;
            ref var state = ref reg.componentsStates[entity.id];
            if (state == false) {

                state = true;
                this.storagesCache.archetypes.Set<TComponent>(in entity);
                ++this.componentsStructCache.count;
                this.AddComponentToFilter(entity);
                
            }

            return ref reg.components[entity.id];
            
            //return ref this.componentsStructCache.Get<TComponent>(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetData<TComponent>(in Entity entity, in TComponent data) where TComponent : struct, IStructComponent {

            // Inline all manually
            ref var r = ref this.componentsStructCache.list[WorldUtilities.GetComponentTypeId<TComponent>()];
            var reg = (StructComponents<TState, TComponent>)r;
            reg.components[entity.id] = data;
            ref var state = ref reg.componentsStates[entity.id];
            if (state == false) {

                state = true;
                this.storagesCache.archetypes.Set<TComponent>(in entity);
                ++this.componentsStructCache.count;
                this.AddComponentToFilter(entity);

            }

            /*
            if (this.componentsStructCache.Set(in entity, data) == true) {
            
                this.AddComponentToFilter(entity);

            }*/
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetData<TComponent>(in Entity entity, in TComponent data, bool updateFilters) where TComponent : struct, IStructComponent {

            // Inline all manually
            var reg = (StructComponents<TState, TComponent>)this.componentsStructCache.list[WorldUtilities.GetComponentTypeId<TComponent>()];
            reg.components[entity.id] = data;
            ref var state = ref reg.componentsStates[entity.id];
            if (state == false) {

                state = true;
                this.storagesCache.archetypes.Set<TComponent>(in entity);
                ++this.componentsStructCache.count;
                if (updateFilters == true) this.AddComponentToFilter(entity);

            }
            
            /*
            if (this.componentsStructCache.Set(in entity, data) == true && updateFilters == true) {
            
                this.AddComponentToFilter(entity);

            }*/
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            var reg = (StructComponents<TState, TComponent>)this.componentsStructCache.list[WorldUtilities.GetComponentTypeId<TComponent>()];
            ref var state = ref reg.componentsStates[entity.id];
            if (state == true) {

                state = false;
                reg.components[entity.id] = default;
                this.storagesCache.archetypes.Remove<TComponent>(in entity);
                --this.componentsStructCache.count;
                this.RemoveComponentFromFilter(entity);
                
            }

            /*
            if (this.componentsStructCache.Remove<TComponent>(in entity) == true) {
            
                this.RemoveComponentFromFilter(entity);

            }*/
            
        }

    }

}