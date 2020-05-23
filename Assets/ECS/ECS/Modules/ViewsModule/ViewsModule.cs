#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using Unity.Jobs;

namespace ME.ECS {
    
    using ME.ECS.Views;
    
    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {
        
        ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer;
        bool UnRegisterViewSource(IView prefab);
        void InstantiateView(ViewId prefab, Entity entity);
        void InstantiateView(IView prefab, Entity entity);
        void DestroyView(ref IView instance);
        void DestroyAllViews(Entity entity);

        ViewId RegisterViewSourceShared<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer;
        bool UnRegisterViewSourceShared(IView prefab);
        void InstantiateViewShared(ViewId prefab);
        void InstantiateViewShared(IView prefab);
        void DestroyViewShared(ref IView instance);
        void DestroyAllViewsShared();

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void DestroyEntityPlugin2(Entity entity) {

            var viewsModule = this.GetModule<ViewsModule<TState>>();
            viewsModule.DestroyAllViews(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void RegisterPlugin1ModuleForEntity() {

            if (this.HasModule<ViewsModule<TState>>() == false) {

                this.AddModule<ViewsModule<TState>>();

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSource(IView prefab) {
            
            var viewsModule = this.GetModule<ViewsModule<TState>>();
            return viewsModule.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer {
            
            var viewsModule = this.GetModule<ViewsModule<TState>>();
            return viewsModule.RegisterViewSource(providerInitializer, prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(IView prefab, Entity entity) {

            var viewsModule = this.GetModule<ViewsModule<TState>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ViewId prefab, Entity entity) {

            var viewsModule = this.GetModule<ViewsModule<TState>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref IView instance) {
            
            var viewsModule = this.GetModule<ViewsModule<TState>>();
            viewsModule.DestroyView(ref instance);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews(Entity entity) {
            
            var viewsModule = this.GetModule<ViewsModule<TState>>();
            viewsModule.DestroyAllViews(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSourceShared(IView prefab) {

            return this.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSourceShared<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer {

            return this.RegisterViewSource<TProvider>(providerInitializer, prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateViewShared(IView prefab) {

            this.InstantiateView(prefab, this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateViewShared(ViewId prefab) {

            this.InstantiateView(prefab, this.sharedEntity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyViewShared(ref IView instance) {
            
            this.DestroyView(ref instance);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViewsShared() {
            
            this.DestroyAllViews(this.sharedEntity);
            
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

    public interface IView : IViewBase {

        void DoInitialize();
        void DoDeInitialize();
        void ApplyState(float deltaTime, bool immediately);
        
        void UpdateParticlesSimulation(float deltaTime);
        void SimulateParticles(float time, uint seed);

    }

    public interface IViewModuleBase : IModuleBase {

        System.Collections.ICollection GetData();
        System.Collections.IDictionary GetViewSourceData();
        IViewsProviderBase GetViewSourceProvider(ViewId viewSourceId);

    }

    public partial interface IViewModule<TState> : IViewModuleBase, IModule<TState> where TState : class, IState<TState>, new() {

        void Register(IView instance);
        bool UnRegister(IView instance);

        ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer;
        bool UnRegisterViewSource(IView prefab);
        
        void InstantiateView(IView prefab, Entity entity);
        void InstantiateView(ViewId prefabSourceId, Entity entity);
        void DestroyView(ref IView instance);

    }

    public class ViewRegistryNotFoundException : System.Exception {

        public ViewRegistryNotFoundException(ViewId sourceViewId) : base("[Views] View with id " + sourceViewId.ToString() + " not found in registry. Have you called RegisterViewSource()?") {}

    }

    public interface IViewComponent {

        ref ViewInfo GetViewInfo();

    }

    #if UNITY_ENABLED
    public abstract class ViewBase : UnityEngine.MonoBehaviour {

        

    }
    #endif

    public readonly struct ViewInfo : System.IEquatable<ViewInfo> {

        public readonly Entity entity;
        public readonly ViewId prefabSourceId;
        public readonly Tick creationTick;

        public ViewInfo(Entity entity, ViewId prefabSourceId, Tick creationTick) {

            this.entity = entity;
            this.prefabSourceId = prefabSourceId;
            this.creationTick = creationTick;

        }

        public ulong GetKey() {

            return MathUtils.GetKey((uint)this.entity.id, this.prefabSourceId.v);

        }

        public override int GetHashCode() {

            return (int)MathUtils.GetKey(this.entity.id, this.prefabSourceId.GetHashCode()/* ^ this.creationTick.GetHashCode()*/);
            
        }
            
        public override bool Equals(object obj) {
                
            throw new AllocationException();
                
        }

        public bool Equals(ViewInfo p) {
                
            return /*this.creationTick == p.creationTick &&*/ this.entity.id == p.entity.id && this.prefabSourceId == p.prefabSourceId;
                
        }

        public override string ToString() {
                
            return this.entity.ToString() + "\nPrefab Source Id: " + this.prefabSourceId.ToString() + "\nCreation Tick: " + this.creationTick.ToString();
                
        }

    }

    /// <summary>
    /// Private component class to describe Views
    /// </summary>
    public class ViewComponent<TState> : IComponentCopyable<TState>, IViewComponent where TState : IStateBase {

        public ViewInfo viewInfo;
        public uint seed;

        public ref ViewInfo GetViewInfo() {

            return ref this.viewInfo;

        }

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.seed = default;

        }

        void IComponentCopyable<TState>.CopyFrom(IComponentCopyable<TState> other) {

            var otherView = (ViewComponent<TState>)other;
            this.viewInfo = otherView.viewInfo;
            this.seed = otherView.seed;

        }

    }

    public interface IViewComponentRequest<TState> : IComponentCopyable<TState> where TState : IStateBase {}

    public class DestroyViewComponentRequest<TState> : IViewComponentRequest<TState> where TState : IStateBase {

        public ViewInfo viewInfo;
        public IView viewInstance;

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.viewInstance = default;

        }

        void IComponentCopyable<TState>.CopyFrom(IComponentCopyable<TState> other) {
            
            var otherView = (DestroyViewComponentRequest<TState>)other;
            this.viewInfo = otherView.viewInfo;
            this.viewInstance = otherView.viewInstance;

        }

    }

    public class CreateViewComponentRequest<TState> : IViewComponentRequest<TState> where TState : IStateBase {

        public ViewInfo viewInfo;
        public uint seed;

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.seed = default;

        }

        void IComponentCopyable<TState>.CopyFrom(IComponentCopyable<TState> other) {
            
            var otherView = (CreateViewComponentRequest<TState>)other;
            this.viewInfo = otherView.viewInfo;
            this.seed = otherView.seed;

        }

    }

    /// <summary>
    /// Private predicate to filter components
    /// </summary>
    public struct RemoveComponentViewPredicate<TState> : IComponentPredicate<ViewComponent<TState>> where TState : class, IState<TState>, new() {

        public int entityId;
        public ViewId prefabSourceId;
        public Tick creationTick;
        
        public bool Execute(ViewComponent<TState> data) {

            if (data.viewInfo.creationTick == this.creationTick && data.viewInfo.entity.id == this.entityId && data.viewInfo.prefabSourceId == this.prefabSourceId) {

                Worlds<TState>.currentWorld.storagesCache.archetypes.Remove<ViewComponent<TState>>(in data.viewInfo.entity);
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
    public partial class ViewsModule<TState> : IViewModule<TState> where TState : class, IState<TState>, new() {

        private const int REGISTRY_PROVIDERS_CAPACITY = 100;
        private const int REGISTRY_CAPACITY = 100;
        private const int VIEWS_CAPACITY = 1000;
        private const int INTERNAL_ENTITIES_CACHE_CAPACITY = 100;
        private const int INTERNAL_COMPONENTS_CACHE_CAPACITY = 10;
        
        private List<IView>[] list;
        private HashSet<ViewInfo> rendering;
        private Dictionary<ViewId, IViewsProviderInitializerBase> registryPrefabToProviderInitializer;
        private Dictionary<ViewId, IViewsProvider> registryPrefabToProvider;
        private Dictionary<IView, ViewId> registryPrefabToId;
        private Dictionary<ViewId, IView> registryIdToPrefab;
        private ViewId viewSourceIdRegistry;
        private ViewId viewIdRegistry;
        private bool isRequestsDirty;
        
        public IWorld<TState> world { get; set; }

        void IModuleBase.OnConstruct() {

            this.isRequestsDirty = false;
            this.list = PoolArray<List<IView>>.Spawn(ViewsModule<TState>.VIEWS_CAPACITY);
            this.rendering = PoolHashSet<ViewInfo>.Spawn(ViewsModule<TState>.VIEWS_CAPACITY);
            this.registryPrefabToId = PoolDictionary<IView, ViewId>.Spawn(ViewsModule<TState>.REGISTRY_CAPACITY);
            this.registryIdToPrefab = PoolDictionary<ViewId, IView>.Spawn(ViewsModule<TState>.REGISTRY_CAPACITY);

            this.registryPrefabToProvider = PoolDictionary<ViewId, IViewsProvider>.Spawn(ViewsModule<TState>.REGISTRY_PROVIDERS_CAPACITY);
            this.registryPrefabToProviderInitializer = PoolDictionary<ViewId, IViewsProviderInitializerBase>.Spawn(ViewsModule<TState>.REGISTRY_PROVIDERS_CAPACITY);

        }

        void IModuleBase.OnDeconstruct() {

            var temp = PoolList<IView>.Spawn(this.registryPrefabToId.Count);
            foreach (var prefab in this.registryIdToPrefab) {

                temp.Add(prefab.Value);
                
            }

            foreach (var prefab in temp) {
                
                this.UnRegisterViewSource(prefab);
                
            }
            PoolList<IView>.Recycle(ref temp);

            PoolDictionary<ViewId, IViewsProvider>.Recycle(ref this.registryPrefabToProvider);
            PoolDictionary<ViewId, IViewsProviderInitializerBase>.Recycle(ref this.registryPrefabToProviderInitializer);
            
            PoolDictionary<ViewId, IView>.Recycle(ref this.registryIdToPrefab);
            PoolDictionary<IView, ViewId>.Recycle(ref this.registryPrefabToId);
            
            PoolHashSet<ViewInfo>.Recycle(ref this.rendering);

            foreach (var item in this.list) {
                
                if (item != null) PoolList<IView>.Recycle(item);
                
            }
            //PoolDictionary<int, List<IView<TEntity>>>.Recycle(ref this.list);
            PoolArray<List<IView>>.Recycle(ref this.list);

        }

        System.Collections.ICollection IViewModuleBase.GetData() {

            return this.list;

        }

        System.Collections.IDictionary IViewModuleBase.GetViewSourceData() {

            return this.registryIdToPrefab;

        }

        IViewsProviderBase IViewModuleBase.GetViewSourceProvider(ViewId viewSourceId) {

            lock (this.registryPrefabToProvider) {

                IViewsProvider provider;
                if (this.registryPrefabToProvider.TryGetValue(viewSourceId, out provider) == true) {

                    return provider;

                }

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(IView prefab, Entity entity) {
            
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

            lock (this) {

                var viewInfo = new ViewInfo(entity, sourceId, this.world.GetStateTick());
                
                var component = this.world.AddComponent<ViewComponent<TState>>(entity);
                component.viewInfo = viewInfo;
                component.seed = (uint)this.world.GetSeedValue();

                /*var request = this.world.AddComponent<CreateViewComponentRequest<TState>, IViewComponentRequest<TState>>(entity);
                request.viewInfo = viewInfo;
                request.seed = component.seed;*/

                this.isRequestsDirty = true;

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref IView instance) {

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false && this.world.HasResetState() == true) {

                throw new OutOfStateException();

            }

            lock (this) {

                var predicate = new RemoveComponentViewPredicate<TState>();
                predicate.entityId = instance.entity.id;
                predicate.prefabSourceId = instance.prefabSourceId;
                predicate.creationTick = instance.creationTick;

                this.world.RemoveComponentsPredicate<ViewComponent<TState>, RemoveComponentViewPredicate<TState>>(instance.entity, predicate);

                this.isRequestsDirty = true;
                
                /*var viewInfo = new ViewInfo();
                viewInfo.entity = instance.entity;
                viewInfo.prefabSourceId = instance.prefabSourceId;
                viewInfo.creationTick = instance.creationTick;

                var request = this.world.AddComponent<DestroyViewComponentRequest<TState>, IViewComponentRequest<TState>>(instance.entity);
                request.viewInfo = viewInfo;
                request.viewInstance = instance;*/

                instance = null;

            }

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private IView SpawnView_INTERNAL(ViewInfo viewInfo) {

            lock (this.registryPrefabToProvider) {

                IViewsProvider provider;
                if (this.registryPrefabToProvider.TryGetValue(viewInfo.prefabSourceId, out provider) == true) {

                    var instance = provider.Spawn(this.GetViewSource(viewInfo.prefabSourceId), viewInfo.prefabSourceId);
                    instance.entity = viewInfo.entity;
                    instance.prefabSourceId = viewInfo.prefabSourceId;
                    instance.creationTick = viewInfo.creationTick;
                    this.Register(instance);

                    return instance;

                }

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RecycleView_INTERNAL(ref IView instance) {

            this.UnRegister(instance);

            lock (this.registryPrefabToProvider) {

                if (this.registryPrefabToProvider.TryGetValue(instance.prefabSourceId, out var provider) == true) {

                    provider.Destroy(ref instance);

                }

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews(Entity entity) {

            if (entity.id >= this.list.Length) return;
            
            var views = this.list[entity.id];
            if (views == null) return;
            
            for (int i = 0, length = views.Count; i < length; ++i) {
                
                var view = views[i];
                this.DestroyView(ref view);
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IView GetViewSource(ViewId viewSourceId) {

            if (this.registryIdToPrefab.TryGetValue(viewSourceId, out var prefab) == true) {

                return prefab;

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId GetViewSourceId(IView prefab) {

            if (this.registryPrefabToId.TryGetValue(prefab, out var viewId) == true) {

                return viewId;

            }

            return ViewId.Zero;

        }

        public ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer {

            /*if (this.world.HasStep(WorldStep.LogicTick) == true) {

                throw new InStateException();

            }*/

            lock (this.registryPrefabToProvider) {

                if (this.registryPrefabToId.TryGetValue(prefab, out var viewId) == true) {

                    return viewId;

                }

                ++this.viewSourceIdRegistry;
                this.registryPrefabToId.Add(prefab, this.viewSourceIdRegistry);
                this.registryIdToPrefab.Add(this.viewSourceIdRegistry, prefab);
                var viewsProvider = (IViewsProviderInitializer)providerInitializer;
                var provider = viewsProvider.Create();
                provider.world = this.world;
                provider.OnConstruct();
                this.registryPrefabToProvider.Add(this.viewSourceIdRegistry, provider);
                this.registryPrefabToProviderInitializer.Add(this.viewSourceIdRegistry, viewsProvider);

                return this.viewSourceIdRegistry;

            }
            
        }

        public bool UnRegisterViewSource(IView prefab) {

            if (this.world.HasStep(WorldStep.LogicTick) == true) {

                throw new InStateException();

            }

            lock (this.registryPrefabToProvider) {

                if (this.registryPrefabToId.TryGetValue(prefab, out var viewId) == true) {

                    var provider = this.registryPrefabToProvider[viewId];
                    provider.world = null;
                    provider.OnDeconstruct();
                    ((IViewsProviderInitializer)this.registryPrefabToProviderInitializer[viewId]).Destroy(provider);
                    this.registryPrefabToProviderInitializer.Remove(viewId);
                    this.registryPrefabToProvider.Remove(viewId);
                    this.registryPrefabToId.Remove(prefab);
                    return this.registryIdToPrefab.Remove(viewId);

                }

            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Register(IView instance) {

            var id = instance.entity.id;
            ArrayUtils.Resize(in id, ref this.list);

            if (this.list[id] == null) {
                
                var list = PoolList<IView>.Spawn(100);
                list.Add(instance);
                this.list[id] = list;

            } else {
                
                this.list[id].Add(instance);
                
            }

            var viewInfo = new ViewInfo(instance.entity, instance.prefabSourceId, instance.creationTick);
            this.rendering.Add(viewInfo);

            instance.DoInitialize();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegister(IView instance) {
            
            return this.UnRegister_INTERNAL(instance, removeFromList: true);
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool UnRegister_INTERNAL(IView instance, bool removeFromList = true) {
            
            instance.DoDeInitialize();

            if (removeFromList == true) {

                var id = instance.entity.id;
                if (id < this.list.Length) {

                    this.list[id].Remove(instance);

                }

            }

            var viewInfo = new ViewInfo(instance.entity, instance.prefabSourceId, instance.creationTick);
            return this.rendering.Remove(viewInfo);

        }

        void IModule<TState>.AdvanceTick(in TState state, in float deltaTime) {}

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void CreateVisualInstance(in uint seed, in ViewInfo viewInfo) {
            
            var instance = this.SpawnView_INTERNAL(viewInfo);
            if (instance == null) {

                UnityEngine.Debug.LogError("CreateVisualInstance failed while viewInfo.prefabSourceId: " + viewInfo.prefabSourceId + " and contains: " + this.registryPrefabToProvider.ContainsKey(viewInfo.prefabSourceId));

            }

            // Call ApplyState with deltaTime = current time offset
            var dt = UnityEngine.Mathf.Max(0f, (this.world.GetCurrentTick() - viewInfo.creationTick) * this.world.GetTickTime());
            instance.ApplyState(dt, immediately: true);
            // Simulate particle systems
            instance.SimulateParticles(dt, seed);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool IsRenderingNow(in ViewInfo viewInfo) {

            foreach (var item in this.rendering) {

                if (item.Equals(viewInfo) == true) {

                    return true;

                }
                
            }

            return false;//this.rendering.Contains(viewInfo);

        }

        //private HashSet<ViewInfo> prevList = new HashSet<ViewInfo>();
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateRequests() {

            if (this.isRequestsDirty == false) return;
            this.isRequestsDirty = false;
            
            var aliveEntities = PoolHashSet<int>.Spawn(ViewsModule<TState>.INTERNAL_ENTITIES_CACHE_CAPACITY);
            if (this.world.ForEachEntity(out RefList<Entity> allEntities) == true) {

                for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {

                    if (allEntities.IsFree(j) == true) continue;
                    ref var item = ref allEntities[j];

                    aliveEntities.Add(item.id);
                    
                    var allViews = this.world.ForEachComponent<ViewComponent<TState>>(item);
                    if (allViews != null) {

                        lock (this) {

                            // Comparing current state views to current rendering
                            foreach (var viewComponent in allViews) {

                                var view = (ViewComponent<TState>)viewComponent;
                                if (this.IsRenderingNow(in view.viewInfo) == true) {

                                    // is rendering now
                                    //this.prevList.Add(view.viewInfo);

                                } else {

                                    // is not rendering now
                                    // create required instance
                                    this.CreateVisualInstance(in view.seed, in view.viewInfo);

                                }

                            }

                        }

                    }

                }
                
            }

            for (var id = 0; id < this.list.Length; ++id) {
                
                var list = this.list[id];
                if (list == null) continue;

                if (aliveEntities.Contains(id) == false) {

                    for (int i = 0, count = list.Count; i < count; ++i) {

                        var instance = list[i];
                        this.RecycleView_INTERNAL(ref instance);
                        --i;
                        --count;

                    }

                    list.Clear();

                } else {
                    
                    // If entity is alive - check if we are rendering needed view
                    for (int i = 0, count = list.Count; i < count; ++i) {

                        var instance = list[i];
                        var allViews = this.world.ForEachComponent<ViewComponent<TState>>(instance.entity);
                        if (allViews == null) continue;
                        
                        var found = false;
                        foreach (var viewComponent in allViews) {
                            
                            var view = (ViewComponent<TState>)viewComponent;
                            if (instance.prefabSourceId == view.viewInfo.prefabSourceId &&
                                instance.creationTick == view.viewInfo.creationTick) {

                                found = true;
                                break;

                            }
                            
                        }

                        if (found == false) {
                            
                            this.RecycleView_INTERNAL(ref instance);
                            --i;
                            --count;
                            
                        }

                    }
                    
                }

            }

            {
                
                /*for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {

                    // For each entity in state
                    ref var item = ref allEntities[j];
                    if (allEntities.IsFree(j) == true) continue;

                    aliveEntities.Add(item.entity.id);

                    // Get all requests
                    var requests = this.world.ForEachComponent<TEntity, IViewComponentRequest<TState>>(item.entity);
                    if (requests != null) {

                        foreach (var request in requests) {

                            if (request is CreateViewComponentRequest<TState> createComponentRequest) {

                                this.CreateVisualInstance(in item, in createComponentRequest.seed, in createComponentRequest.viewInfo);
                                
                            } else if (request is DestroyViewComponentRequest<TState> destroyComponentRequest) {

                                if (this.list.TryGetValue(item.entity.id, out var viewsList) == true) {

                                    viewsList.Remove(destroyComponentRequest.viewInstance);

                                }

                                this.RecycleView_INTERNAL(ref destroyComponentRequest.viewInstance);

                            }

                        }

                    }
                    this.world.RemoveComponents<TEntity, IViewComponentRequest<TState>>(item.entity);

                    // Get all views
                    var allViews = this.world.ForEachComponent<TEntity, ViewComponent<TState>>(item.entity);
                    if (allViews != null) {
                        
                        // Comparing current state views to current rendering
                        foreach (var viewComponent in allViews) {

                            var view = (ViewComponent<TState>)viewComponent;
                            if (this.IsRenderingNow(in view.viewInfo) == false) {
                                
                                // current data doesn't represent any visual instance
                                this.CreateVisualInstance(in item, in view.seed, in view.viewInfo);

                            }

                        }

                    }

                }*/

            }
            // Iterate all current view instances
            // Search for views that doesn't represent any entity and destroy them
            /*foreach (var item in this.list) {

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

            }*/
            
            PoolHashSet<int>.Recycle(ref aliveEntities);
            
        }

        void IModule<TState>.Update(in TState state, in float deltaTime) {

            this.UpdateRequests();

            for (var id = 0; id < this.list.Length; ++id) {
                
                var list = this.list[id];
                if (list == null) continue;
                
                for (int i = 0, count = list.Count; i < count; ++i) {

                    var instance = list[i];
                    instance.ApplyState(deltaTime, immediately: false);
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