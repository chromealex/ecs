using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    using ME.ECS.Collections;
    
    public interface IStructComponent {

    }

    public interface IStructRegistry : IPoolableRecycle {
        
        void CopyFrom(IStructRegistry other);
        IStructComponent GetObject(Entity entity);
        void SetObject(Entity entity, IStructComponent data);

    }

    public interface IStructRegistry<TComponent> : IStructRegistry where TComponent : struct, IStructComponent {
        
        TComponent Get(Entity entity);
        bool Has(Entity entity);
        void Set(Entity entity, TComponent data);
        void Remove(Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class StructComponents<TComponent> : IStructRegistry<TComponent> where TComponent : struct, IStructComponent {

        private const int COMPONENTS_CAPACITY = 20;
        
        private TComponent[][] components;
        private bool[][] componentsStates;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int GetBucketId(in int entityId, out int index) {

            var bucketId = entityId / StructComponents<TComponent>.COMPONENTS_CAPACITY;
            index = entityId % StructComponents<TComponent>.COMPONENTS_CAPACITY;
            
            if (ArrayUtils.WillResize(bucketId, ref this.components) == true) {

                lock (this) {

                    ArrayUtils.Resize(bucketId, ref this.components);
                    ArrayUtils.Resize(bucketId, ref this.componentsStates);
                    
                }

            }
            
            var bucket = this.components[bucketId];
            if (bucket == null || bucket.Length == 0) {

                lock (this) {

                    this.components[bucketId] = PoolArray<TComponent>.Spawn(StructComponents<TComponent>.COMPONENTS_CAPACITY);
                    this.componentsStates[bucketId] = PoolArray<bool>.Spawn(StructComponents<TComponent>.COMPONENTS_CAPACITY);

                }

            }

            return bucketId;

        }

        public IStructComponent GetObject(Entity entity) {

            var bucketId = this.GetBucketId(in entity.id, out var index);
            var bucket = this.components[bucketId];
            var bucketState = this.componentsStates[bucketId];
            if (bucketState[index] == true) return bucket[index];

            return null;

        }

        public void SetObject(Entity entity, IStructComponent data) {

            var bucketId = this.GetBucketId(in entity.id, out var index);
            var bucket = this.components[bucketId];
            var bucketState = this.componentsStates[bucketId];
            bucket[index] = (TComponent)data;
            bucketState[index] = true;
            this.components[bucketId] = bucket;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {

            var bucketId = this.GetBucketId(in entity.id, out var index);
            var bucketState = this.componentsStates[bucketId];
            return bucketState[index] == true;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent Get(Entity entity) {

            var bucketId = this.GetBucketId(in entity.id, out var index);
            var bucket = this.components[bucketId];
            return bucket[index];

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Set(Entity entity, TComponent data) {

            var bucketId = this.GetBucketId(in entity.id, out var index);
            var bucket = this.components[bucketId];
            var bucketState = this.componentsStates[bucketId];
            bucket[index] = data;
            bucketState[index] = true;
            this.components[bucketId] = bucket;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Remove(Entity entity) {

            var bucketId = this.GetBucketId(in entity.id, out var index);
            var bucket = this.components[bucketId];
            var bucketState = this.componentsStates[bucketId];
            bucket[index] = default;
            bucketState[index] = false;
            this.components[bucketId] = bucket;
            this.componentsStates[bucketId] = bucketState;

        }

        public void CopyFrom(IStructRegistry other) {

            this.OnRecycle();

            var _other = (StructComponents<TComponent>)other;
            this.components = PoolArray<TComponent[]>.Spawn(_other.components.Length);
            this.componentsStates = PoolArray<bool[]>.Spawn(_other.componentsStates.Length);

            for (var i = 0; i < _other.components.Length; ++i) {
                
                ArrayUtils.Copy(_other.components[i], ref this.components[i]);
                ArrayUtils.Copy(_other.componentsStates[i], ref this.componentsStates[i]);

            }

        }

        public void OnRecycle() {
            
            if (this.components != null) PoolArray<TComponent[]>.Recycle(ref this.components);
            if (this.componentsStates != null) {

                foreach (var bucket in this.componentsStates) {
                    if (bucket != null) PoolArray<bool>.Recycle(bucket);
                }
                PoolArray<bool[]>.Recycle(ref this.componentsStates);

            }

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class StructComponentsContainer : IPoolableRecycle {

        private IStructRegistry[] list;
        private int count;

        public void Initialize(bool freeze, bool restore) {
            
            ArrayUtils.Resize(100, ref this.list);
            
        }

        public IStructRegistry[] GetAllRegistries() {

            return this.list;

        }

        public int Count {
            get {
                return this.count;
            }
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Validate<TComponent>(int code) where TComponent : struct, IStructComponent {

            if (ArrayUtils.WillResize(code, ref this.list) == true) {

                lock (this.list) ArrayUtils.Resize(code, ref this.list);
                
            }
            
            if (this.list[code] == null) {

                lock (this.list) {

                    var instance = (IStructRegistry)PoolComponents.Spawn(typeof(IStructRegistry));
                    if (instance == null) instance = new StructComponents<TComponent>();
                    this.list[code] = instance;

                }

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Has<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            return reg.Has(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent Get<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            return reg.Get(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Set<TComponent>(Entity entity, TComponent data) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            reg.Set(entity, data);
            ++this.count;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Remove<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            reg.Remove(entity);
            --this.count;

        }

        void IPoolableRecycle.OnRecycle() {
            
            if (this.list != null) {
                
                for (int i = 0; i < this.list.Length; ++i) {
                    
                    if (this.list[i] != null) {
                        
                        this.list[i].OnRecycle();
                        PoolComponents.Recycle(this.list[i]);
                        this.list[i] = null;

                    }
                    
                }
                
                PoolArray<IStructRegistry>.Recycle(ref this.list);
                
            }

            this.count = 0;

        }

        public void CopyFrom(StructComponentsContainer other) {

            this.count = other.count;
            
            if (this.list != null) {
                
                for (int i = 0; i < this.list.Length; ++i) {

                    if (this.list[i] != null) {
                        
                        this.list[i].OnRecycle();
                        PoolComponents.Recycle(this.list[i]);
                        this.list[i] = null;

                    }
                    
                }
                
                PoolArray<IStructRegistry>.Recycle(ref this.list);
                
            }
            
            this.list = PoolArray<IStructRegistry>.Spawn(other.list.Length);
            for (int i = 0; i < other.list.Length; ++i) {

                if (other.list[i] != null) {

                    var type = other.list[i].GetType();
                    var comp = (IStructRegistry)PoolComponents.Spawn(type);
                    if (comp == null) {
                        
                        comp = (IStructRegistry)System.Activator.CreateInstance(type);
                        
                    }

                    this.list[i] = comp;
                    this.list[i].CopyFrom(other.list[i]);

                }

            }

        }

    }

    public partial interface IWorldBase {

        void Register(ref StructComponentsContainer componentsContainer, bool freeze, bool restore);

        bool HasData<TComponent>(Entity entity) where TComponent : struct, IStructComponent;
        TComponent GetData<TComponent>(Entity entity) where TComponent : struct, IStructComponent;
        void SetData<TComponent>(Entity entity, TComponent data) where TComponent : struct, IStructComponent;
        void RemoveData<TComponent>(Entity entity) where TComponent : struct, IStructComponent;

        bool HasSharedData<TComponent>() where TComponent : struct, IStructComponent;
        TComponent GetSharedData<TComponent>() where TComponent : struct, IStructComponent;
        void SetSharedData<TComponent>(TComponent data) where TComponent : struct, IStructComponent;
        void RemoveSharedData<TComponent>() where TComponent : struct, IStructComponent;

    }

    public partial class World<TState> {

        private StructComponentsContainer componentsStructCache;

        partial void OnSpawnStructComponents() {

            this.componentsStructCache = PoolClass<StructComponentsContainer>.Spawn();

        }

        partial void OnRecycleStructComponents() {

            PoolClass<StructComponentsContainer>.Recycle(ref this.componentsStructCache);
            
        }

        void IWorldBase.Register(ref StructComponentsContainer componentsContainer, bool freeze, bool restore) {

            if (componentsContainer == null) {

                componentsContainer = PoolClass<StructComponentsContainer>.Spawn();
                componentsContainer.Initialize(freeze, restore);

            }

            if (freeze == false) {

                this.componentsStructCache = componentsContainer;

            }

        }

        public bool HasSharedData<TComponent>() where TComponent : struct, IStructComponent {

            return this.HasData<TComponent>(this.sharedEntity);

        }

        public TComponent GetSharedData<TComponent>() where TComponent : struct, IStructComponent {
            
            return this.GetData<TComponent>(this.sharedEntity);

        }

        public void SetSharedData<TComponent>(TComponent data) where TComponent : struct, IStructComponent {
            
            this.SetData(this.sharedEntity, data);

        }

        public void RemoveSharedData<TComponent>() where TComponent : struct, IStructComponent {
            
            this.RemoveData<TComponent>(this.sharedEntity);

        }

        public bool HasData<TComponent>(Entity entity) where TComponent : struct, IStructComponent {

            return this.componentsStructCache.Has<TComponent>(entity);

        }

        public TComponent GetData<TComponent>(Entity entity) where TComponent : struct, IStructComponent {

            return this.componentsStructCache.Get<TComponent>(entity);

        }

        public void SetData<TComponent>(Entity entity, TComponent data) where TComponent : struct, IStructComponent {
            
            this.componentsStructCache.Set(entity, data);
            lock (this.componentsStructCache) this.AddComponentToFilter(entity);
            
        }

        public void RemoveData<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            this.componentsStructCache.Remove<TComponent>(entity);
            lock (this.componentsStructCache) this.RemoveComponentFromFilter(entity);
            
        }

    }

}