#if PARTICLES_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {
    
    using ME.ECS.Views;
    using ME.ECS.Views.Providers;

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

}

namespace ME.ECS.Views {
    
    using ME.ECS.Views.Providers;
    
    public partial interface IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        ViewId RegisterViewSource(ParticleViewSourceBase prefab);
        void UnRegisterViewSource(ParticleViewSourceBase prefab);
        void InstantiateView(ParticleViewSourceBase prefab, Entity entity);

    }

    public partial class ViewsModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(ParticleViewSourceBase prefab) {

            return this.RegisterViewSource<UnityParticlesProvider>(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(ParticleViewSourceBase prefab) {
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ParticleViewSourceBase prefab, Entity entity) {
            
            var viewSource = prefab.GetSource<TEntity>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

}

namespace ME.ECS.Views.Providers {

    public abstract class ParticleViewBase : IPoolableRecycle {

        [System.Serializable]
        public struct Item {

            public UnityEngine.ParticleSystem.Particle particleData;
            public ParticleSystemItem itemData;

        }

        public Item[] items;

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<ParticleViewBase.Item>.Recycle(ref this.items);
            
        }

        public override string ToString() {
            
            return "Renderers Count: " + this.items.Length.ToString();
            
        }

        public UnityEngine.ParticleSystem.Particle rootData {
            
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {

                return this.items[0].particleData;

            }
            
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            set {

                this.items[0].particleData = value;
                for (int i = 1; i < this.items.Length; ++i) {

                    ref var item = ref this.items[i];
                    item.particleData.position = value.position + item.itemData.localPosition;
                    item.particleData.rotation3D = value.rotation3D + item.itemData.localRotation;
                    item.particleData.startSize3D = UnityEngine.Vector3.Scale(value.startSize3D, item.itemData.localScale);

                }

            }
        }

        public void SetItems(UnityEngine.Vector3 rootPosition, UnityEngine.Vector3 rootRotation, UnityEngine.Vector3 rootScale, UnityEngine.MeshFilter[] filters, UnityEngine.Renderer[] renderers) {
            
            this.items = new Item[filters.Length];
            for (int i = 0; i < filters.Length; ++i) {

                var tr = filters[i].transform;
                
                var item = new Item();
                var itemData = new ParticleSystemItem();
                itemData.material = renderers[i].sharedMaterial;
                itemData.mesh = filters[i].sharedMesh;
                itemData.meshFilter = filters[i];
                itemData.localPosition = (i == 0 ? rootPosition : tr.position - rootPosition);
                itemData.localRotation = (i == 0 ? rootRotation : tr.rotation.eulerAngles - rootRotation);
                itemData.localScale = (i == 0 ? rootScale : UnityEngine.Vector3.Scale(tr.lossyScale, new UnityEngine.Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z)));
                item.itemData = itemData;
                this.items[i] = item;

            }

        }

        public void SimulateParticles(float time) {
            
        }

    }

    public abstract class ParticleView<TEntity> : ParticleViewBase, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }

        public virtual void OnInitialize(in TEntity data) { }
        public virtual void OnDeInitialize(in TEntity data) { }
        public abstract void ApplyState(in TEntity data, float deltaTime, bool immediately);

    }
    
    [System.Serializable]
    public struct ParticleSystemItem {

        public UnityEngine.Mesh mesh;
        public UnityEngine.MeshFilter meshFilter;
        public UnityEngine.Material material;
        public UnityEngine.Vector3 localPosition;
        public UnityEngine.Vector3 localRotation;
        public UnityEngine.Vector3 localScale;
        
        // runtime
        [System.NonSerialized]
        public UnityEngine.ParticleSystem ps;
        [System.NonSerialized]
        public UnityEngine.ParticleSystemRenderer psRenderer;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public long GetKey() {
            
            return MathUtils.GetKey(this.material.GetInstanceID(), (this.mesh != null ? this.mesh.GetInstanceID() : this.meshFilter.GetInstanceID()));
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public UnityEngine.Mesh GetMesh() {

            return (this.mesh != null ? this.mesh : this.meshFilter.sharedMesh);

        }

    }

    public class UnityParticlesProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        private System.Collections.Generic.Dictionary<long, ParticleSystemItem> psItems;
        private PoolInternalBase pool;
        private UnityEngine.ParticleSystem.Particle[] particles;
        private int maxParticles = 1000;

        public override void OnConstruct() {

            this.psItems = PoolDictionary<long, ParticleSystemItem>.Spawn(100);
            this.pool = new PoolInternalBase(null, null);
            
        }

        public override void OnDeconstruct() {

            this.pool = null;
            PoolDictionary<long, ParticleSystemItem>.Recycle(ref this.psItems);
            
        }

        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }

            var prefabSource = (ParticleView<TEntity>)prefab;
            var particleViewBase = (ParticleView<TEntity>)obj;
            particleViewBase.items = PoolArray<ParticleViewBase.Item>.Spawn(prefabSource.items.Length);
            for (int i = 0; i < particleViewBase.items.Length; ++i) {

                particleViewBase.items[i] = prefabSource.items[i];

            }
            particleViewBase.entity = prefabSource.entity;
            particleViewBase.prefabSourceId = prefabSource.prefabSourceId;
            
            var maxParticleCount = 0;
            long key;
            // Create PS if doesn't exists
            for (int i = 0; i < prefabSource.items.Length; ++i) {

                maxParticleCount = 0;
                ref var source = ref prefabSource.items[i];
                key = source.itemData.GetKey();
                ParticleSystemItem psItem;
                if (this.psItems.TryGetValue(key, out psItem) == false) {
                    
                    psItem = source.itemData;
                    
                    var particleSystem = new UnityEngine.GameObject("PS-Render-" + key.ToString(), typeof(UnityEngine.ParticleSystem)).GetComponent<UnityEngine.ParticleSystem>();
                    //particleSystem.gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
                    particleSystem.Stop();
                    
                    var main = particleSystem.main;
                    main.duration = float.MaxValue;
                    main.loop = false;
                    main.prewarm = false;
                    main.playOnAwake = false;
                    main.startLifetime = float.MaxValue;
                    main.startSpeed = 0f;
                    main.maxParticles = int.MaxValue;
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
                    psItem.psRenderer = particleSystemRenderer;
                    this.psItems.Add(key, psItem);
                    
                    particleSystem.Play();
                    
                }

                psItem.ps.Emit(1);
                if (psItem.ps.particleCount > maxParticleCount) maxParticleCount = psItem.ps.particleCount;

                particleViewBase.items[i].itemData = psItem;

            }
            
            if (maxParticleCount > this.maxParticles) this.maxParticles = maxParticleCount;

            return (IView<TEntity>)obj;

        }

        public override void Destroy(ref IView<TEntity> instance) {

            var view = (ParticleViewBase)instance;
            for (int i = 0; i < view.items.Length; ++i) {

                view.items[i].particleData.startSize = 0f;

            }

            this.pool.Recycle(instance);
            instance = null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ValidateParticles() {

            if (this.particles == null || this.particles.Length < this.maxParticles) {
                
                this.particles = new UnityEngine.ParticleSystem.Particle[this.maxParticles];
                
            }

        }

        public override void Update(System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<IView<TEntity>>> list, float deltaTime) {
            
            this.ValidateParticles();
            
            foreach (var item in this.psItems) {
                
                var k = 0;
                var psItem = item.Value;
                var ps = psItem.ps;
                psItem.psRenderer.mesh = psItem.GetMesh();
                //ps.GetParticles(this.particles, this.maxParticles);
                foreach (var itemView in list) {

                    var itemsList = itemView.Value;
                    var count = itemsList.Count;
                    for (int i = 0; i < count; ++i) {
                        
                        var view = (ParticleViewBase)itemsList[i];
                        for (int j = 0; j < view.items.Length; ++j) {
                        
                            ref var element = ref view.items[j];
                            if (element.itemData.ps == ps) {

                                element.particleData.remainingLifetime = float.MaxValue;
                                element.particleData.startLifetime = float.MaxValue;
                                this.particles[k++] = element.particleData;

                            }

                        }
                    
                    }
                }

                ps.SetParticles(this.particles, k, 0);
                
            }
            
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