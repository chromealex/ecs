#if PARTICLES_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        ViewId RegisterViewSource<TEntity>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity;
        ViewId RegisterViewSource<TEntity, TProvider>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProvider;
        void InstantiateView<TEntity>(ParticleViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity, TProvider>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProvider {

            return this.RegisterViewSource<TEntity, TProvider>(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource<TEntity, UnityParticlesProvider>(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(ParticleViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView(prefab.GetSource<TEntity>(), entity);
            
        }

    }

    public abstract class ParticleViewBase {

        public UnityEngine.ParticleSystem.Particle particleData;
        public int particleIndex;
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

            return this.RegisterViewSource<UnityParticlesProvider>(prefab.GetSource<TEntity>());

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
        
        // runtime
        public UnityEngine.ParticleSystem ps;
        public int index;

    }

    public class UnityParticlesProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        private readonly System.Collections.Generic.Dictionary<long, ParticleSystemItem> psItems = new System.Collections.Generic.Dictionary<long, ParticleSystemItem>();
        private readonly PoolInternalBase pool = new PoolInternalBase(null, null);
        private UnityEngine.ParticleSystem.Particle[] particles;
        private int maxParticles = 1000;

        private ParticleSystemItem Validate(IView<TEntity> prefab, out long key) {

            // Create PS if doesn't exists
            var source = (ParticleViewBase)prefab;
            key = MathUtils.GetKey(source.itemData.material.GetInstanceID(), source.itemData.mesh.GetInstanceID());
            ParticleSystemItem psItem;
            if (this.psItems.TryGetValue(key, out psItem) == false) {
                
                psItem = source.itemData;
                
                var particleSystem = new UnityEngine.GameObject("PS-Render", typeof(UnityEngine.ParticleSystem)).GetComponent<UnityEngine.ParticleSystem>();
                particleSystem.Stop();
                
                var main = particleSystem.main;
                main.duration = float.MaxValue;
                main.loop = false;
                main.prewarm = false;
                main.playOnAwake = false;
                main.startLifetime = float.MaxValue;
                main.startSpeed = 0f;
                main.maxParticles = 100000;
                main.ringBufferMode = UnityEngine.ParticleSystemRingBufferMode.PauseUntilReplaced;
                main.simulationSpace = UnityEngine.ParticleSystemSimulationSpace.World;
                
                var trigger = particleSystem.trigger;
                trigger.enabled = true;
                trigger.inside = UnityEngine.ParticleSystemOverlapAction.Ignore;

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
                
                particleSystem.Play();
                
            }

            return psItem;

        }

        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            long key;
            var psItem = this.Validate(prefab, out key);
            psItem.ps.Emit(1);
            var particleIndex = psItem.index;//psItem.ps.particleCount - 1;
            ++psItem.index;
            this.psItems[key] = psItem;

            if (psItem.ps.particleCount > this.maxParticles) this.maxParticles = psItem.ps.particleCount;
            
            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = (ParticleViewBase)System.Activator.CreateInstance(prefab.GetType());
                
            }

            var particleViewBase = (ParticleViewBase)obj;
            particleViewBase.particleIndex = particleIndex;
            particleViewBase.itemData = psItem;
            
            return (IView<TEntity>)obj;

        }

        public override void Destroy(ref IView<TEntity> instance) {

            var view = (ParticleViewBase)instance;
            view.particleData.startSize = 0f;
            
            this.pool.Recycle(instance);
            instance = null;

        }

        private void ValidateParticles() {

            if (this.particles == null || this.particles.Length < this.maxParticles) {
                
                this.particles = new UnityEngine.ParticleSystem.Particle[this.maxParticles];
                
            }

        }

        public override void Update(System.Collections.Generic.List<IView<TEntity>> list, float deltaTime) {

            this.ValidateParticles();

            var count = list.Count;
            foreach (var item in this.psItems) {

                var psItem = item.Value;
                var ps = psItem.ps;
                //ps.GetParticles(this.particles, this.maxParticles);
                for (int i = 0; i < count; ++i) {

                    var view = (ParticleViewBase)list[i];
                    view.particleData.remainingLifetime = float.MaxValue;
                    view.particleData.startLifetime = float.MaxValue;
                    this.particles[i] = view.particleData;

                }
                ps.SetParticles(this.particles, count, 0);

            }

            /*
            for (int i = 0, count = list.Count; i < count; ++i) {

                var particleViewBase = (ParticleViewBase)list[i];
                var ps = particleViewBase.itemData.ps;
                if (ps.isPlaying == false) ps.Play();
                var particlesCount = ps.GetParticles(this.particles, this.maxParticles);
                particleViewBase.particleData.remainingLifetime = float.MaxValue;
                particleViewBase.particleData.startLifetime = float.MaxValue;
                this.particles[particleViewBase.particleIndex] = particleViewBase.particleData;
                ps.SetParticles(this.particles, particlesCount, 0);

            }*/

        }

    }

    public struct UnityParticlesProvider : IViewsProvider {

        public IViewsProvider<TEntity> Create<TEntity>() where TEntity : struct, IEntity {

            return PoolClass<UnityParticlesProvider<TEntity>>.Spawn();

        }

        public void Destroy<TEntity>(IViewsProvider<TEntity> instance) where TEntity : struct, IEntity {

            PoolClass<UnityParticlesProvider<TEntity>>.Recycle((UnityParticlesProvider<TEntity>)instance);

        }

    }

}
#endif