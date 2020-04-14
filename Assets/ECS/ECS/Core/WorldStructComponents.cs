using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    using ME.ECS.Collections;
    
    public interface IStructComponent {

    }

    public interface IStructRegistry {

        void CopyFrom(IStructRegistry other);
        void OnRecycle();

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

        private const int COMPONENTS_CAPACITY = 10;
        
        private ME.ECS.Collections.StackArray10<TComponent>[] components;
        private bool[][] componentsStates;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int GetBucketId(EntityId entityId, out int index) {

            var bucketId = entityId / StructComponents<TComponent>.COMPONENTS_CAPACITY;
            index = entityId % StructComponents<TComponent>.COMPONENTS_CAPACITY;
            ArrayUtils.Resize(bucketId, ref this.components);
            ArrayUtils.Resize(bucketId, ref this.componentsStates);
            var bucket = this.components[bucketId];
            if (bucket.Length == 0) {
                
                this.components[bucketId] = new StackArray10<TComponent>(StructComponents<TComponent>.COMPONENTS_CAPACITY);
                this.componentsStates[bucketId] = PoolArray<bool>.Spawn(StructComponents<TComponent>.COMPONENTS_CAPACITY);

            }
            
            return bucketId;

        }

        public bool Has(Entity entity) {

            var bucketId = this.GetBucketId(entity.id, out var index);
            var bucket = this.componentsStates[bucketId];
            return bucket[index] == true;

        }

        public TComponent Get(Entity entity) {

            var bucketId = this.GetBucketId(entity.id, out var index);
            var bucket = this.components[bucketId];
            if (bucket.Length == 0) bucket = this.components[bucketId] = new StackArray10<TComponent>(StructComponents<TComponent>.COMPONENTS_CAPACITY);
            return bucket[index];

        }

        public void Set(Entity entity, TComponent data) {

            var bucketId = this.GetBucketId(entity.id, out var index);
            var bucket = this.components[bucketId];
            var bucketState = this.componentsStates[bucketId];
            bucket[index] = data;
            bucketState[index] = true;
            this.components[bucketId] = bucket;

        }

        public void Remove(Entity entity) {

            var bucketId = this.GetBucketId(entity.id, out var index);
            var bucket = this.components[bucketId];
            var bucketState = this.componentsStates[bucketId];
            bucket[index] = default;
            bucketState[index] = false;
            this.components[bucketId] = bucket;

        }

        public void CopyFrom(IStructRegistry other) {

            this.OnRecycle();

            var _other = (StructComponents<TComponent>)other;
            this.components = PoolArray<StackArray10<TComponent>>.Spawn(_other.components.Length);
            this.componentsStates = PoolArray<bool[]>.Spawn(_other.componentsStates.Length);

            for (var i = 0; i < _other.components.Length; ++i) {
                
                this.components[i] = _other.components[i];
                ArrayUtils.Copy(_other.componentsStates[i], ref this.componentsStates[i]);

            }

        }

        public void OnRecycle() {
            
            if (this.components != null) PoolArray<StackArray10<TComponent>>.Recycle(ref this.components);
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

        public void Initialize(bool freeze, bool restore) {
            
            ArrayUtils.Resize(100, ref this.list);
            
        }

        private void Validate<TComponent>(int code) where TComponent : struct, IStructComponent {

            ArrayUtils.Resize(code, ref this.list);
            if (this.list[code] == null) {

                var instance = (IStructRegistry)PoolComponents.Spawn(typeof(IStructRegistry));
                if (instance == null) instance = new StructComponents<TComponent>();
                this.Register<TComponent>(instance);

            }

        }

        public bool Has<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            return reg.Has(entity);
            
        }

        public TComponent Get<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            return reg.Get(entity);
            
        }

        public void Set<TComponent>(Entity entity, TComponent data) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            reg.Set(entity, data);

        }

        public void Remove<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            this.Validate<TComponent>(code);
            var reg = (IStructRegistry<TComponent>)this.list[code];
            reg.Remove(entity);

        }

        private void Register<TComponent>(IStructRegistry reg) where TComponent : struct, IStructComponent {

            var code = WorldUtilities.GetComponentTypeId<TComponent>();
            ArrayUtils.Resize(code, ref this.list);
            this.list[code] = reg;
            
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

        }

        public void CopyFrom(StructComponentsContainer other) {

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

    }

    public partial class World<TState> {

        private StructComponentsContainer componentsStructCache = new StructComponentsContainer();

        void IWorldBase.Register(ref StructComponentsContainer componentsContainer, bool freeze, bool restore) {

            if (componentsContainer == null) {

                componentsContainer = PoolClass<StructComponentsContainer>.Spawn();
                componentsContainer.Initialize(freeze, restore);

            }

            if (freeze == false) {

                this.componentsStructCache = componentsContainer;

            }

        }

        public bool HasData<TComponent>(Entity entity) where TComponent : struct, IStructComponent {

            return this.componentsStructCache.Has<TComponent>(entity);

        }

        public TComponent GetData<TComponent>(Entity entity) where TComponent : struct, IStructComponent {

            return this.componentsStructCache.Get<TComponent>(entity);

        }

        public void SetData<TComponent>(Entity entity, TComponent data) where TComponent : struct, IStructComponent {
            
            this.componentsStructCache.Set(entity, data);
            this.AddComponentToFilter(entity);
            
        }

        public void RemoveData<TComponent>(Entity entity) where TComponent : struct, IStructComponent {
            
            this.componentsStructCache.Remove<TComponent>(entity);
            this.RemoveComponentFromFilter(entity);
            
        }

    }

}