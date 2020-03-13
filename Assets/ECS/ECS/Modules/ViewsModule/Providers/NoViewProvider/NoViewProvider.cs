using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    using ME.ECS.Views;
    using ME.ECS.Views.Providers;
    
    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

        ViewId RegisterViewSource<TEntity>(NoViewBase prefab) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(NoViewBase prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(NoViewBase prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource<TEntity, NoViewProviderInitializer<TEntity>>(new NoViewProviderInitializer<TEntity>(), (IView<TEntity>)prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(NoViewBase prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView((IView<TEntity>)prefab, entity);

        }

    }

}

namespace ME.ECS.Views.Providers {

    using ME.ECS;
    using ME.ECS.Views;

    public abstract class NoViewBase {

        public void SimulateParticles(float time, uint seed) {
            
        }

        public void UpdateParticlesSimulation(float deltaTime) {
            
        }

        public override string ToString() {

            return "NoView";
            
        }

    }

    public abstract class NoView<TEntity> : NoViewBase, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }
        public Tick creationTick { get; set; }

        public virtual void OnInitialize(in TEntity data) { }
        public virtual void OnDeInitialize(in TEntity data) { }
        public abstract void ApplyState(in TEntity data, float deltaTime, bool immediately);
        
    }
    
    public class NoViewProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        private PoolInternalBase pool;
        
        public override void OnConstruct() {

            this.pool = new PoolInternalBase(null, null);
            
        }

        public override void OnDeconstruct() {
            
            this.pool = null;
            
        }
        
        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            var prefabSource = (NoView<TEntity>)prefab;
            
            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }
            
            var particleViewBase = (NoView<TEntity>)obj;
            particleViewBase.entity = prefabSource.entity;
            particleViewBase.prefabSourceId = prefabSource.prefabSourceId;
            
            return (IView<TEntity>)obj;

        }

        public override void Destroy(ref IView<TEntity> instance) {

            this.pool.Recycle(instance);
            instance = null;
            
        }

    }

    public struct NoViewProviderInitializer<TEntity> : IViewsProviderInitializer<TEntity> where TEntity : struct, IEntity {

        public IViewsProvider<TEntity> Create() {

            return PoolClass<NoViewProvider<TEntity>>.Spawn();

        }

        public void Destroy(IViewsProvider<TEntity> instance) {

            PoolClass<NoViewProvider<TEntity>>.Recycle((NoViewProvider<TEntity>)instance);
            
        }

    }

}