using System.Collections.Generic;
using EntityId = System.Int32;

namespace ME.ECS {

    public partial interface IWorld<TState> : IWorldBase where TState : class, IState<TState> {

        #region Regular Components
        TComponent AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : struct, IEntity;
        TComponent AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : struct, IEntity;
        TComponent GetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;
        void ForEachComponent<TEntity, TComponent>(Entity entity, System.Collections.Generic.List<TComponent> output) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity;
        bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> where TEntity : struct, IEntity;
        void RemoveComponents(Entity entity);
        void RemoveComponents<TComponent>(Entity entity) where TComponent : class, IComponentBase;
        void RemoveComponents<TComponent>() where TComponent : class, IComponentBase;
        void RemoveComponentsPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent>;
        #endregion
        
        #region Shared Components
        TComponent AddComponentShared<TComponent>() where TComponent : class, IComponentBase, new();
        TComponent AddComponentShared<TComponent>(IComponent<TState, SharedEntity> data) where TComponent : class, IComponentBase;
        TComponent GetComponentShared<TComponent>() where TComponent : class, IComponent<TState, SharedEntity>;
        void ForEachComponentShared<TComponent>(System.Collections.Generic.List<TComponent> output) where TComponent : class, IComponent<TState, SharedEntity>;
        bool HasComponentShared<TComponent>() where TComponent : IComponent<TState, SharedEntity>;
        void RemoveComponentsShared();
        void RemoveComponentsShared<TComponent>() where TComponent : class, IComponentBase;
        void RemoveComponentsSharedPredicate<TComponent, TComponentPredicate>(TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent>;
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
        
        private Dictionary<int, IComponents<TState>> componentsCache; // key = typeof(T:IData), value = list of T:Components
        private Entity sharedEntity;
        
        partial void OnSpawnComponents() {
            
            this.componentsCache = PoolDictionary<int, IComponents<TState>>.Spawn(World<TState>.COMPONENTS_CAPACITY);

        }

        partial void OnRecycleComponents() {
            
            PoolDictionary<int, IComponents<TState>>.Recycle(ref this.componentsCache);
            
        }
        
        #region Regular Components
        /// <summary>
        /// Add component for current entity only (create component data)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : struct, IEntity {

            TComponent data;
            data = PoolComponents.Spawn<TComponent>();

            return this.AddComponent<TEntity, TComponent>(entity, (IComponent<TState, TEntity>)data);

        }

        /// <summary>
        /// Add component for entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> components;
            if (this.componentsCache.TryGetValue(code, out components) == true) {

                var item = (Components<TEntity, TState>)components;
                item.Add(entity, data);

            } else {

                components = PoolClass<Components<TEntity, TState>>.Spawn();
                ((Components<TEntity, TState>)components).Add(entity, data);
                this.componentsCache.Add(code, components);

            }

            return (TComponent)data;

        }

        public TComponent GetComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> components;
            var result = false;
            result = this.componentsCache.TryGetValue(code, out components);
            if (result == true) {

                return ((Components<TEntity, TState>)components).GetFirst<TComponent>(entity);

            }

            return null;
            
        }

        public void ForEachComponent<TEntity, TComponent>(Entity entity, List<TComponent> output) where TComponent : class, IComponent<TState, TEntity> where TEntity : struct, IEntity {

            output.Clear();
            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> components;
            var result = false;
            result = this.componentsCache.TryGetValue(code, out components);
            if (result == true) {

                ((Components<TEntity, TState>)components).ForEach(entity, output);

            }

        }

        /// <summary>
        /// Check is component exists on entity
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> components;
            if (this.componentsCache.TryGetValue(code, out components) == true) {

                return ((Components<TEntity, TState>)components).Contains<TComponent>(entity);

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

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> components;
            if (this.componentsCache.TryGetValue(code, out components) == true) {

                return ((Components<TEntity, TState>)components).ContainsOnce<TComponent>(entity);

            }

            return false;

        }

        /// <summary>
        /// Remove all components from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents(Entity entity) {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> componentsContainer;

            bool result;
            result = this.componentsCache.TryGetValue(code, out componentsContainer);
            if (result == true) {

                componentsContainer.RemoveAll(entity);

            }

        }

        /// <summary>
        /// Remove all components with type from certain entity by predicate
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponentsPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {

                componentsContainer.RemoveAllPredicate<TComponent, TComponentPredicate>(entity, predicate);

            }

        }

        /// <summary>
        /// Remove all components with type from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents<TComponent>(Entity entity) where TComponent : class, IComponentBase {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {

                componentsContainer.RemoveAll<TComponent>(entity);

            }

        }

        /// <summary>
        /// Remove all components with type from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponentsOnce<TComponent>(Entity entity) where TComponent : class, IComponentOnceBase {

            var code = WorldUtilities.GetKey(entity);
            IComponents<TState> componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {

                componentsContainer.RemoveAllOnce<TComponent>(entity);

            }

        }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponents<TComponent>() where TComponent : class, IComponentBase {

            foreach (var components in this.componentsCache) {

                components.Value.RemoveAll<TComponent>();

            }

        }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponentsOnce<TComponent>() where TComponent : class, IComponentOnceBase {

            foreach (var components in this.componentsCache) {

                components.Value.RemoveAllOnce<TComponent>();

            }

        }
        #endregion

        #region Shared Components
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent AddComponentShared<TComponent>() where TComponent : class, IComponentBase, new() {

            return this.AddComponent<SharedEntity, TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent AddComponentShared<TComponent>(IComponent<TState, SharedEntity> data) where TComponent : class, IComponentBase {
            
            return this.AddComponent<SharedEntity, TComponent>(this.sharedEntity, data);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TComponent GetComponentShared<TComponent>() where TComponent : class, IComponent<TState, SharedEntity> {

            return this.GetComponent<SharedEntity, TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void ForEachComponentShared<TComponent>(System.Collections.Generic.List<TComponent> output) where TComponent : class, IComponent<TState, SharedEntity> {
            
            this.ForEachComponent<SharedEntity, TComponent>(this.sharedEntity, output);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasComponentShared<TComponent>() where TComponent : IComponent<TState, SharedEntity> {

            return this.HasComponent<SharedEntity, TComponent>(this.sharedEntity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsShared() {
            
            this.RemoveComponents(this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsShared<TComponent>() where TComponent : class, IComponentBase {
            
            this.RemoveComponents<TComponent>(this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveComponentsSharedPredicate<TComponent, TComponentPredicate>(TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {
            
            this.RemoveComponentsPredicate<TComponent, TComponentPredicate>(this.sharedEntity, predicate);
            
        }
        #endregion
        
    }

}