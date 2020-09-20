#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using Unity.Jobs;

namespace ME.ECS {
    
    using ME.ECS.Views;
    
    public partial interface IWorldBase {
        
        void InstantiateView(ViewId prefab, Entity entity);
        void InstantiateView(IView prefab, Entity entity);

        void InstantiateViewShared(ViewId prefab);
        void InstantiateViewShared(IView prefab);
        
        ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer;
        bool UnRegisterViewSource(IView prefab);
        void DestroyView(ref IView instance);
        void DestroyAllViews(Entity entity);

        ViewId RegisterViewSourceShared<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer;
        bool UnRegisterViewSourceShared(IView prefab);
        void DestroyViewShared(ref IView instance);
        void DestroyAllViewsShared();

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static partial class EntityExtensions {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void InstantiateView(this Entity entity, ViewId viewId) {

            Worlds.currentWorld.InstantiateView(viewId, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void InstantiateView(this Entity entity, IView prefab) {

            Worlds.currentWorld.InstantiateView(prefab, entity);
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void DestroyEntityPlugin2(Entity entity) {

            var viewsModule = this.GetModule<ViewsModule>();
            viewsModule.DestroyAllViews(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        partial void RegisterPlugin1ModuleForEntity() {

            if (this.HasModule<ViewsModule>() == false) {

                this.AddModule<ViewsModule>();

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSource(IView prefab) {
            
            var viewsModule = this.GetModule<ViewsModule>();
            return viewsModule.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TProvider>(TProvider providerInitializer, IView prefab) where TProvider : struct, IViewsProviderInitializer {
            
            var viewsModule = this.GetModule<ViewsModule>();
            return viewsModule.RegisterViewSource(providerInitializer, prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(IView prefab, Entity entity) {

            var viewsModule = this.GetModule<ViewsModule>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ViewId prefab, Entity entity) {

            var viewsModule = this.GetModule<ViewsModule>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref IView instance) {
            
            var viewsModule = this.GetModule<ViewsModule>();
            viewsModule.DestroyView(ref instance);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews(Entity entity) {
            
            var viewsModule = this.GetModule<ViewsModule>();
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

    internal interface IViewBaseInternal {

        void Setup(World world, ViewInfo viewInfo);

    }

    public interface IViewBase {
        
        World world { get; }
        Entity entity { get; }
        ViewId prefabSourceId { get; }
        Tick creationTick { get; }

    }

    public interface IView : IViewBase, System.IComparable<IView> {

        void DoInitialize();
        void DoDeInitialize();
        void ApplyState(float deltaTime, bool immediately);
        void ApplyPhysicsState(float deltaTime);
        
        void UpdateParticlesSimulation(float deltaTime);
        void SimulateParticles(float time, uint seed);

    }

    public interface IViewModuleBase : IModuleBase {

        BufferArray<Views> GetData();
        System.Collections.IDictionary GetViewSourceData();
        IViewsProviderBase GetViewSourceProvider(ViewId viewSourceId);

        bool UpdateRequests();

    }

    public partial interface IViewModule : IViewModuleBase, IModule {

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

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
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

            return this.entity.id ^ this.prefabSourceId.GetHashCode(); //(int)MathUtils.GetKey(this.entity.id, this.prefabSourceId.GetHashCode()/* ^ this.creationTick.GetHashCode()*/);

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
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class ViewComponent : IComponentCopyable, IViewComponent {

        public ViewInfo viewInfo;
        public uint seed;

        public ref ViewInfo GetViewInfo() {

            return ref this.viewInfo;

        }

        void IPoolableRecycle.OnRecycle() {

            this.viewInfo = default;
            this.seed = default;

        }

        void IComponentCopyable.CopyFrom(IComponentCopyable other) {

            var otherView = (ViewComponent)other;
            this.viewInfo = otherView.viewInfo;
            this.seed = otherView.seed;

        }

    }

    /// <summary>
    /// Private predicate to filter components
    /// </summary>
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct RemoveComponentViewPredicate : IComponentPredicate<ViewComponent> {

        public int entityId;
        public ViewId prefabSourceId;
        public Tick creationTick;
        
        public bool Execute(ViewComponent data) {

            if (data.viewInfo.creationTick == this.creationTick && data.viewInfo.entity.id == this.entityId && data.viewInfo.prefabSourceId == this.prefabSourceId) {

                Worlds.currentWorld.currentState.storage.archetypes.Remove<ViewComponent>(in data.viewInfo.entity);
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
    public struct Views {

        public IView mainView;
        public ListCopyable<IView> otherViews;
        public bool isNotEmpty;

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public int Length {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                var count = 0;
                if (this.mainView != null) ++count;
                if (this.otherViews != null) count += this.otherViews.Count;
                return count;
            }
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public IView this[int i] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                if (i == 0) return this.mainView;
                return this.otherViews[i - 1];
            }
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Add(IView view) {
            
            if (this.mainView == null) {

                this.mainView = view;

            } else {

                if (this.otherViews == null) {

                    this.otherViews = PoolList<IView>.Spawn(1);
                    
                }

                this.otherViews.Add(view);

            }
            
            this.isNotEmpty = true;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Remove(IView view) {

            if (this.otherViews != null) {

                if (this.otherViews.Remove(view) == false) {

                    if (this.mainView == view) {

                        //this.mainView = null;
                        if (this.otherViews.Count > 0) {
                            
                            this.mainView = this.otherViews[this.otherViews.Count - 1];
                            this.otherViews.RemoveAt(this.otherViews.Count - 1);
                            this.isNotEmpty = true;
                            
                        } else {

                            this.mainView = null;
                            this.isNotEmpty = false;

                        }
                        return true;

                    }
                    
                }

            } else {

                if (this.mainView == view) {

                    this.mainView = null;
                    this.isNotEmpty = false;
                    return true;

                }

            }

            return false;

        }
        
    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class ViewsModule : IViewModule, IUpdate, IModulePhysicsUpdate {

        private const int REGISTRY_PROVIDERS_CAPACITY = 100;
        private const int REGISTRY_CAPACITY = 100;
        private const int VIEWS_CAPACITY = 1000;
        private const int INTERNAL_ENTITIES_CACHE_CAPACITY = 100;
        private const int INTERNAL_COMPONENTS_CACHE_CAPACITY = 10;
        
        private BufferArray<Views> list;
        private HashSet<ViewInfo> rendering;
        private Dictionary<ViewId, IViewsProviderInitializerBase> registryPrefabToProviderInitializer;
        private Dictionary<ViewId, IViewsProvider> registryPrefabToProvider;
        private Dictionary<IView, ViewId> registryPrefabToId;
        private Dictionary<ViewId, IView> registryIdToPrefab;
        private ViewId viewSourceIdRegistry;
        private ViewId viewIdRegistry;
        private bool isRequestsDirty;
        
        public World world { get; set; }
        
        void IModuleBase.OnConstruct() {

            this.isRequestsDirty = false;
            this.list = PoolArray<Views>.Spawn(ViewsModule.VIEWS_CAPACITY);
            this.rendering = PoolHashSet<ViewInfo>.Spawn(ViewsModule.VIEWS_CAPACITY);
            this.registryPrefabToId = PoolDictionary<IView, ViewId>.Spawn(ViewsModule.REGISTRY_CAPACITY);
            this.registryIdToPrefab = PoolDictionary<ViewId, IView>.Spawn(ViewsModule.REGISTRY_CAPACITY);

            this.registryPrefabToProvider = PoolDictionary<ViewId, IViewsProvider>.Spawn(ViewsModule.REGISTRY_PROVIDERS_CAPACITY);
            this.registryPrefabToProviderInitializer = PoolDictionary<ViewId, IViewsProviderInitializerBase>.Spawn(ViewsModule.REGISTRY_PROVIDERS_CAPACITY);

            Components.SetTypeInHash<ViewComponent>(false);
            
        }

        void IModuleBase.OnDeconstruct() {

            this.isRequestsDirty = true;
            this.UpdateRequests();
            
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

            for (int i = 0; i < this.list.Length; ++i) {

                var views = this.list.arr[i];
                if (views.otherViews != null) PoolList<IView>.Recycle(views.otherViews);
                
            }
            //PoolDictionary<int, List<IView<TEntity>>>.Recycle(ref this.list);
            PoolArray<Views>.Recycle(ref this.list);

        }

        BufferArray<Views> IViewModuleBase.GetData() {

            return this.list;

        }

        System.Collections.IDictionary IViewModuleBase.GetViewSourceData() {

            return this.registryIdToPrefab;

        }

        IViewsProviderBase IViewModuleBase.GetViewSourceProvider(ViewId viewSourceId) {

            IViewsProvider provider;
            if (this.registryPrefabToProvider.TryGetValue(viewSourceId, out provider) == true) {

                return provider;

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(IView prefab, Entity entity) {
            
            this.InstantiateView(this.GetViewSourceId(prefab), entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ViewId sourceId, Entity entity) {

            if (this.world.settings.turnOffViews == true) return;

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false && this.world.HasResetState() == true) {

                throw new OutOfStateException();

            }

            if (this.registryIdToPrefab.ContainsKey(sourceId) == false) {

                throw new ViewRegistryNotFoundException(sourceId);

            }

            var viewInfo = new ViewInfo(entity, sourceId, this.world.GetStateTick());
            
            var component = this.world.AddComponent<ViewComponent>(entity);
            component.viewInfo = viewInfo;
            component.seed = (uint)this.world.GetSeedValue();

            /*var request = this.world.AddComponent<CreateViewComponentRequest<TState>, IViewComponentRequest<TState>>(entity);
            request.viewInfo = viewInfo;
            request.seed = component.seed;*/

            this.isRequestsDirty = true;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref IView instance) {

            if (this.world.settings.turnOffViews == true) return;

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false && this.world.HasResetState() == true) {

                throw new OutOfStateException();

            }

            var predicate = new RemoveComponentViewPredicate();
            predicate.entityId = instance.entity.id;
            predicate.prefabSourceId = instance.prefabSourceId;
            predicate.creationTick = instance.creationTick;

            this.world.RemoveComponentsPredicate<ViewComponent, RemoveComponentViewPredicate>(instance.entity, predicate);

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
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private IView SpawnView_INTERNAL(ViewInfo viewInfo) {

            IViewsProvider provider;
            if (this.registryPrefabToProvider.TryGetValue(viewInfo.prefabSourceId, out provider) == true) {

                var instance = provider.Spawn(this.GetViewSource(viewInfo.prefabSourceId), viewInfo.prefabSourceId);
                var instanceInternal = (IViewBaseInternal)instance;
                instanceInternal.Setup(this.world, viewInfo);
                this.Register(instance);

                return instance;

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RecycleView_INTERNAL(ref IView instance) {

            var viewInstance = instance;
            this.UnRegister(instance);

            if (this.registryPrefabToProvider.TryGetValue(viewInstance.prefabSourceId, out var provider) == true) {

                provider.Destroy(ref viewInstance);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyAllViews(Entity entity) {

            if (entity.id >= this.list.Length) return;
            
            var views = this.list.arr[entity.id];
            if (views.mainView == null) return;

            this.DestroyView(ref views.mainView);
            if (views.otherViews != null) {

                for (int i = 0, length = views.otherViews.Count; i < length; ++i) {

                    var view = views.otherViews[i];
                    this.DestroyView(ref view);

                }

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

            if (this.registryPrefabToId.TryGetValue(prefab, out var viewId) == true) {

                return viewId;

            }
            
            if (this.world.HasStep(WorldStep.LogicTick) == true) {

                throw new InStateException();

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

        public bool UnRegisterViewSource(IView prefab) {

            if (this.registryPrefabToId.TryGetValue(prefab, out var viewId) == true) {

                if (this.world.HasStep(WorldStep.LogicTick) == true) {

                    throw new InStateException();

                }

                var provider = this.registryPrefabToProvider[viewId];
                provider.world = null;
                provider.OnDeconstruct();
                ((IViewsProviderInitializer)this.registryPrefabToProviderInitializer[viewId]).Destroy(provider);
                this.registryPrefabToProviderInitializer.Remove(viewId);
                this.registryPrefabToProvider.Remove(viewId);
                this.registryPrefabToId.Remove(prefab);
                return this.registryIdToPrefab.Remove(viewId);

            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Register(IView instance) {

            var id = instance.entity.id;
            ArrayUtils.Resize(in id, ref this.list);

            this.list.arr[id].Add(instance);
            
            var viewInfo = new ViewInfo(instance.entity, instance.prefabSourceId, instance.creationTick);
            this.rendering.Add(viewInfo);

            instance.DoInitialize();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegister(IView instance) {
            
            instance.DoDeInitialize();

            var id = instance.entity.id;
            if (id < this.list.Length) {

                this.list.arr[id].Remove(instance);

            }

            var viewInfo = new ViewInfo(instance.entity, instance.prefabSourceId, instance.creationTick);
            return this.rendering.Remove(viewInfo);

        }

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

            /*foreach (var item in this.rendering) {

                if (item.Equals(viewInfo) == true) {

                    return true;

                }
                
            }*/

            return this.rendering.Contains(viewInfo);

        }

        //private HashSet<ViewInfo> prevList = new HashSet<ViewInfo>();
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UpdateRequests() {

            if (this.isRequestsDirty == false) return false;
            this.isRequestsDirty = false;

            var hasChanged = false;
            var aliveEntities = PoolHashSet<int>.Spawn(ViewsModule.INTERNAL_ENTITIES_CACHE_CAPACITY);
            if (this.world.ForEachEntity(out RefList<Entity> allEntities) == true) {

                for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {

                    if (allEntities.IsFree(j) == true) continue;
                    ref var item = ref allEntities[j];

                    aliveEntities.Add(item.id);
                    
                    var allViews = this.world.ForEachComponent<ViewComponent>(item);
                    if (allViews != null) {

                        // Comparing current state views to current rendering
                        foreach (var viewComponent in allViews) {

                            var view = (ViewComponent)viewComponent;
                            if (this.IsRenderingNow(in view.viewInfo) == true) {

                                // is rendering now
                                //this.prevList.Add(view.viewInfo);

                            } else {

                                // is not rendering now
                                // create required instance
                                this.CreateVisualInstance(in view.seed, in view.viewInfo);
                                hasChanged = true;

                            }

                        }

                    }

                }
                
            }

            for (var id = this.list.Length - 1; id >= 0; --id) {
                
                ref var views = ref this.list.arr[id];
                if (views.mainView == null) continue;
                
                if (aliveEntities.Contains(id) == false) {

                    if (views.otherViews != null) {

                        for (int i = 0, count = views.otherViews.Count; i < count; ++i) {

                            var instance = views.otherViews[i];
                            this.RecycleView_INTERNAL(ref instance);
                            --i;
                            --count;

                        }
                        views.otherViews.Clear();

                    }

                    this.RecycleView_INTERNAL(ref views.mainView);
                    hasChanged = true;

                } else {
                    
                    // If entity is alive - check if we are rendering needed view
                    for (int i = views.Length - 1; i >= 0; --i) {

                        var instance = views[i];
                        var allViews = this.world.ForEachComponent<ViewComponent>(instance.entity);
                        if (allViews == null) continue;

                        //ViewComponent viewFound = null;
                        var found = false;
                        for (int index = 0, count = allViews.Count; index < count; ++index) {
                            
                            var viewComponent = allViews[index];
                            var view = (ViewComponent)viewComponent;
                            if (instance.prefabSourceId == view.viewInfo.prefabSourceId) {

                                //viewFound = view;
                                found = true;
                                break;

                            }
                            
                        }

                        if (found == false) {
                            
                            this.RecycleView_INTERNAL(ref instance);
                            hasChanged = true;
                            //--i;
                            //--count;
                            
                        } /*else {
                            
                            if (this.IsRenderingNow(in viewFound.viewInfo) == false) this.CreateVisualInstance(in viewFound.seed, in viewFound.viewInfo);
                            
                        }*/

                    }
                    
                }

            }

            PoolHashSet<int>.Recycle(ref aliveEntities);

            return hasChanged;

        }

        void IModulePhysicsUpdate.UpdatePhysics(float deltaTime) {
            
            for (var id = 0; id < this.list.Length; ++id) {
                
                ref var list = ref this.list.arr[id];
                if (list.mainView == null) continue;
                
                for (int i = 0, count = list.Length; i < count; ++i) {

                    var instance = list[i];
                    if (instance != null) instance.ApplyPhysicsState(deltaTime);
                    
                }
                
            }
            
        }

        public void Update(in float deltaTime) {

            if (this.world.settings.turnOffViews == true) return;
            
            var hasChanged = this.UpdateRequests();

            for (var id = 0; id < this.list.Length; ++id) {
                
                ref var list = ref this.list.arr[id];
                if (list.mainView == null) continue;
                
                for (int i = 0, count = list.Length; i < count; ++i) {

                    var instance = list[i];
                    instance.ApplyState(deltaTime, immediately: false);
                    instance.UpdateParticlesSimulation(deltaTime);

                }
                
            }

            // Update providers
            foreach (var providerKv in this.registryPrefabToProvider) {

                providerKv.Value.Update(this.list, deltaTime, hasChanged);

            }

        }

    }

}
#endif