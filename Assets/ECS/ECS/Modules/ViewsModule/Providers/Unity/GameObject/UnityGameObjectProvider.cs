#if GAMEOBJECT_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    using ME.ECS.Views;
    using ME.ECS.Views.Providers;
    
    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

        ViewId RegisterViewSource<TEntity>(UnityEngine.GameObject prefab) where TEntity : struct, IEntity;
        ViewId RegisterViewSource<TEntity>(MonoBehaviourViewBase prefab) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(UnityEngine.GameObject prefab, Entity entity) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(MonoBehaviourViewBase prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(UnityEngine.GameObject prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource(new UnityGameObjectProviderInitializer<TEntity>(), prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(UnityGameObjectProviderInitializer<TEntity> providerInitializer, UnityEngine.GameObject prefab) where TEntity : struct, IEntity {

            IView<TEntity> component;
            if (prefab.TryGetComponent(out component) == true) {

                return this.RegisterViewSource(providerInitializer, component);

            }

            return 0UL;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(MonoBehaviourViewBase prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource(new UnityGameObjectProviderInitializer<TEntity>(), (IView<TEntity>)prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(UnityEngine.GameObject prefab, Entity entity) where TEntity : struct, IEntity {

            IView<TEntity> component;
            if (prefab.TryGetComponent(out component) == true) {

                this.InstantiateView(component, entity);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(MonoBehaviourViewBase prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView((IView<TEntity>)prefab, entity);

        }

    }

}

namespace ME.ECS.Views {

    using ME.ECS.Views.Providers;

    public partial interface IViewModule<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        ViewId RegisterViewSource(UnityEngine.GameObject prefab);
        void UnRegisterViewSource(UnityEngine.GameObject prefab);
        void InstantiateView(UnityEngine.GameObject prefab, Entity entity);

    }

    public partial class ViewsModule<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(UnityEngine.GameObject prefab) {

            return this.RegisterViewSource(new UnityGameObjectProviderInitializer<TEntity>(), prefab.GetComponent<MonoBehaviourView<TEntity>>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(UnityEngine.GameObject prefab) {

            this.UnRegisterViewSource(prefab.GetComponent<MonoBehaviourView<TEntity>>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(UnityEngine.GameObject prefab, Entity entity) {
            
            var viewSource = prefab.GetComponent<MonoBehaviourView<TEntity>>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

}

namespace ME.ECS.Views.Providers {

    using ME.ECS;
    using ME.ECS.Views;

    public abstract class MonoBehaviourViewBase : UnityEngine.MonoBehaviour, IDoValidate {

        public ParticleSystemSimulation particleSystemSimulation;

        public void SimulateParticles(float time, uint seed) {
            
            this.particleSystemSimulation.SimulateParticles(time, seed);
            
        }

        public void UpdateParticlesSimulation(float deltaTime) {
            
            this.particleSystemSimulation.Update(deltaTime);
            
        }

        [UnityEngine.ContextMenu("Validate")]
        public void DoValidate() {
            
            this.particleSystemSimulation.OnValidate(this.GetComponentsInChildren<UnityEngine.ParticleSystem>(true));
            
        }

        public void OnValidate() {
            
            this.DoValidate();
            
        }

        public override string ToString() {

            return this.particleSystemSimulation.ToString();
            
        }

    }

    public abstract class MonoBehaviourView<TEntity> : MonoBehaviourViewBase, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }
        public Tick creationTick { get; set; }

        public virtual void OnInitialize(in TEntity data) { }
        public virtual void OnDeInitialize(in TEntity data) { }
        public abstract void ApplyState(in TEntity data, float deltaTime, bool immediately);
        
    }
    
    public class UnityGameObjectProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        private PoolGameObject<MonoBehaviourView<TEntity>> pool;
        
        public override void OnConstruct() {

            this.pool = new PoolGameObject<MonoBehaviourView<TEntity>>();
            
        }

        public override void OnDeconstruct() {

            this.pool = null;

        }
        
        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            return this.pool.Spawn((MonoBehaviourView<TEntity>)prefab, prefabSourceId);

        }

        public override void Destroy(ref IView<TEntity> instance) {

            var instanceTyped = (MonoBehaviourView<TEntity>)instance;
            this.pool.Recycle(ref instanceTyped);
            instance = null;

        }

    }

    public struct UnityGameObjectProviderInitializer<TEntity> : IViewsProviderInitializer<TEntity> where TEntity : struct, IEntity {

        public IViewsProvider<TEntity> Create() {
            
            return PoolClass<UnityGameObjectProvider<TEntity>>.Spawn();

        }

        public void Destroy(IViewsProvider<TEntity> instance) {

            PoolClass<UnityGameObjectProvider<TEntity>>.Recycle((UnityGameObjectProvider<TEntity>)instance);
            
        }

    }

}
#endif