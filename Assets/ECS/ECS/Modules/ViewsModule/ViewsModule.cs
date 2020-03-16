#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {
    
    using ME.ECS.Views;
    
    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {
        
        ViewId RegisterViewSource<TEntity, TProvider>(TProvider providerInitializer, IView<TEntity> prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProviderInitializer<TEntity>;
        bool UnRegisterViewSource<TEntity>(IView<TEntity> prefab) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(IView<TEntity> prefab, Entity entity) where TEntity : struct, IEntity;
        void DestroyView<TEntity>(ref IView<TEntity> instance) where TEntity : struct, IEntity;
        void DestroyAllViews<TEntity>(Entity entity) where TEntity : struct, IEntity;

        ViewId RegisterViewSourceShared<TProvider>(TProvider providerInitializer, IView<SharedEntity> prefab) where TProvider : struct, IViewsProviderInitializer<SharedEntity>;
        bool UnRegisterViewSourceShared(IView<SharedEntity> prefab);
        void InstantiateViewShared(ViewId prefab);
        void InstantiateViewShared(IView<SharedEntity> prefab);
        void DestroyViewShared(ref IView<SharedEntity> instance);
        void DestroyAllViewsShared();

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void DestroyEntityPlugin1<TEntity>(Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.DestroyAllViews(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void RegisterPlugin1ModuleForEntity<TEntity>() where TEntity : struct, IEntity {

            if (this.HasModule<ViewsModule<TState, TEntity>>() == false) {

                this.AddModule<ViewsModule<TState, TEntity>>();

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSource<TEntity>(IView<TEntity> prefab) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity, TProvider>(TProvider providerInitializer, IView<TEntity> prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProviderInitializer<TEntity> {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.RegisterViewSource(providerInitializer, prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(IView<TEntity> prefab, Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView<TEntity>(ref IView<TEntity> instance) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.DestroyView(ref instance);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.DestroyAllViews(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSourceShared(IView<SharedEntity> prefab) {

            return this.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSourceShared<TProvider>(TProvider providerInitializer, IView<SharedEntity> prefab) where TProvider : struct, IViewsProviderInitializer<SharedEntity> {

            return this.RegisterViewSource<SharedEntity, TProvider>(providerInitializer, prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateViewShared(IView<SharedEntity> prefab) {

            this.InstantiateView(prefab, this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateViewShared(ViewId prefab) {

            this.InstantiateView<SharedEntity>(prefab, this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyViewShared(ref IView<SharedEntity> instance) {
            
            this.DestroyView(ref instance);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViewsShared() {
            
            this.DestroyAllViews<SharedEntity>(this.sharedEntity);
            
        }

    }

}

namespace ME.ECS.Views {
    
    using ME.ECS.Collections;
    
    public interface IViewBase {
        
        Entity entity { get; set; }
        ViewId prefabSourceId { get; set; }
        Tick creationTick { get; set; }

    }

    public interface IView<T> : IViewBase where T : struct, IEntity {

        void OnInitialize(in T data);
        void OnDeInitialize(in T data);
        void ApplyState(in T data, float deltaTime, bool immediately);
        
        void UpdateParticlesSimulation(float deltaTime);
        void SimulateParticles(float time, uint seed);

    }

    public interface IViewModuleBase : IModuleBase {

        System.Collections.IDictionary GetData();
        System.Collections.IDictionary GetViewSourceData();
        IViewsProviderBase GetViewSourceProvider(ViewId viewSourceId);

    }

    public partial interface IViewModule<TState, TEntity> : IViewModuleBase, IModule<TState> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        void Register(IView<TEntity> instance);
        bool UnRegister(IView<TEntity> instance);

        ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView<TEntity> prefab) where TProvider : struct, IViewsProviderInitializer<TEntity>;
        bool UnRegisterViewSource(IView<TEntity> prefab);
        
        void InstantiateView(IView<TEntity> prefab, Entity entity);
        void InstantiateView(ViewId prefabSourceId, Entity entity);
        void DestroyView(ref IView<TEntity> instance);

    }

    public class ViewRegistryNotFoundException : System.Exception {

        public ViewRegistryNotFoundException(ViewId sourceViewId) : base("[Views] View with id " + sourceViewId.ToString() + " not found in registry. Have you called RegisterViewSource()?") {}

    }

    public interface IViewComponent {

        ref ViewInfo GetViewInfo();

    }

    public struct ViewInfo : System.IEquatable<ViewInfo> {

        public Entity entity;
        public ViewId prefabSourceId;
        public Tick creationTick;

        public override int GetHashCode() {
                
            return this.entity.id ^ (int)this.prefabSourceId ^ (int)this.creationTick;
                
        }
            
        public override bool Equals(object obj) {
                
            return this.Equals((ViewInfo)obj);
                
        }

        public bool Equals(ViewInfo p) {
                
            return this.creationTick == p.creationTick && this.entity.id == p.entity.id && this.prefabSourceId == p.prefabSourceId;
                
        }

        public override string ToString() {
                
            return this.entity.ToString() + "\nPrefab Source Id: " + this.prefabSourceId.ToString() + "\nCreation Tick: " + this.creationTick.ToString();
                
        }

    }

    /// <summary>
    /// Private component class to describe Views
    /// </summary>
    public class ViewComponent<TState, TEntity> : IComponentCopyable<TState, TEntity>, IViewComponent where TState : IStateBase where TEntity : struct, IEntity {

        public ViewInfo viewInfo;
        public uint seed;

        public ref ViewInfo GetViewInfo() {

            return ref this.viewInfo;

        }

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.seed = default;

        }

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {

            var otherView = (ViewComponent<TState, TEntity>)other;
            this.viewInfo = otherView.viewInfo;
            this.seed = otherView.seed;

        }

    }

    public interface IViewComponentRequest<TState, TEntity> : IComponentCopyable<TState, TEntity> where TState : IStateBase where TEntity : struct, IEntity {}

    public class DestroyViewComponentRequest<TState, TEntity> : IViewComponentRequest<TState, TEntity> where TState : IStateBase where TEntity : struct, IEntity {

        public ViewInfo viewInfo;
        public IView<TEntity> viewInstance;

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.viewInstance = default;

        }

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var otherView = (DestroyViewComponentRequest<TState, TEntity>)other;
            this.viewInfo = otherView.viewInfo;
            this.viewInstance = otherView.viewInstance;

        }

    }

    public class CreateViewComponentRequest<TState, TEntity> : IViewComponentRequest<TState, TEntity> where TState : IStateBase where TEntity : struct, IEntity {

        public ViewInfo viewInfo;
        public uint seed;

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.seed = default;

        }

        void IComponentCopyable<TState, TEntity>.CopyFrom(IComponent<TState, TEntity> other) {
            
            var otherView = (CreateViewComponentRequest<TState, TEntity>)other;
            this.viewInfo = otherView.viewInfo;
            this.seed = otherView.seed;

        }

    }

    /// <summary>
    /// Private predicate to filter components
    /// </summary>
    public struct RemoveComponentViewPredicate<TState, TEntity> : IComponentPredicate<ViewComponent<TState, TEntity>> where TState : IStateBase where TEntity : struct, IEntity {

        public EntityId entityId;
        public ViewId prefabSourceId;
        public Tick creationTick;
        
        public bool Execute(ViewComponent<TState, TEntity> data) {

            if (data.viewInfo.creationTick == this.creationTick && data.viewInfo.entity.id == this.entityId && data.viewInfo.prefabSourceId == this.prefabSourceId) {

                return true;

            }

            return false;

        }
        
    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class ViewsModule<TState, TEntity> : IViewModule<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        private const int REGISTRY_PROVIDERS_CAPACITY = 100;
        private const int REGISTRY_CAPACITY = 100;
        private const int VIEWS_CAPACITY = 1000;
        private const int INTERNAL_ENTITIES_CACHE_CAPACITY = 100;
        private const int INTERNAL_COMPONENTS_CACHE_CAPACITY = 10;
        
        private Dictionary<EntityId, List<IView<TEntity>>> list;
        private HashSet<ViewInfo> rendering;
        private Dictionary<ViewId, IViewsProviderInitializerBase> registryPrefabToProviderBase;
        private Dictionary<ViewId, IViewsProvider<TEntity>> registryPrefabToProvider;
        private Dictionary<IView<TEntity>, ViewId> registryPrefabToId;
        private Dictionary<ViewId, IView<TEntity>> registryIdToPrefab;
        private ViewId viewSourceIdRegistry;
        private ViewId viewIdRegistry;
        
        public IWorld<TState> world { get; set; }

        void IModuleBase.OnConstruct() {

            this.list = PoolDictionary<EntityId, List<IView<TEntity>>>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.rendering = PoolHashSet<ViewInfo>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.registryPrefabToId = PoolDictionary<IView<TEntity>, ViewId>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);
            this.registryIdToPrefab = PoolDictionary<ViewId, IView<TEntity>>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);

            this.registryPrefabToProvider = PoolDictionary<ViewId, IViewsProvider<TEntity>>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_PROVIDERS_CAPACITY);
            this.registryPrefabToProviderBase = PoolDictionary<ViewId, IViewsProviderInitializerBase>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_PROVIDERS_CAPACITY);

        }

        void IModuleBase.OnDeconstruct() {
            
            PoolDictionary<ViewId, IViewsProvider<TEntity>>.Recycle(ref this.registryPrefabToProvider);
            PoolDictionary<ViewId, IViewsProviderInitializerBase>.Recycle(ref this.registryPrefabToProviderBase);
            
            PoolDictionary<ViewId, IView<TEntity>>.Recycle(ref this.registryIdToPrefab);
            PoolDictionary<IView<TEntity>, ViewId>.Recycle(ref this.registryPrefabToId);
            
            PoolHashSet<ViewInfo>.Recycle(ref this.rendering);

            foreach (var item in this.list) {
                
                PoolList<IView<TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, List<IView<TEntity>>>.Recycle(ref this.list);

        }

        System.Collections.IDictionary IViewModuleBase.GetData() {

            return this.list;

        }

        System.Collections.IDictionary IViewModuleBase.GetViewSourceData() {

            return this.registryIdToPrefab;

        }

        IViewsProviderBase IViewModuleBase.GetViewSourceProvider(ViewId viewSourceId) {

            IViewsProvider<TEntity> provider;
            if (this.registryPrefabToProvider.TryGetValue(viewSourceId, out provider) == true) {

                return provider;

            }

            return null;

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

            if (this.registryIdToPrefab.ContainsKey(sourceId) == false) {

                throw new ViewRegistryNotFoundException(sourceId);

            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = entity;
            viewInfo.prefabSourceId = sourceId;
            viewInfo.creationTick = this.world.GetStateTick();

            var component = this.world.AddComponent<TEntity, ViewComponent<TState, TEntity>>(entity);
            component.viewInfo = viewInfo;
            component.seed = (uint)this.world.GetSeedValue();
            
            var request = this.world.AddComponent<TEntity, CreateViewComponentRequest<TState, TEntity>, IViewComponentRequest<TState, TEntity>>(entity);
            request.viewInfo = viewInfo;
            request.seed = component.seed;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref IView<TEntity> instance) {

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false && this.world.HasResetState() == true) {

                throw new OutOfStateException();

            }
            
            var predicate = new RemoveComponentViewPredicate<TState, TEntity>();
            predicate.entityId = instance.entity.id;
            predicate.prefabSourceId = instance.prefabSourceId;
            predicate.creationTick = instance.creationTick;
            
            this.world.RemoveComponentsPredicate<ViewComponent<TState, TEntity>, RemoveComponentViewPredicate<TState, TEntity>, TEntity>(instance.entity, predicate);

            var viewInfo = new ViewInfo();
            viewInfo.entity = instance.entity;
            viewInfo.prefabSourceId = instance.prefabSourceId;
            viewInfo.creationTick = instance.creationTick;
            
            var request = this.world.AddComponent<TEntity, DestroyViewComponentRequest<TState, TEntity>, IViewComponentRequest<TState, TEntity>>(instance.entity);
            request.viewInfo = viewInfo;
            request.viewInstance = instance;
            
            instance = null;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private IView<TEntity> SpawnView_INTERNAL(ViewInfo viewInfo) {

            IViewsProvider<TEntity> provider;
            if (this.registryPrefabToProvider.TryGetValue(viewInfo.prefabSourceId, out provider) == true) {

                var instance = provider.Spawn(this.GetViewSource(viewInfo.prefabSourceId), viewInfo.prefabSourceId);
                instance.entity = viewInfo.entity;
                instance.prefabSourceId = viewInfo.prefabSourceId;
                instance.creationTick = viewInfo.creationTick;
                this.Register(instance);

                return instance;
                
            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RecycleView_INTERNAL(ref IView<TEntity> instance) {

            this.UnRegister(instance);

            IViewsProvider<TEntity> provider;
            if (this.registryPrefabToProvider.TryGetValue(instance.prefabSourceId, out provider) == true) {

                provider.Destroy(ref instance);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews(Entity entity) {

            List<IView<TEntity>> list;
            if (this.list.TryGetValue(entity.id, out list) == true) {

                for (int i = 0, count = list.Count; i < count; ++i) {
                    
                    var view = list[i];
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

        public ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView<TEntity> prefab) where TProvider : struct, IViewsProviderInitializer<TEntity> {

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
            var viewsProvider = (IViewsProviderInitializer<TEntity>)providerInitializer;
            var provider = viewsProvider.Create();
            provider.OnConstruct();
            this.registryPrefabToProvider.Add(this.viewSourceIdRegistry, provider);
            this.registryPrefabToProviderBase.Add(this.viewSourceIdRegistry, viewsProvider);

            return this.viewSourceIdRegistry;

        }

        public bool UnRegisterViewSource(IView<TEntity> prefab) {

            if (this.world.HasStep(WorldStep.LogicTick) == true) {

                throw new InStateException();

            }

            ViewId viewId;
            if (this.registryPrefabToId.TryGetValue(prefab, out viewId) == true) {

                var provider = this.registryPrefabToProvider[viewId];
                provider.OnDeconstruct();
                ((IViewsProviderInitializer<TEntity>)this.registryPrefabToProviderBase[viewId]).Destroy(provider);
                this.registryPrefabToProviderBase.Remove(viewId);
                this.registryPrefabToProvider.Remove(viewId);
                this.registryPrefabToId.Remove(prefab);
                return this.registryIdToPrefab.Remove(viewId);
                
            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Register(IView<TEntity> instance) {

            List<IView<TEntity>> list;
            if (this.list.TryGetValue(instance.entity.id, out list) == true) {
                
                list.Add(instance);
                
            } else {
                
                list = PoolList<IView<TEntity>>.Spawn(100);
                list.Add(instance);
                this.list.Add(instance.entity.id, list);
                
            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = instance.entity;
            viewInfo.prefabSourceId = instance.prefabSourceId;
            viewInfo.creationTick = instance.creationTick;
            this.rendering.Add(viewInfo);

            instance.OnInitialize(this.GetData(instance));

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegister(IView<TEntity> instance) {
            
            return this.UnRegister_INTERNAL(instance, removeFromList: true);
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool UnRegister_INTERNAL(IView<TEntity> instance, bool removeFromList = true) {
            
            instance.OnDeInitialize(this.GetData(instance));

            if (removeFromList == true) {

                List<IView<TEntity>> list;
                if (this.list.TryGetValue(instance.entity.id, out list) == true) {

                    list.Remove(instance);

                }

            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = instance.entity;
            viewInfo.prefabSourceId = instance.prefabSourceId;
            viewInfo.creationTick = instance.creationTick;
            return this.rendering.Remove(viewInfo);

        }

        private TEntity GetData(IViewBase view) {
            
            TEntity data;
            if (this.world.GetEntityData(view.entity, out data) == true) {

                return data;

            }

            return default;

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {}

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool IsRenderingNow(in ViewInfo viewInfo) {

            return this.rendering.Contains(viewInfo);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void CreateVisualInstance(in TEntity data, in uint seed, in ViewInfo viewInfo) {
            
            var instance = this.SpawnView_INTERNAL(viewInfo);
            // Call ApplyState with deltaTime = current time offset
            var dt = UnityEngine.Mathf.Max(0f, (this.world.GetCurrentTick() - viewInfo.creationTick) * this.world.GetTickTime());
            instance.ApplyState(in data, dt, immediately: true);
            // Simulate particle systems
            instance.SimulateParticles(dt, seed);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateRequests() {
            
            var aliveEntities = PoolHashSet<EntityId>.Spawn(ViewsModule<TState, TEntity>.INTERNAL_ENTITIES_CACHE_CAPACITY);
            RefList<TEntity> allEntities;
            if (this.world.ForEachEntity(out allEntities) == true) {

                for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {

                    // For each entity in state
                    ref var item = ref allEntities[j];
                    if (allEntities.IsFree(j) == true) continue;

                    aliveEntities.Add(item.entity.id);

                    // Get all requests
                    /*var requests = this.world.ForEachComponent<TEntity, IViewComponentRequest<TState, TEntity>>(item.entity);
                    if (requests != null) {

                        foreach (var request in requests) {

                            if (request is CreateViewComponentRequest<TState, TEntity> createComponentRequest) {

                                this.CreateVisualInstance(in item, in createComponentRequest.seed, in createComponentRequest.viewInfo);
                                
                            } else if (request is DestroyViewComponentRequest<TState, TEntity> destroyComponentRequest) {

                                if (this.list.TryGetValue(item.entity.id, out var viewsList) == true) {

                                    viewsList.Remove(destroyComponentRequest.viewInstance);

                                }

                                this.RecycleView_INTERNAL(ref destroyComponentRequest.viewInstance);

                            }

                        }

                    }
                    this.world.RemoveComponents<TEntity, IViewComponentRequest<TState, TEntity>>(item.entity);*/

                    // Get all views
                    var allViews = this.world.ForEachComponent<TEntity, ViewComponent<TState, TEntity>>(item.entity);
                    if (allViews != null) {
                        
                        // Comparing current state views to current rendering
                        foreach (var viewComponent in allViews) {

                            var view = (ViewComponent<TState, TEntity>)viewComponent;
                            if (this.IsRenderingNow(in view.viewInfo) == false) {
                                
                                // current data doesn't represent any visual instance
                                this.CreateVisualInstance(in item, in view.seed, in view.viewInfo);

                            }

                        }

                    }

                }

            }
            
            // Iterate all current view instances
            // Search for views that doesn't represent any entity and destroy them
            foreach (var item in this.list) {

                if (aliveEntities.Contains(item.Key) == false) {

                    var list = item.Value;
                    for (int i = 0, count = list.Count; i < count; ++i) {

                        var instance = list[i];
                        this.RecycleView_INTERNAL(ref instance);
                        --i;
                        --count;

                    }

                    list.Clear();

                }

            }
            
            PoolHashSet<EntityId>.Recycle(ref aliveEntities);
            
        }

        void IModule<TState>.Update(TState state, float deltaTime) {

            this.UpdateRequests();

            foreach (var item in this.list) {

                var list = item.Value;
                for (int i = 0, count = list.Count; i < count; ++i) {
                
                    var instance = list[i];
                    instance.ApplyState(this.GetData(instance), deltaTime, immediately: false);
                    instance.UpdateParticlesSimulation(deltaTime);
                
                }

            }

            // Update providers
            foreach (var providerKv in this.registryPrefabToProvider) {

                providerKv.Value.Update(this.list, deltaTime);

            }

        }

    }

}
#endif