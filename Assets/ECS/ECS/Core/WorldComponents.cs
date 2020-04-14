using System.Collections.Generic;

namespace ME.ECS {

    public partial interface IWorldBase {
        
        TEntity RunComponents<TEntity>(ref TEntity data, float deltaTime, int index) where TEntity : struct, IEntity;

    }

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState>, new() {

        #region Regular Components
        TComponent AddOrGetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity>, new() where TEntity : struct, IEntity;
        TComponent AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity>, new() where TEntity : struct, IEntity;
        TComponent AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;

        TComponent AddComponent<TEntity, TComponent, TComponentType>(Entity entity) where TComponentType : class, IComponent<TState, TEntity>
                                                                                    where TComponent : class, TComponentType, IComponent<TState, TEntity>, new()
                                                                                    where TEntity : struct, IEntity;
        TComponent GetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;
        List<IComponent<TState, TEntity>> ForEachComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;
        bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponentBase where TEntity : struct, IEntity;
        bool HasComponentOnce<TEntity, TComponent>(Entity entity) where TComponent : IComponentOnce<TState, TEntity> where TEntity : struct, IEntity;
        void RemoveComponents<TEntity, TComponent>(Entity entity) where TEntity : struct, IEntity where TComponent : class, IComponentBase;
        void RemoveComponentsByEntityType<TEntity>(Entity entity) where TEntity : struct, IEntity;
        void RemoveComponentsOnce<TEntity, TComponent>(Entity entity) where TEntity : struct, IEntity where TComponent : class, IComponentOnceBase;
        void RemoveComponents<TComponent>() where TComponent : class, IComponentBase;
        void RemoveComponentsOnce<TComponent>() where TComponent : class, IComponentOnceBase;
        void RemoveComponentsPredicate<TComponent, TComponentPredicate, TEntity>(Entity entity, TComponentPredicate predicate) where TEntity : struct, IEntity where TComponent : class, IComponent<TState, TEntity> where TComponentPredicate : IComponentPredicate<TComponent>;
        #endregion
        
        #region Shared Components
        TComponent AddOrGetComponentShared<TComponent>(Entity entity) where TComponent : class, IComponent<TState, SharedEntity>, new();
        TComponent AddComponentShared<TComponent>() where TComponent : class, IComponent<TState, SharedEntity>, new();
        TComponent AddComponentShared<TComponent>(IComponent<TState, SharedEntity> data) where TComponent : class, IComponent<TState, SharedEntity>;
        TComponent GetComponentShared<TComponent>() where TComponent : class, IComponent<TState, SharedEntity>;
        List<IComponent<TState, SharedEntity>> ForEachComponentShared<TComponent>(Entity entity) where TComponent : class, IComponent<TState, SharedEntity>;
        bool HasComponentShared<TComponent>() where TComponent : IComponent<TState, SharedEntity>;
        void RemoveComponentsShared<TComponent>() where TComponent : class, IComponentBase;
        void RemoveComponentsSharedPredicate<TComponent, TComponentPredicate>(TComponentPredicate predicate) where TComponent : class, IComponent<TState, SharedEntity> where TComponentPredicate : IComponentPredicate<TComponent>;
        #endregion
        
    }

    public struct SharedEntity : IEntity {

