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

    public abstract class View<TEntity> : UnityEngine.MonoBehaviour, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }

        public virtual void OnInitialize(TEntity data) { }
        public virtual void OnDeInitialize(TEntity data) { }
        public abstract void ApplyState(TEntity data, float deltaTime, bool immediately);

    }

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        void InstantiateView<TEntity>(View<TEntity> prefab, Entity entity) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(UnityEngine.GameObject prefab, Entity entity) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity;
        void DestroyView<TEntity>(ref View<TEntity> instance) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSource<TEntity>(View<TEntity> prefab) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(View<TEntity> prefab) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.RegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(UnityEngine.GameObject prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource(prefab.GetComponent<View<TEntity>>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(ViewId prefab, Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(View<TEntity> prefab, Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(UnityEngine.GameObject prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView(prefab.GetComponent<View<TEntity>>(), entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView<TEntity>(ref View<TEntity> instance) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.DestroyView(ref instance);
            
        }

    }

    public interface IViewModule<TState, TEntity> : IModule<TState> where TState : class, IState<TState> where TEntity : struct, IEntity {

        void Register(View<TEntity> instance);
        void UnRegister(View<TEntity> instance);

        ViewId RegisterViewSource(UnityEngine.GameObject prefab);
        ViewId RegisterViewSource(View<TEntity> prefab);
        bool UnRegisterViewSource(View<TEntity> prefab);
        
        void InstantiateView(UnityEngine.GameObject prefab, Entity entity);
        void InstantiateView(View<TEntity> prefab, Entity entity);
        void InstantiateView(ViewId prefabSourceId, Entity entity);
        void DestroyView(ref View<TEntity> instance);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class ViewsModule<TState, TEntity> : IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

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

        private List<View<TEntity>> list;
        private Dictionary<View<TEntity>, ViewId> registryPrefabToId;
        private Dictionary<ViewId, View<TEntity>> registryIdToPrefab;
        private ViewId viewSourceIdRegistry;
        private ViewId viewIdRegistry;
        
        public IWorld<TState> world { get; set; }

        void IModule<TState>.OnConstruct() {

            this.list = PoolList<View<TEntity>>.Spawn(ViewsModule<TState, TEntity>.VIEWS_CAPACITY);
            this.registryPrefabToId = PoolDictionary<View<TEntity>, ViewId>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);
            this.registryIdToPrefab = PoolDictionary<ViewId, View<TEntity>>.Spawn(ViewsModule<TState, TEntity>.REGISTRY_CAPACITY);

        }

        void IModule<TState>.OnDeconstruct() {
            
            PoolDictionary<ViewId, View<TEntity>>.Recycle(ref this.registryIdToPrefab);
            PoolDictionary<View<TEntity>, ViewId>.Recycle(ref this.registryPrefabToId);
            PoolList<View<TEntity>>.Recycle(ref this.list);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(UnityEngine.GameObject prefab, Entity entity) {
            
            var viewSource = prefab.GetComponent<View<TEntity>>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(View<TEntity> prefab, Entity entity) {
            
            this.InstantiateView(this.GetViewSourceId(prefab), entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ViewId sourceId, Entity entity) {

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false) {

                throw new OutOfStateException();

            }

            var viewInfo = new ViewInfo();
            viewInfo.entity = entity;
            viewInfo.prefabSourceId = sourceId;

            var component = this.world.AddComponent<TEntity, ViewComponent>(entity);
            component.viewInfo = viewInfo;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView(ref View<TEntity> instance) {

            // Called by tick system
            if (this.world.HasStep(WorldStep.LogicTick) == false) {

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
        private View<TEntity> SpawnView_INTERNAL(ViewInfo viewInfo) {
            
            var instance = PoolGameObject.Spawn(this.GetViewSource(viewInfo.prefabSourceId), viewInfo.prefabSourceId);
            instance.entity = viewInfo.entity;
            instance.prefabSourceId = viewInfo.prefabSourceId;
            this.Register(instance);

            return instance;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RecycleView_INTERNAL(ref View<TEntity> instance) {

            this.UnRegister(instance);
            PoolGameObject.Recycle(ref instance);

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
        public ViewId RegisterViewSource(UnityEngine.GameObject prefab) {

            return this.RegisterViewSource(prefab.GetComponent<View<TEntity>>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public View<TEntity> GetViewSource(ViewId viewSourceId) {

            View<TEntity> prefab;
            if (this.registryIdToPrefab.TryGetValue(viewSourceId, out prefab) == true) {

                return prefab;

            }

            return null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId GetViewSourceId(View<TEntity> prefab) {

            ViewId viewId;
            if (this.registryPrefabToId.TryGetValue(prefab, out viewId) == true) {

                return viewId;

            }

            return 0UL;

        }

        public ViewId RegisterViewSource(View<TEntity> prefab) {

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

        public bool UnRegisterViewSource(View<TEntity> prefab) {

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
        public void Register(View<TEntity> instance) {

            this.list.Add(instance);
            instance.OnInitialize(this.GetData(instance));

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegister(View<TEntity> instance) {
            
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
