using System.Collections.Generic;

namespace ME.ECS {

    public partial interface IWorldBase {

        #region Regular Components
        TComponent AddOrGetComponent<TComponent>(Entity entity) where TComponent : class, IComponent, new();
        TComponent AddComponent<TComponent>(Entity entity) where TComponent : class, IComponent, new();
        TComponent AddComponent<TComponent>(Entity entity, TComponent data) where TComponent : class, IComponent;

        TComponent AddComponent<TComponent, TComponentType>(Entity entity) where TComponentType : class, IComponent
                                                                                    where TComponent : class, TComponentType, IComponent, new();
        TComponent GetComponent<TComponent>(Entity entity) where TComponent : class, IComponent;
        List<IComponent> ForEachComponent<TComponent>(Entity entity) where TComponent : class, IComponent;
        bool HasComponent<TComponent>(Entity entity) where TComponent : class, IComponent;
        void RemoveComponents<TComponent>(Entity entity) where TComponent : class, IComponent;
        void RemoveComponents(Entity entity);
        void RemoveComponents<TComponent>() where TComponent : class, IComponent;
        void RemoveComponentsPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponent where TComponentPredicate : IComponentPredicate<TComponent>;
        #endregion
        
        #region Shared Components
        TComponent AddOrGetComponentShared<TComponent>() where TComponent : class, IComponent, new();
        TComponent AddComponentShared<TComponent>() where TComponent : class, IComponent, new();
        TComponent AddComponentShared<TComponent>(TComponent data) where TComponent : class, IComponent;
        TComponent GetComponentShared<TComponent>() where TComponent : class, IComponent;
        List<IComponent> ForEachComponentShared<TComponent>(Entity entity) where TComponent : class, IComponent;
        bool HasComponentShared<TComponent>() where TComponent : class, IComponent;
        void RemoveComponentsShared<TComponent>() where TComponent : class, IComponent;
        void RemoveComponentsSharedPredicate<TComponent, TComponentPredicate>(TComponentPredicate predicate) where TComponent : class, IComponent where TComponentPredicate : IComponentPredicate<TComponent>;
        #endregion
        
    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World<TState> : IWorld<TState>, IPoolableSpawn, IPoolableRecycle where TState : class, IState<TState>, new() {

        private const int COMPONENTS_CAPACITY = 100;
        
        //private Dictionary<int, IComponents<TState>> componentsCache; // key = typeof(T:IData), value = list of T:Components
        private Components<TState> componentsCache;
        private Entity sharedEntity;
        private bool sharedEntityInitialized;
        
        partial void OnSpawnComponents() {

            this.componentsCache = PoolClass<Components<TState>>.Spawn(); //PoolDictionary<int, IComponents<TState>>.Spawn(World<TState>.COMPONENTS_CAPACITY);
            this.sharedEntity = default;
            this.sharedEntityInitialized = false;

        }

        partial void OnRecycleComponents() {
            
            PoolClass<Components<TState>>.Recycle(ref this.componentsCache);
            //PoolDictionary<int, IComponents<TState>>.Recycle(ref this.componentsCache);
            
        }
        
        #region Regular Components
        public TComponent AddOrGetComponent<TComponent>(Entity entity) where TComponent : class, IComponent, new() {

            var element = this.componentsCache.GetFirst<TComponent>(entity.id);
            if (element != null) return element;
            
            return this.AddComponent<TComponent>(entity);

        }

        /// <summary>
        /// Add component for current entity only (create component data)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TComponent>(Entity entity) where TComponent : class, IComponent, new() {

            TComponent data;
            data = PoolComponents.Spawn<TComponent>();

            return this.AddComponent<TComponent>(entity, data);

        }

        /// <summary>
        /// Add component for current entity only (create component data)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <typeparam name="TComponentType"></typeparam>
        public TComponent AddComponent<TComponent, TComponentType>(Entity entity) where TComponentType : class, IComponent where TComponent : class, TComponentType, IComponent, new() {

            TComponent data;
            data = PoolComponents.Spawn<TComponent>();

            return (TComponent)this.AddComponent<TComponentType>(entity, data);

        }

        /// <summary>
        /// Add component for entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TComponent>(Entity entity, TComponent data) where TComponent : class, IComponent {

            this.componentsCache.Add<TComponent>(entity.id, data);
            this.storagesCache.archetypes.Set<TComponent>(in entity);
            this.AddComponentToFilter(entity);
            
            return (TComponent)data;

        }

        public TComponent GetComponent<TComponent>(Entity entity) where TComponent : class, IComponent {

            return this.componentsCache.GetFirst<TComponent>(entity.id);

        }

        public List<IComponent> ForEachComponent<TComponent>(Entity entity) where TComponent : class, IComponent {

            return this.componentsCache.ForEach<TComponent>(entity.id);

        }

        /// <summary>
        /// Check is component exists on entity
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TComponent>(Entity entity) where TComponent : class, IComponent {

            return this.componentsCache.Contains<TComponent>(entity.id);
            
        }

        /// <summary>
        /// Remove all components with type from certain entity by predicate
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponentsPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponent where TComponentPredicate : IComponentPredicate<TComponent> {

            if (this.componentsCache.RemoveAllPredicate<TComponent, TComponentPredicate>(entity.id, predicate) > 0) {
                
                this.RemoveComponentFromFilter(entity);

            }

        }

        /// <summary>
        /// Remove all components from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents(Entity entity) {

            if (this.componentsCache.RemoveAll(entity.id) > 0) {
                
                this.storagesCache.archetypes.RemoveAll(in entity);
                this.RemoveComponentFromFilter(entity);

            }

        }

        /// <summary>
        /// Remove all components from certain entity by type
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents<TComponent>(Entity entity) where TComponent : class, IComponent {

            if (this.componentsCache.RemoveAll<TComponent>(entity.id) > 0) {
                
                this.storagesCache.archetypes.Remove<TComponent>(in entity);
                this.RemoveComponentFromFilter(entity);

            }

        }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// This method doesn't update any filter, you should call UpdateFilter manually
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponents<TComponent>() where TComponent : class, IComponent {

            if (this.componentsCache.RemoveAll<TComponent>() > 0) {
                
                this.storagesCache.archetypes.RemoveAll<TComponent>();

            }

        }
        #endregion

        #region Shared Components
        public TComponent AddOrGetComponentShared<TComponent>() where TComponent : class, IComponent, new() {

            return this.AddOrGetComponent<TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent AddComponentShared<TComponent>() where TComponent : class, IComponent, new() {

            return this.AddComponent<TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent AddComponentShared<TComponent>(TComponent data) where TComponent : class, IComponent {
            
            return this.AddComponent<TComponent>(this.sharedEntity, data);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent GetComponentShared<TComponent>() where TComponent : class, IComponent {

            return this.GetComponent<TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public List<IComponent> ForEachComponentShared<TComponent>(Entity entity) where TComponent : class, IComponent {
            
            return this.ForEachComponent<TComponent>(this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasComponentShared<TComponent>() where TComponent : class, IComponent {

            return this.HasComponent<TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsShared<TComponent>() where TComponent : class, IComponent {
            
            this.RemoveComponents<TComponent>(this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsSharedPredicate<TComponent, TComponentPredicate>(TComponentPredicate predicate) where TComponent : class, IComponent where TComponentPredicate : IComponentPredicate<TComponent> {
            
            this.RemoveComponentsPredicate<TComponent, TComponentPredicate>(this.sharedEntity, predicate);
            
        }
        #endregion
        
    }

}