        public Entity entity { get; set; }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World<TState> : IWorld<TState>, IPoolableSpawn, IPoolableRecycle where TState : class, IState<TState>, new() {

        private const int COMPONENTS_CAPACITY = 100;
        
        //private Dictionary<int, IComponents<TState>> componentsCache; // key = typeof(T:IData), value = list of T:Components
        private IComponents<TState>[] componentsCache;
        private Entity sharedEntity;
        
        partial void OnSpawnComponents() {

            this.componentsCache = PoolArray<IComponents<TState>>.Spawn(World<TState>.COMPONENTS_CAPACITY); //PoolDictionary<int, IComponents<TState>>.Spawn(World<TState>.COMPONENTS_CAPACITY);

        }

        partial void OnRecycleComponents() {
            
            PoolArray<IComponents<TState>>.Recycle(ref this.componentsCache);
            //PoolDictionary<int, IComponents<TState>>.Recycle(ref this.componentsCache);
            
        }
        
        #region Regular Components
        public TEntity RunComponents<TEntity>(ref TEntity data, float deltaTime, int index) where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {

                var entityId = data.entity.id;
                var buckets = ((Components<TEntity, TState>)this.componentsCache[code]).GetAllBuckets();
                foreach (var bucket in buckets) {

                    if (bucket.components == null || entityId < 0 || entityId >= bucket.components.Length) continue;

                    var bucketComponents = bucket.components[entityId];
                    if (bucketComponents == null) continue;
                    
                    foreach (var component in bucketComponents) {

                        if (component is IRunnableComponent<TState, TEntity> runnable) runnable.AdvanceTick(this.currentState, ref data, deltaTime, index);

                    }

                }

            }

            return data;

        }

        public TComponent AddOrGetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity>, new() where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {

                ref var components = ref this.componentsCache[code];
                var element = ((Components<TEntity, TState>)components).GetFirst<TComponent>(entity.id);
                if (element != null) return element;

            }

            return this.AddComponent<TEntity, TComponent>(entity);

        }

        /// <summary>
        /// Add component for current entity only (create component data)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity>, new() where TEntity : struct, IEntity {

            TComponent data;
            data = PoolComponents.Spawn<TComponent>();

