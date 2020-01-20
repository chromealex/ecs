#if PARTICLES_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        ViewId RegisterViewSource<TEntity>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(ParticleViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(ParticleViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView(prefab.GetSource<TEntity>(), entity);
            
        }

    }

    public abstract class ParticleViewBase {

        public UnityEngine.ParticleSystem.Particle particleData;
        public ParticleSystemItem itemData;
        
    }

    public abstract class ParticleView<TEntity> : ParticleViewBase, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }

        public virtual void OnInitialize(in TEntity data) { }
        public virtual void OnDeInitialize(in TEntity data) { }
        public abstract void ApplyState(in TEntity data, float deltaTime, bool immediately);

    }
    
    public partial interface IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        ViewId RegisterViewSource(ParticleViewSourceBase prefab);
        void InstantiateView(ParticleViewSourceBase prefab, Entity entity);

    }

    public partial class ViewsModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(ParticleViewSourceBase prefab) {

            return this.RegisterViewSource(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ParticleViewSourceBase prefab, Entity entity) {
            
            var viewSource = prefab.GetSource<TEntity>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

    [System.Serializable]
    public struct ParticleSystemItem {

        public UnityEngine.Mesh mesh;
        public UnityEngine.Material material;
        public UnityEngine.ParticleSystem ps;

    }

    public class UnityParticlesProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        private System.Collections.Generic.Dictionary<long, ParticleSystemItem> psItems = new System.Collections.Generic.Dictionary<long, ParticleSystemItem>();
        private PoolInternalBase pool = new PoolInternalBase(null, null);

        private ParticleSystemItem Validate(IView<TEntity> prefab) {

            // Create PS if doesn't exists
            var source = (ParticleViewBase)prefab;
            var key = MathUtils.GetKey(source.itemData.material.GetInstanceID(), source.itemData.mesh.GetInstanceID());
            ParticleSystemItem psItem;
            if (this.psItems.TryGetValue(key, out psItem) == false) {
                
                psItem = source.itemData;
                
                var particleSystem = new UnityEngine.GameObject("PS-Render", typeof(UnityEngine.ParticleSystem)).GetComponent<UnityEngine.ParticleSystem>();
                var emission = particleSystem.emission;
                emission.enabled = false;
                var shape = particleSystem.shape;
                shape.enabled = false;

                var particleSystemRenderer = particleSystem.GetComponent<UnityEngine.ParticleSystemRenderer>();
                particleSystemRenderer.alignment = UnityEngine.ParticleSystemRenderSpace.World;
                particleSystemRenderer.renderMode = UnityEngine.ParticleSystemRenderMode.Mesh;
                particleSystemRenderer.sharedMaterial = psItem.material;
                particleSystemRenderer.mesh = psItem.mesh;

                psItem.ps = particleSystem;
                this.psItems.Add(key, psItem);
                
            }

            return psItem;

        }

        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            var psItem = this.Validate(prefab);
            psItem.ps.Emit(1);
            
            var obj = this.pool.Spawn();
            if (obj == null) obj = System.Activator.CreateInstance(prefab.GetType());
            
            return (IView<TEntity>)obj;

        }

        public override void Destroy(ref IView<TEntity> instance) {

            this.pool.Recycle(instance);
            instance = null;

        }

    }

    public class UnityParticlesProvider : IViewsProvider {

        public void RegisterEntityType<TState, TEntity>(IViewModule<TState, TEntity> module) where TState : class, IState<TState> where TEntity : struct, IEntity {

            module.RegisterProvider<UnityParticlesProvider<TEntity>>();

        }

    }

}
#endif