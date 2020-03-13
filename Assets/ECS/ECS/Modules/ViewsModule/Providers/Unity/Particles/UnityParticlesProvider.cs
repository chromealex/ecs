#if PARTICLES_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {
    
    using ME.ECS.Views;
    using ME.ECS.Views.Providers;

    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

        ViewId RegisterViewSource<TEntity>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(ParticleViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(ParticleViewSourceBase prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource(new UnityParticlesProviderInitializer<TEntity>(), prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(ParticleViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView(prefab.GetSource<TEntity>(), entity);
            
        }

    }

}

namespace ME.ECS.Views {
    
    using ME.ECS.Views.Providers;
    
    public partial interface IViewModule<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {

        ViewId RegisterViewSource(ParticleViewSourceBase prefab);
        void UnRegisterViewSource(ParticleViewSourceBase prefab);
        void InstantiateView(ParticleViewSourceBase prefab, Entity entity);

    }

    public partial class ViewsModule<TState, TEntity> where TState : class, IState<TState>, new() where TEntity : struct, IEntity {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(ParticleViewSourceBase prefab) {

            return this.RegisterViewSource(new UnityParticlesProviderInitializer<TEntity>(), prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(ParticleViewSourceBase prefab) {
            
            this.UnRegisterViewSource(prefab.GetSource<TEntity>());
            
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

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref UnityEngine.ParticleSystem.Particle GetRootData() {

            return ref this.items[0].particleData;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetRootData(ref UnityEngine.ParticleSystem.Particle data) {

            this.items[0].particleData = data;
            for (int i = 1; i < this.items.Length; ++i) {

                ref var item = ref this.items[i];
                item.particleData.position = data.position + item.itemData.localPosition;
                item.particleData.rotation3D = data.rotation3D + item.itemData.localRotation;
                item.particleData.startSize3D = UnityEngine.Vector3.Scale(data.startSize3D, item.itemData.localScale);

            }
            
        }

        public void OnValidate(
            UnityEngine.Vector3 rootPosition,
            UnityEngine.Vector3 rootRotation,
            UnityEngine.Vector3 rootScale,
            UnityEngine.MeshFilter[] filters,
            UnityEngine.ParticleSystem[] particleSystems,
            bool reset) {

            if (this.items.Length != filters.Length + particleSystems.Length) {

                reset = true;

            }

            if (reset == true) {
                
                this.items = new Item[0];
                
            }

            if (reset == true) {

                var itemsList = new System.Collections.Generic.List<Item>();
                for (int i = 0; i < filters.Length; ++i) {

                    var tr = filters[i].transform;

                    var item = new Item();
                    var itemData = new ParticleSystemItem();
                    itemData.material = tr.GetComponent<UnityEngine.Renderer>().sharedMaterial;
                    itemData.mesh = filters[i].sharedMesh;
                    itemData.meshFilter = filters[i];
                    itemData.localPosition = tr.position - rootPosition;
                    itemData.localRotation = tr.rotation.eulerAngles - rootRotation;
                    itemData.localScale = UnityEngine.Vector3.Scale(tr.lossyScale, new UnityEngine.Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z));
                    item.itemData = itemData;
                    itemsList.Add(item);

                }

                for (int i = 0; i < particleSystems.Length; ++i) {

                    var tr = particleSystems[i].transform;

                    var item = new Item();
                    var itemData = new ParticleSystemItem();

                    itemData.psSimulation = new ParticleSystemSimulationItem();
                    itemData.psSimulation.OnValidate(particleSystems[i]);
                    itemData.psSource = particleSystems[i];
                    itemData.localPosition = tr.position - rootPosition;
                    itemData.localRotation = tr.rotation.eulerAngles - rootRotation;
                    itemData.localScale = UnityEngine.Vector3.Scale(tr.lossyScale, new UnityEngine.Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z));

                    item.itemData = itemData;
                    itemsList.Add(item);

                }

                this.items = itemsList.ToArray();

            }
            
        }

        public void SimulateParticles(float time, uint seed) {

            for (int i = 0; i < this.items.Length; ++i) {

                this.items[i].itemData.SimulateParticles(time, seed);

            }

        }

        public void UpdateParticlesSimulation(float deltaTime) {
            
            for (int i = 0; i < this.items.Length; ++i) {

                this.items[i].itemData.UpdateParticlesSimulation(deltaTime);

            }
            
        }

    }

    public abstract class ParticleView<TEntity> : ParticleViewBase, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }
        public Tick creationTick { get; set; }

        public virtual void OnInitialize(in TEntity data) { }
        public virtual void OnDeInitialize(in TEntity data) { }
        public abstract void ApplyState(in TEntity data, float deltaTime, bool immediately);

    }
    
    [System.Serializable]
    public struct ParticleSystemItem {

        public ParticleSystemSimulationItem psSimulation;
        public UnityEngine.ParticleSystem psSource;
        public UnityEngine.Mesh mesh;
        public UnityEngine.MeshFilter meshFilter;
        public UnityEngine.Material material;
        public UnityEngine.Vector3 localPosition;
        public UnityEngine.Vector3 localRotation;
        public UnityEngine.Vector3 localScale;
        
        // runtime
        [System.NonSerialized]
        public bool simulation;
        [System.NonSerialized]
        public int r;
        [System.NonSerialized]
        public float startLifetime;
        [System.NonSerialized]
        public float lifetime;
        [System.NonSerialized]
        public int subEmitterIdx;
        [System.NonSerialized]
        public int subEmitterIdxInheritedLifetime;
        [System.NonSerialized]
        public UnityEngine.ParticleSystem ps;
        [System.NonSerialized]
        public UnityEngine.ParticleSystemRenderer psRenderer;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public long GetKey() {

            if (this.psSource != null) {

                return this.psSource.GetInstanceID();

            }

            return MathUtils.GetKey(this.material.GetInstanceID(), (this.mesh != null ? this.mesh.GetInstanceID() : this.meshFilter.GetInstanceID()));
            
        }

        public void SimulateParticles(float time, uint seed) {

            if (this.psSource != null) {

                this.psSimulation.SetAsCustomLifetime();
                this.psSimulation.SimulateParticles(time, seed);
                this.lifetime = this.startLifetime - this.psSimulation.GetCustomLifetime();
                this.Resimulate();

                if (time > this.psSimulation.settings.minSimulationTime) {

                    this.simulation = true;

                } else {
                    
                    this.simulation = false;
                    
                }

            }

        }

        public void UpdateParticlesSimulation(float deltaTime) {

            if (this.psSource != null) {

                if (this.psSimulation.Update(deltaTime) == true) {

                    this.lifetime = this.startLifetime - this.psSimulation.GetCustomLifetime();
                    this.Resimulate();
                    
                }

            }
            
        }

        private void Resimulate() {

            if (this.r >= 1) {

                var childPs = this.psSimulation.particleItem;
                childPs.particleSystem.Clear();
                this.r = 0;

            }

        }

    }

    public class UnityParticlesProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        private System.Collections.Generic.Dictionary<long, ParticleSystemItem> psItems;
        private PoolInternalBase pool;
        private UnityEngine.ParticleSystem.Particle[] particles;
        private UnityEngine.ParticleSystem.Particle[] particlesStatic;
        private int maxParticles = 1000;

        private UnityEngine.ParticleSystem mainParticleSystem;

        public override void OnConstruct() {

            this.psItems = PoolDictionary<long, ParticleSystemItem>.Spawn(100);
            this.pool = new PoolInternalBase(null, null);
            
            UnityEngine.ParticleSystem particleSystem;
            UnityEngine.ParticleSystemRenderer particleSystemRenderer;
            this.CreateParticleSystemInstance("Main-" + this.GetType().ToString(), out particleSystem, out particleSystemRenderer);

            var mainModule = particleSystem.main;
            mainModule.startSize = 1f;

            var subEmittersModule = particleSystem.subEmitters;
            subEmittersModule.enabled = true;

            particleSystemRenderer.enabled = false;
            particleSystemRenderer.renderMode = UnityEngine.ParticleSystemRenderMode.Billboard;

            this.mainParticleSystem = particleSystem;
            this.mainParticleSystem.Play();

        }

        public override void OnDeconstruct() {

            UnityEngine.Object.Destroy(this.mainParticleSystem);
            this.mainParticleSystem = null;
            
            this.pool = null;
            PoolDictionary<long, ParticleSystemItem>.Recycle(ref this.psItems);
            
        }

        private void CreateParticleSystemInstance(string key, out UnityEngine.ParticleSystem particleSystem, out UnityEngine.ParticleSystemRenderer particleSystemRenderer) {
            
            particleSystem = new UnityEngine.GameObject("PS-Render-" + key, typeof(UnityEngine.ParticleSystem)).GetComponent<UnityEngine.ParticleSystem>();
            //particleSystem.gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            particleSystem.Stop(withChildren: true);
            particleSystem.Pause(withChildren: true);
            particleSystem.useAutoRandomSeed = false;
            particleSystem.randomSeed = 1u;
            
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
                    
            var emission = particleSystem.emission;
            emission.enabled = false;
                    
            var shape = particleSystem.shape;
            shape.enabled = false;
            
            particleSystemRenderer = particleSystem.GetComponent<UnityEngine.ParticleSystemRenderer>();
            particleSystemRenderer.alignment = UnityEngine.ParticleSystemRenderSpace.World;
                    
            //particleSystem.Play(true);

        }
        
        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            var prefabSource = (ParticleView<TEntity>)prefab;
            
            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }

            var particleViewBase = (ParticleView<TEntity>)obj;
            particleViewBase.items = PoolArray<ParticleViewBase.Item>.Spawn(prefabSource.items.Length);
            for (int i = 0; i < particleViewBase.items.Length; ++i) {

                particleViewBase.items[i] = prefabSource.items[i];

            }
            
            particleViewBase.entity = prefabSource.entity;
            particleViewBase.prefabSourceId = prefabSource.prefabSourceId;

            var maxParticleCount = 0;
            long key;
            // Create PS if doesn't exist
            for (int i = 0; i < prefabSource.items.Length; ++i) {

                maxParticleCount = 0;
                ref var source = ref prefabSource.items[i];
                key = source.itemData.GetKey();
                ParticleSystemItem psItem;
                if (this.psItems.TryGetValue(key, out psItem) == false) {
                    
                    psItem = source.itemData;

                    var subEmittersModule = this.mainParticleSystem.subEmitters;

                    var idx = -1;
                    var idxInheritedLifetime = -1;
                    UnityEngine.ParticleSystem particleSystem;
                    if (psItem.psSource != null) {

                        particleSystem = UnityEngine.ParticleSystem.Instantiate(psItem.psSource, this.mainParticleSystem.transform);

                        idx = subEmittersModule.subEmittersCount;
                        subEmittersModule.AddSubEmitter(
                            particleSystem,
                            UnityEngine.ParticleSystemSubEmitterType.Manual,
                            UnityEngine.ParticleSystemSubEmitterProperties.InheritNothing
                        );

                        var particleSystemInheritLifetime = UnityEngine.ParticleSystem.Instantiate(psItem.psSource, this.mainParticleSystem.transform);
                        
                        idxInheritedLifetime = subEmittersModule.subEmittersCount;
                        subEmittersModule.AddSubEmitter(
                            particleSystemInheritLifetime,
                            UnityEngine.ParticleSystemSubEmitterType.Manual,
                            UnityEngine.ParticleSystemSubEmitterProperties.InheritLifetime
                        );

                        psItem.ps = particleSystem;
                        psItem.psSimulation.particleItem.particleSystem = particleSystem;

                    } else {

                        UnityEngine.ParticleSystemRenderer particleSystemRenderer;
                        this.CreateParticleSystemInstance(key.ToString(), out particleSystem, out particleSystemRenderer);
                        
                        particleSystemRenderer.alignment = UnityEngine.ParticleSystemRenderSpace.World;
                        particleSystemRenderer.renderMode = UnityEngine.ParticleSystemRenderMode.Mesh;
                        particleSystemRenderer.sharedMaterial = psItem.material;
                        particleSystemRenderer.mesh = psItem.mesh;

                        /*particleSystem.transform.SetParent(this.mainParticleSystem.transform);
                        
                        var emissionModule = particleSystem.emission;
                        emissionModule.enabled = true;
                        emissionModule.rateOverTime = new UnityEngine.ParticleSystem.MinMaxCurve(0f);
                        emissionModule.rateOverDistance = new UnityEngine.ParticleSystem.MinMaxCurve(0f);
                        emissionModule.burstCount = 1;
                        emissionModule.SetBurst(0, new UnityEngine.ParticleSystem.Burst(0f, 1));

                        idx = subEmittersModule.subEmittersCount;
                        subEmittersModule.AddSubEmitter(
                            particleSystem,
                            UnityEngine.ParticleSystemSubEmitterType.Manual,
                            UnityEngine.ParticleSystemSubEmitterProperties.InheritNothing);*/
                        
                        psItem.psRenderer = particleSystemRenderer;

                        psItem.ps = particleSystem;

                    }

                    psItem.subEmitterIdx = idx;
                    psItem.subEmitterIdxInheritedLifetime = idxInheritedLifetime;
                    this.psItems.Add(key, psItem);
                    
                }

                if (psItem.psSource != null) {
                    
                    psItem.lifetime = psItem.ps.main.startLifetime.constantMax;
                    psItem.startLifetime = psItem.lifetime;
                    
                    this.mainParticleSystem.Emit(1);

                } else {

                    psItem.lifetime = -1f;

                    psItem.ps.Emit(1);

                }
                
                psItem.ps.Play();

                psItem.r = 0;
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

            if (this.particlesStatic == null || this.particlesStatic.Length < this.maxParticles) {
                
                this.particlesStatic = new UnityEngine.ParticleSystem.Particle[this.maxParticles];
                
            }

        }

        public override void Update(System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<IView<TEntity>>> list, float deltaTime) {
            
            this.ValidateParticles();
            
            var k = 0;
            //var kSize = this.mainParticleSystem.GetParticles(this.particles);
            foreach (var item in this.psItems) {

                var staticK = 0;
                var dynamicK = 0;
                var psItem = item.Value;
                var ps = psItem.ps;
                //psItem.psRenderer.mesh = psItem.GetMesh();
                //ps.GetParticles(this.particles, this.maxParticles);
                foreach (var itemView in list) {

                    var itemsList = itemView.Value;
                    var count = itemsList.Count;
                    for (int i = 0; i < count; ++i) {
                        
                        var view = (ParticleViewBase)itemsList[i];
                        for (int j = 0; j < view.items.Length; ++j) {
                        
                            ref var element = ref view.items[j];
                            if (element.itemData.ps == ps) {

                                if (element.itemData.lifetime > 0f) {

                                    // Sub particle system effect
                                    
                                    element.itemData.lifetime -= deltaTime;
                                    if (element.itemData.lifetime <= 0f) {

                                        // we should never render this element
                                        element.itemData.lifetime = UnityEngine.Mathf.Epsilon;
                                        continue;

                                    }

                                    element.particleData.remainingLifetime = element.itemData.lifetime;
                                    element.particleData.startLifetime = element.itemData.startLifetime;
                                    element.particleData.startSize = 1f;
                                    this.particles[k] = element.particleData;
                                    
                                    // Just trigger sub system
                                    if (element.itemData.r == 1) {

                                        if (element.itemData.simulation == true) {
                                            
                                            this.particles[k].remainingLifetime = element.itemData.lifetime / element.itemData.startLifetime;
                                            this.mainParticleSystem.TriggerSubEmitter(element.itemData.subEmitterIdxInheritedLifetime, ref this.particles[k]);
                                            
                                        } else {

                                            this.mainParticleSystem.TriggerSubEmitter(element.itemData.subEmitterIdx, ref this.particles[k]);

                                        }

                                    }
                                    ++element.itemData.r;

                                    ++k;
                                    ++dynamicK;

                                } else {
                                    
                                    // Static mesh with material
                                    element.particleData.remainingLifetime = float.MaxValue;
                                    element.particleData.startLifetime = float.MaxValue;

                                    this.particlesStatic[staticK] = element.particleData;
                                    ++staticK;

                                }

                            }

                        }

                    }
                    
                }
                
                if (staticK > 0 || dynamicK == 0) ps.SetParticles(this.particlesStatic, staticK, 0);

            }

            this.mainParticleSystem.SetParticles(this.particles, k, 0);
            
        }

    }

    public struct UnityParticlesProviderInitializer<TEntity> : IViewsProviderInitializer<TEntity> where TEntity : struct, IEntity {

        public IViewsProvider<TEntity> Create() {

            return PoolClass<UnityParticlesProvider<TEntity>>.Spawn();

        }

        public void Destroy(IViewsProvider<TEntity> instance) {

            PoolClass<UnityParticlesProvider<TEntity>>.Recycle((UnityParticlesProvider<TEntity>)instance);

        }

    }

}
#endif