            return this.AddComponent<TEntity, TComponent>(entity, data);

        }

        /// <summary>
        /// Add component for current entity only (create component data)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <typeparam name="TComponentType"></typeparam>
        public TComponent AddComponent<TEntity, TComponent, TComponentType>(Entity entity) where TComponentType : class, IComponent<TState, TEntity> where TComponent : class, TComponentType, IComponent<TState, TEntity>, new() where TEntity : struct, IEntity {

            TComponent data;
            data = PoolComponents.Spawn<TComponent>();

            return (TComponent)this.AddComponent<TEntity, TComponentType>(entity, data);

        }

        /// <summary>
        /// Add component for entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {

                ((Components<TEntity, TState>)this.componentsCache[code]).Add<TComponent>(entity.id, data);

            } else {
                
                var components = PoolClass<Components<TEntity, TState>>.Spawn();
                components.Add<TComponent>(entity.id, data);

                ArrayUtils.Resize(code, ref this.componentsCache);
                this.componentsCache[code] = components;
                
            }

            this.AddComponentToFilter(entity);
            return (TComponent)data;

        }

        public TComponent GetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {

                return ((Components<TEntity, TState>)this.componentsCache[code]).GetFirst<TComponent>(entity.id);

            }

            return null;
            
        }

        public List<IComponent<TState, TEntity>> ForEachComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                return ((Components<TEntity, TState>)this.componentsCache[code]).ForEach<TComponent>(entity.id);
                
            }

            return null;

        }

        /// <summary>
        /// Check is component exists on entity
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponentBase where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                return ((Components<TEntity, TState>)this.componentsCache[code]).Contains<TComponent>(entity.id);
                
            }

            return false;

        }

        /// <summary>
        /// Check is component exists on entity
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponentOnce<TEntity, TComponent>(Entity entity) where TComponent : IComponentOnce<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                return ((Components<TEntity, TState>)this.componentsCache[code]).ContainsOnce<TComponent>(entity.id);
                
            }

            return false;

        }

        /// <summary>
        /// Remove all components from certain entity
        /// </summary>
        /// <param name="entity"></param>
        /*public void RemoveComponents(Entity entity) {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> componentsContainer;

            bool result;
            result = this.componentsCache.TryGetValue(code, out componentsContainer);
            if (result == true) {

                componentsContainer.RemoveAll(entity.id);
                
            }

        }*/

        /// <summary>
        /// Remove all components with type from certain entity by predicate
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponentsPredicate<TComponent, TComponentPredicate, TEntity>(Entity entity, TComponentPredicate predicate) where TEntity : struct, IEntity where TComponent : class, IComponent<TState, TEntity> where TComponentPredicate : IComponentPredicate<TComponent> {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                if (((Components<TEntity, TState>)this.componentsCache[code]).RemoveAllPredicate<TComponent, TComponentPredicate>(entity.id, predicate) > 0) {
                    
                    this.RemoveComponentFromFilter(entity);

                }

            }

        }

        /// <summary>
        /// Remove all components with type from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents<TEntity, TComponent>(Entity entity) where TEntity : struct, IEntity where TComponent : class, IComponentBase {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                if (this.componentsCache[code].RemoveAll<TComponent>(entity.id) > 0) {
                    
                    this.RemoveComponentFromFilter(entity);

                }

            }

        }

        /// <summary>
        /// Remove all components from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponentsByEntityType<TEntity>(Entity entity) where TEntity : struct, IEntity {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                if (this.componentsCache[code].RemoveAll(entity.id) > 0) {
                    
                    this.RemoveComponentFromFilter(entity);

                }

            }

        }

        /// <summary>
        /// Remove all components with type from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponentsOnce<TEntity, TComponent>(Entity entity) where TEntity : struct, IEntity where TComponent : class, IComponentOnceBase {

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            if (code >= 0 && code < this.componentsCache.Length) {
                
                if (this.componentsCache[code].RemoveAllOnce<TComponent>(entity.id) > 0) {
                    
                    this.RemoveComponentFromFilter(entity);

                }

            }

        }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// This method doesn't update any filter, you should call UpdateFilter manually
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponents<TComponent>() where TComponent : class, IComponentBase {

            for (var i = 0; i < this.componentsCache.Length; ++i) {
                
                var components = this.componentsCache[i];
                if (components != null) components.RemoveAll<TComponent>();

            }

        }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// This method doesn't update any filter, you should call UpdateFilter manually
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponentsOnce<TComponent>() where TComponent : class, IComponentOnceBase {

            for (var i = 0; i < this.componentsCache.Length; ++i) {
                
                var components = this.componentsCache[i];
                if (components != null) components.RemoveAllOnce<TComponent>();
                
            }

        }
        #endregion

        #region Shared Components
        public TComponent AddOrGetComponentShared<TComponent>(Entity entity) where TComponent : class, IComponent<TState, SharedEntity>, new() {

            return this.AddOrGetComponent<SharedEntity, TComponent>(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent AddComponentShared<TComponent>() where TComponent : class, IComponent<TState, SharedEntity>, new() {

            return this.AddComponent<SharedEntity, TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent AddComponentShared<TComponent>(IComponent<TState, SharedEntity> data) where TComponent : class, IComponent<TState, SharedEntity> {
            
            return this.AddComponent<SharedEntity, TComponent>(this.sharedEntity, data);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent GetComponentShared<TComponent>() where TComponent : class, IComponent<TState, SharedEntity> {

            return this.GetComponent<SharedEntity, TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public List<IComponent<TState, SharedEntity>> ForEachComponentShared<TComponent>(Entity entity) where TComponent : class, IComponent<TState, SharedEntity> {
            
            return this.ForEachComponent<SharedEntity, TComponent>(this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasComponentShared<TComponent>() where TComponent : IComponent<TState, SharedEntity> {

            return this.HasComponent<SharedEntity, TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsShared<TComponent>() where TComponent : class, IComponentBase {
            
            this.RemoveComponents<SharedEntity, TComponent>(this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsSharedPredicate<TComponent, TComponentPredicate>(TComponentPredicate predicate) where TComponent : class, IComponent<TState, SharedEntity> where TComponentPredicate : IComponentPredicate<TComponent> {
            
            this.RemoveComponentsPredicate<TComponent, TComponentPredicate, SharedEntity>(this.sharedEntity, predicate);
            
        }
        #endregion
        
    }

}