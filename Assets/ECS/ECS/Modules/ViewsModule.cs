#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    public interface IViewBase {
        
        Entity entity { get; set; }
        ViewId prefabSourceId { get; set; }

    }

    public interface IView<in T> : IViewBase where T : struct, IEntity {

        void OnInitialize(T data);
        void OnDeInitialize(T data);
        void ApplyState(T data, float deltaTime, bool immediately);
        
    }

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        void AddViewsProvider<TProvider>() where TProvider : class, IViewsProvider, new();
        void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        public void AddViewsProvider<TProvider>() where TProvider : class, IViewsProvider, new() {

            var list = PoolList<IViewModuleBase>.Spawn(10);
            this.GetModules(list);
            for (int i = 0, count = list.Count; i < count; ++i) {
                
                list[i].SetProvider(PoolClass<TProvider>.Spawn());
                
            }
            PoolList<IViewModuleBase>.Recycle(ref list);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void DestroyEntityPlugin1<TEntity>(Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.DestroyAllViews(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void RegisterPlugin1ModuleForEntity<TEntity>() where TEntity : struct, IEntity {

            if (this.HasModule<ViewsModule<TState, TEntity>>() == false) {

                if (this.AddModule<ViewsModule<TState, TEntity>>() == true) {

                    var module = this.GetModule<ViewsModule<TState, TEntity>>();
                    module.GetProvider().RegisterEntityType(module);

                }

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

    }

    public interface IViewModuleBase : IModuleBase {

        bool SetProvider(IViewsProvider provider);

    }

    public partial interface IViewModule<TState, TEntity> : IViewModuleBase, IModule<TState> where TState : class, IState<TState> where TEntity : struct, IEntity {

        IViewModule<TState, TEntity> RegisterProvider<TProvider>() where TProvider : class, IViewsProvider<TEntity>, new();
        IViewModule<TState, TEntity> UnRegisterProvider<TProvider>() where TProvider : class, IViewsProvider<TEntity>, new();

        void Register(IView<TEntity> instance);
        void UnRegister(IView<TEntity> instance);

        ViewId RegisterViewSource(IView<TEntity> prefab);
        bool UnRegisterViewSource(IView<TEntity> prefab);
        
        void InstantiateView(IView<TEntity> prefab, Entity entity);
        void InstantiateView(ViewId prefabSourceId, Entity entity);
        void DestroyView(ref IView<TEntity> instance);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class ViewsModule<TState, TEntity> : IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        private const int REGISTRY_CAPACITY = 100;
        private const int VIEWS_CAPACITY = 1000;
        private const int INTERNAL_ENTITIES_CACHE_CAPACITY = 100;
        private const int INTERNAL_ALL_ENTITIES_CACHE_CAPACITY = 1000;
        private const int INTERNAL_COMPONENTS_CACHE_CAPACITY = 10;
        
        /// <summary>
        /// Private predicate to filter components
        /// </summary>
        private struct RemoveComponentViewPredicate : IComponentPredicate<ViewComponent> {

            public EntityId entityId;
            public ViewId prefabSourceId;
        
            public bool Execute(ViewComponent data) {

                if (data.viewInfo.entity.id == this.entityId && data.viewInfo.prefabSourceId == this.prefabSourceId) {

                    return true;

                }

                return false;

            }
        
        }

        /// <summary>
        /// Private component class to describe Views
        /// </summary>
        private class ViewComponent : IComponent<TState, TEntity> {

            public ViewInfo viewInfo;
        
            public void AdvanceTick(TState state, ref TEntity data, float deltaTime, int index) {}

            void IComponent<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

                var otherView = (ViewComponent)other;
                this.viewInfo = otherView.viewInfo;
                
            }
    
        }

        private struct ViewInfo {

            public Entity entity;
            public ViewId prefabSourceId;
            
        }

        private List<IView<TEntity>> list;
        private Dictionary<IView<TEntity>, ViewId> registryPrefabToId;
        private Dictionary<ViewId, IView<TEntity>> registryIdToPrefab;
        private ViewId viewSourceIdRegistry;
        private ViewId viewIdRegistry;
        private IViewsProvider<TEntity> internalProvider;
        private IViewsProvider provider;
        
        public IWorld<TState> world { get; set; }

        void IModule<TState>.OnConstruct() {

            this.list = PoolList<IView<TEntity>>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.registryPrefabToId = PoolDictionary<IView<TEntity>, ViewId>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);
            this.registryIdToPrefab = PoolDictionary<ViewId, IView<TEntity>>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);

        }

        void IModule<TState>.OnDeconstruct() {
            
            PoolDictionary<ViewId, IView<TEntity>>.Recycle(ref this.registryIdToPrefab);
            PoolDictionary<IView<TEntity>, ViewId>.Recycle(ref this.registryPrefabToId);
            PoolList<IView<TEntity>>.Recycle(ref this.list);

        }

        public bool SetProvider(IViewsProvider provider) {

            if (this.provider == null) {
                
                this.provider = provider;
                return true;

            }
            
            return false;

        }

        public IViewsProvider GetProvider() {

            return this.provider;

        }

        public IViewModule<TState, TEntity> RegisterProvider<TProvider>() where TProvider : class, IViewsProvider<TEntity>, new() {

            this.internalProvider = PoolClass<TProvider>.Spawn();

            return this;

        }

        public IViewModule<TState, TEntity> UnRegisterProvider<TProvider>() where TProvider : class, IViewsProvider<TEntity>, new() {

            if (this.internalProvider != null) {

                var internalProvider = (TProvider)this.internalProvider;
                PoolClass<TProvider>.Recycle(ref internalProvider);
                this.internalProvider = null;

            }
            
            return this;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(IView<TEntity> prefab, Entity entity) {
            
            this.InstantiateView(this.GetViewSourceId(prefab), entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ViewId sourceId, Entity entity) {

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false && this.world.HasResetState() == true) {

                throw new OutOfStateException();

            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = entity;
            viewInfo.prefabSourceId = sourceId;

            var component = this.world.AddComponent<TEntity, ViewComponent>(entity);
            component.viewInfo = viewInfo;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref IView<TEntity> instance) {

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false && this.world.HasResetState() == true) {

                throw new OutOfStateException();

            }
            
            var predicate = new RemoveComponentViewPredicate();
            predicate.entityId = instance.entity.id;
            predicate.prefabSourceId = instance.prefabSourceId;
            
            this.world.RemoveComponentsPredicate<ViewComponent, RemoveComponentViewPredicate>(instance.entity, predicate);
            
            /*while (items.MoveNext() == true) {

                var item = items.Current;
                var viewInfo = item.component.viewInfo;
                if (viewInfo.entity.id == instance.entity.id && viewInfo.prefabSourceId == instance.prefabSourceId) {
                    
                    item.Remove();
                    
                }

            }*/

            instance = null;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private IView<TEntity> SpawnView_INTERNAL(ViewInfo viewInfo) {

            if (this.internalProvider == null) return null; 
            
            var instance = this.internalProvider.Spawn(this.GetViewSource(viewInfo.prefabSourceId), viewInfo.prefabSourceId);
            instance.entity = viewInfo.entity;
            instance.prefabSourceId = viewInfo.prefabSourceId;
            this.Register(instance);

            return instance;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RecycleView_INTERNAL(ref IView<TEntity> instance) {

            this.UnRegister(instance);
            if (this.internalProvider != null) this.internalProvider.Destroy(ref instance);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews(Entity entity) {
            
            for (int i = 0, count = this.list.Count; i < count; ++i) {

                var view = this.list[i];
                if (view.entity.id == entity.id) {

                    this.DestroyView(ref view);

                }

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IView<TEntity> GetViewSource(ViewId viewSourceId) {

            IView<TEntity> prefab;
            if (this.registryIdToPrefab.TryGetValue(viewSourceId, out prefab) == true) {

                return prefab;

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId GetViewSourceId(IView<TEntity> prefab) {

            ViewId viewId;
            if (this.registryPrefabToId.TryGetValue(prefab, out viewId) == true) {

                return viewId;

            }

            return 0UL;

        }

        public ViewId RegisterViewSource(IView<TEntity> prefab) {

            if (this.world.HasStep(WorldStep.LogicTick) == true) {

                throw new InStateException();

            }

            ViewId viewId;
            if (this.registryPrefabToId.TryGetValue(prefab, out viewId) == true) {

                return viewId;

            }

            ++this.viewSourceIdRegistry;
            this.registryPrefabToId.Add(prefab, this.viewSourceIdRegistry);
            this.registryIdToPrefab.Add(this.viewSourceIdRegistry, prefab);
            return this.viewSourceIdRegistry;

        }

        public bool UnRegisterViewSource(IView<TEntity> prefab) {

            if (this.world.HasStep(WorldStep.LogicTick) == true) {

                throw new InStateException();

            }

            ViewId viewId;
            if (this.registryPrefabToId.TryGetValue(prefab, out viewId) == true) {

                this.registryPrefabToId.Remove(prefab);
                return this.registryIdToPrefab.Remove(viewId);
                
            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Register(IView<TEntity> instance) {

            this.list.Add(instance);
            instance.OnInitialize(this.GetData(instance));

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegister(IView<TEntity> instance) {
            
            instance.OnDeInitialize(this.GetData(instance));
            this.list.Remove(instance);
            
        }

        private TEntity GetData(IViewBase view) {
            
            TEntity data;
            if (this.world.GetEntityData(view.entity.id, out data) == true) {

                return data;

            }

            return default;

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {}

        private bool IsRenderingNow(ref ViewInfo viewInfo) {
            
            // Iterate all current view instances
            for (int i = 0, count = this.list.Count; i < count; ++i) {

                var viewInstance = this.list[i];
                // Does view exists?
                if (viewInstance.entity.id == viewInfo.entity.id && viewInstance.prefabSourceId == viewInfo.prefabSourceId) {
                            
                    // View exists
                    return true;

                }

            }

            return false;

        }

        private void RestoreViews() {
            
            var entitiesList = PoolList<EntityId>.Spawn(ViewsModule<TState, TEntity>.INTERNAL_ENTITIES_CACHE_CAPACITY);
            var allEntities = PoolList<TEntity>.Spawn(ViewsModule<TState, TEntity>.INTERNAL_ALL_ENTITIES_CACHE_CAPACITY);
            this.world.ForEachEntity(allEntities);
            for (int j = 0, jCount = allEntities.Count; j < jCount; ++j) {
                
                // For each entity in state
                var item = allEntities[j];
                entitiesList.Add(item.entity.id);

                // For each view component
                var components = PoolList<ViewComponent>.Spawn(ViewsModule<TState, TEntity>.INTERNAL_COMPONENTS_CACHE_CAPACITY);
                this.world.ForEachComponent<TEntity, ViewComponent>(item.entity, components);
                for (int k = 0, kCount = components.Count; k < kCount; ++k) {

                    var component = components[k];
                    
                    var isRenderingNow = this.IsRenderingNow(ref component.viewInfo);
                    if (isRenderingNow == true) {
                        
                        // All is fine, we have rendering component view for entity
                        
                    } else {
                        
                        // We need to create component view for entity
                        var instance = this.SpawnView_INTERNAL(component.viewInfo);
                        // Call ApplyState with deltaTime = current time offset
                        instance.ApplyState(item, 0f, immediately: true);
                        
                    }

                }
                PoolList<ViewComponent>.Recycle(ref components);

            }
            
            // Iterate all current view instances
            // Search for views that doesn't represent any entity and destroy them
            for (int i = 0, count = this.list.Count; i < count; ++i) {
                
                var instance = this.list[i];
                if (entitiesList.Contains(instance.entity.id) == false) {

                    this.RecycleView_INTERNAL(ref instance);
                    --i;
                    --count;

                }

            }
            PoolList<EntityId>.Recycle(ref entitiesList);
            PoolList<TEntity>.Recycle(ref allEntities);

        }

        void IModule<TState>.Update(TState state, float deltaTime) {

            this.RestoreViews();
            
            for (int i = 0, count = this.list.Count; i < count; ++i) {
                
                var instance = this.list[i];
                instance.ApplyState(this.GetData(instance), deltaTime, immediately: false);
                
            }
            
        }
        
    }

}
#endif