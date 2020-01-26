#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {
    
    using ME.ECS.Views;

    public partial interface IWorld<TState> where TState : class, IState<TState> {
        
        ViewId RegisterViewSource<TEntity, TProvider>(IView<TEntity> prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProvider;
        bool UnRegisterViewSource<TEntity>(IView<TEntity> prefab) where TEntity : struct, IEntity;

        //void AddViewsProvider<TProvider>() where TProvider : class, IViewsProvider, new();
        void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(IView<TEntity> prefab, Entity entity) where TEntity : struct, IEntity;
        void DestroyView<TEntity>(ref IView<TEntity> instance) where TEntity : struct, IEntity;

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
        public ViewId RegisterViewSource<TEntity, TProvider>(IView<TEntity> prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProvider {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.RegisterViewSource<TProvider>(prefab);

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

    }

}

namespace ME.ECS.Views {

    public interface IViewBase {
        
        Entity entity { get; set; }
        ViewId prefabSourceId { get; set; }
        Tick creationTick { get; set; }

    }

    public interface IView<T> : IViewBase where T : struct, IEntity {

        void OnInitialize(in T data);
        void OnDeInitialize(in T data);
        void ApplyState(in T data, float deltaTime, bool immediately);
        void SimulateParticles(float time);

    }

    public interface IViewModuleBase : IModuleBase {

        System.Collections.IDictionary GetData();

    }

    public partial interface IViewModule<TState, TEntity> : IViewModuleBase, IModule<TState> where TState : class, IState<TState> where TEntity : struct, IEntity {

        void Register(IView<TEntity> instance);
        void UnRegister(IView<TEntity> instance);

        ViewId RegisterViewSource<TProvider>(IView<TEntity> prefab) where TProvider : struct, IViewsProvider;
        bool UnRegisterViewSource(IView<TEntity> prefab);
        
        void InstantiateView(IView<TEntity> prefab, Entity entity);
        void InstantiateView(ViewId prefabSourceId, Entity entity);
        void DestroyView(ref IView<TEntity> instance);

    }

    public class ViewRegistryNotFoundException : System.Exception {

        public ViewRegistryNotFoundException(ViewId sourceViewId) : base("[Views] View with id " + sourceViewId.ToString() + " not found in registry. Have you called RegisterViewSource()?") {}

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class ViewsModule<TState, TEntity> : IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        private const int REGISTRY_PROVIDERS_CAPACITY = 100;
        private const int REGISTRY_CAPACITY = 100;
        private const int VIEWS_CAPACITY = 1000;
        private const int INTERNAL_ENTITIES_CACHE_CAPACITY = 100;
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

        private struct ViewInfo : System.IEquatable<ViewInfo> {

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

        }

        private Dictionary<EntityId, List<IView<TEntity>>> list;
        private Dictionary<EntityId, Dictionary<ViewId, List<IView<TEntity>>>> listDic;
        private HashSet<ViewInfo> rendering;
        private Dictionary<ViewId, IViewsProvider> registryPrefabToProviderBase;
        private Dictionary<ViewId, IViewsProvider<TEntity>> registryPrefabToProvider;
        private Dictionary<IView<TEntity>, ViewId> registryPrefabToId;
        private Dictionary<ViewId, IView<TEntity>> registryIdToPrefab;
        private ViewId viewSourceIdRegistry;
        private ViewId viewIdRegistry;
        
        public IWorld<TState> world { get; set; }

        void IModule<TState>.OnConstruct() {

            this.list = PoolDictionary<EntityId, List<IView<TEntity>>>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.listDic = PoolDictionary<EntityId, Dictionary<ViewId, List<IView<TEntity>>>>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.rendering = PoolHashSet<ViewInfo>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.registryPrefabToId = PoolDictionary<IView<TEntity>, ViewId>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);
            this.registryIdToPrefab = PoolDictionary<ViewId, IView<TEntity>>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);

            this.registryPrefabToProvider = PoolDictionary<ViewId, IViewsProvider<TEntity>>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_PROVIDERS_CAPACITY);
            this.registryPrefabToProviderBase = PoolDictionary<ViewId, IViewsProvider>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_PROVIDERS_CAPACITY);

        }

        void IModule<TState>.OnDeconstruct() {
            
            PoolDictionary<ViewId, IViewsProvider<TEntity>>.Recycle(ref this.registryPrefabToProvider);
            PoolDictionary<ViewId, IViewsProvider>.Recycle(ref this.registryPrefabToProviderBase);
            
            PoolDictionary<ViewId, IView<TEntity>>.Recycle(ref this.registryIdToPrefab);
            PoolDictionary<IView<TEntity>, ViewId>.Recycle(ref this.registryPrefabToId);
            
            PoolHashSet<ViewInfo>.Recycle(ref this.rendering);

            foreach (var item in this.listDic) {

                foreach (var itemView in item.Value) {
                
                    PoolList<IView<TEntity>>.Recycle(itemView.Value);
                
                }
                PoolDictionary<ViewId, List<IView<TEntity>>>.Recycle(item.Value);

            }
            PoolDictionary<EntityId, Dictionary<ViewId, List<IView<TEntity>>>>.Recycle(ref this.listDic);

            foreach (var item in this.list) {
                
                PoolList<IView<TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, List<IView<TEntity>>>.Recycle(ref this.list);

        }

        System.Collections.IDictionary IViewModuleBase.GetData() {

            return this.list;

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
            viewInfo.creationTick = this.world.GetTick();

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
                list.Clear();

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

        public ViewId RegisterViewSource<TProvider>(IView<TEntity> prefab) where TProvider : struct, IViewsProvider {

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
            var provider = new TProvider().Create<TEntity>();
            provider.OnConstruct();
            this.registryPrefabToProvider.Add(this.viewSourceIdRegistry, provider);
            this.registryPrefabToProviderBase.Add(this.viewSourceIdRegistry, new TProvider());

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
                this.registryPrefabToProviderBase[viewId].Destroy(provider);
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

            Dictionary<ViewId, List<IView<TEntity>>> dic;
            if (this.listDic.TryGetValue(instance.entity.id, out dic) == true) {

                if (dic.TryGetValue(instance.prefabSourceId, out list) == true) {
                    
                    list.Add(instance);
                    
                } else {
                    
                    list = PoolList<IView<TEntity>>.Spawn(1);
                    list.Add(instance);
                    dic.Add(instance.prefabSourceId, list);
                    
                }

            } else {

                dic = PoolDictionary<ViewId, List<IView<TEntity>>>.Spawn(100);
                list = PoolList<IView<TEntity>>.Spawn(1);
                list.Add(instance);
                dic.Add(instance.prefabSourceId, list);
                this.listDic.Add(instance.entity.id, dic);

            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = instance.entity;
            viewInfo.prefabSourceId = instance.prefabSourceId;
            viewInfo.creationTick = instance.creationTick;
            this.rendering.Add(viewInfo);

            instance.OnInitialize(this.GetData(instance));

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegister(IView<TEntity> instance) {
            
            instance.OnDeInitialize(this.GetData(instance));
            
            List<IView<TEntity>> list;
            if (this.list.TryGetValue(instance.entity.id, out list) == true) {

                list.Remove(instance);

            }
            
            Dictionary<ViewId, List<IView<TEntity>>> dic;
            if (this.listDic.TryGetValue(instance.entity.id, out dic) == true) {

                if (dic.TryGetValue(instance.prefabSourceId, out list) == true) {

                    list.Remove(instance);

                }

            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = instance.entity;
            viewInfo.prefabSourceId = instance.prefabSourceId;
            this.rendering.Remove(viewInfo);
            
        }

        private TEntity GetData(IViewBase view) {
            
            TEntity data;
            if (this.world.GetEntityData(view.entity.id, out data) == true) {

                return data;

            }

            return default;

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {}

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool IsRenderingNow(ref ViewInfo viewInfo) {
            
            return this.rendering.Contains(viewInfo);
            /*
            Dictionary<ViewId, List<IView<TEntity>>> dic;
            if (this.listDic.TryGetValue(viewInfo.entity.id, out dic) == true) {

                List<IView<TEntity>> list;
                if (dic.TryGetValue(viewInfo.prefabSourceId, out list) == true) {

                    return list.Count > 0;

                }

            }

            return false;*/

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RestoreViews() {
            
            var aliveEntities = PoolDictionary<EntityId, byte>.Spawn(ViewsModule<TState, TEntity>.INTERNAL_ENTITIES_CACHE_CAPACITY);
            List<TEntity> allEntities;
            if (this.world.ForEachEntity(out allEntities) == true) {

                for (int j = 0, jCount = allEntities.Count; j < jCount; ++j) {

                    // For each entity in state
                    var item = allEntities[j];
                    aliveEntities.Add(item.entity.id, 0);

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
                            var dt = UnityEngine.Mathf.Max(0f, (this.world.GetTick() - component.viewInfo.creationTick) * this.world.GetTickTime());
                            instance.ApplyState(item, dt, immediately: true);
                            // Simulate particle systems
                            instance.SimulateParticles(dt);

                        }

                    }

                    PoolList<ViewComponent>.Recycle(ref components);

                }

            }

            // Iterate all current view instances
            // Search for views that doesn't represent any entity and destroy them
            foreach (var item in this.list) {

                if (aliveEntities.ContainsKey(item.Key) == false) {

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
            
            PoolDictionary<EntityId, byte>.Recycle(ref aliveEntities);

        }

        void IModule<TState>.Update(TState state, float deltaTime) {

            this.RestoreViews();

            foreach (var item in this.list) {

                var list = item.Value;
                for (int i = 0, count = list.Count; i < count; ++i) {
                
                    var instance = list[i];
                    instance.ApplyState(this.GetData(instance), deltaTime, immediately: false);
                
                }

            }

            // Update providers
            foreach (var providerKv in this.registryPrefabToProvider) {

                providerKv.Value.Update(this.list, deltaTime);

            }

        }

        public override string ToString() {

            var renderersCount = 0;
            foreach (var ren in this.list) {

                renderersCount += ren.Value.Count;

            }

            return "<b>Alive Views:</b> " + renderersCount.ToString() + ", <b>Entities Type:</b> " + typeof(TEntity).ToString();
            
        }

    }

}
#endif