#if PARTICLES_VIEWS_MODULE_SUPPORT
namespace ME.ECS {
    
    using ME.ECS.Views;
    using ME.ECS.Views.Providers;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial struct WorldViewsSettings {

        public bool unityParticlesProviderDisableJobs;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial struct WorldDebugViewsSettings {

        public bool unityParticlesProviderShowOnScene;

    }

    public partial interface IWorldBase {
        
        ViewId RegisterViewSource(ParticleViewSourceBase prefab);
        void InstantiateView(ParticleViewSourceBase prefab, Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(ParticleViewSourceBase prefab) {

            return this.RegisterViewSource(new UnityParticlesProviderInitializer(), prefab.GetSource());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ParticleViewSourceBase prefab, Entity entity) {

            this.InstantiateView(prefab.GetSource(), entity);
            
        }

    }

}

namespace ME.ECS.Views {
    
    using ME.ECS.Views.Providers;
    
    public partial interface IViewModule {

        ViewId RegisterViewSource(ParticleViewSourceBase prefab);
        void UnRegisterViewSource(ParticleViewSourceBase prefab);
        void InstantiateView(ParticleViewSourceBase prefab, Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class ViewsModule {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(ParticleViewSourceBase prefab) {

            return this.RegisterViewSource(new UnityParticlesProviderInitializer(), prefab.GetSource());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(ParticleViewSourceBase prefab) {
            
            this.UnRegisterViewSource(prefab.GetSource());
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(ParticleViewSourceBase prefab, Entity entity) {
            
            var viewSource = prefab.GetSource();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

}

namespace ME.ECS.Views.Providers {

    using Unity.Jobs;
    using Collections;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class ParticleViewBase : IPoolableRecycle {

        [System.Serializable]
        public struct Item {

            public UnityEngine.ParticleSystem.Particle particleData;
            public ParticleSystemItem itemData;

        }

        public BufferArray<Item> items;

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<ParticleViewBase.Item>.Recycle(ref this.items);
            
        }

        public virtual void ApplyStateJob(float deltaTime, bool immediately) { }
        
        public override string ToString() {
            
            return "Renderers Count: " + this.items.Length.ToString();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref UnityEngine.ParticleSystem.Particle GetRootData() {

            return ref this.items.arr[0].particleData;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetRootData(ref UnityEngine.ParticleSystem.Particle data) {

            this.items.arr[0].particleData = data;
            for (int i = 1; i < this.items.Length; ++i) {

                ref var item = ref this.items.arr[i];
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
            UnityEngine.Renderer[] renderers,
            UnityEngine.ParticleSystem[] particleSystems,
            bool reset) {

            if (this.items.arr == null || this.items.Length != filters.Length + particleSystems.Length) {

                reset = true;

            }

            if (reset == true) {
                
                this.items = BufferArray<Item>.Empty;
                
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
                    itemData.meshRenderer = renderers[i];
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

                this.items = BufferArray<Item>.From(itemsList.ToArray());

            }
            
        }

        public void SimulateParticles(float time, uint seed) {

            for (int i = 0; i < this.items.Length; ++i) {

                this.items.arr[i].itemData.SimulateParticles(time, seed);

            }

        }

        public void UpdateParticlesSimulation(float deltaTime) {
            
            for (int i = 0; i < this.items.Length; ++i) {

                this.items.arr[i].itemData.UpdateParticlesSimulation(deltaTime);

            }
            
        }

        public abstract void DoCopyFrom(ParticleViewBase source);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class ParticleView<T> : ParticleViewBase, IView, IViewBaseInternal where T : ParticleView<T> {

        int System.IComparable<IView>.CompareTo(IView other) { return 0; }

        public World world { get; private set; }
        public Entity entity { get; private set; }
        public ViewId prefabSourceId { get; private set; }
        public Tick creationTick { get; private set; }

        void IViewBaseInternal.Setup(World world, ViewInfo viewInfo) {

            this.world = world;
            this.entity = viewInfo.entity;
            this.prefabSourceId = viewInfo.prefabSourceId;
            this.creationTick = viewInfo.creationTick;

        }

        void IView.DoInitialize() {
            
            this.OnInitialize();
            
        }

        void IView.DoDeInitialize() {
            
            this.OnDeInitialize();
            
        }

        public virtual void OnInitialize() { }
        public virtual void OnDeInitialize() { }
        public virtual void ApplyState(float deltaTime, bool immediately) { }
        public virtual void ApplyPhysicsState(float deltaTime) { }

        public sealed override void DoCopyFrom(ParticleViewBase source) {

            var sourceView = (T)source;
            this.entity = sourceView.entity;
            this.prefabSourceId = sourceView.prefabSourceId;
            this.creationTick = sourceView.creationTick;

            this.CopyFrom((T)source);

        }

        protected virtual void CopyFrom(T source) {}

    }
    
    [System.Serializable]
    public struct ParticleSystemItem {

        public ParticleSystemSimulationItem psSimulation;
        public UnityEngine.ParticleSystem psSource;
        public UnityEngine.Mesh mesh;
        public UnityEngine.Renderer meshRenderer;
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

            var shadowCastingMode = (this.meshRenderer != null ? this.meshRenderer.shadowCastingMode : UnityEngine.Rendering.ShadowCastingMode.Off);
            var receiveShadows = (this.meshRenderer != null ? (this.meshRenderer.receiveShadows == true ? 1 : 0) : 0);
            return MathUtils.GetKey(this.material.GetInstanceID(), (this.mesh != null ? this.mesh.GetInstanceID() : this.meshFilter.GetInstanceID())) ^ (int)shadowCastingMode ^ receiveShadows;
            
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

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class UnityParticlesProvider : ViewsProvider {

        private System.Collections.Generic.Dictionary<long, ParticleSystemItem> psItems;
        private PoolInternalBase pool;
        private BufferArray<UnityEngine.ParticleSystem.Particle> particles;
        private BufferArray<UnityEngine.ParticleSystem.Particle> particlesStatic;
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
            subEmittersModule.enabled = false;
            
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
            if (this.world.debugSettings.showViewsOnScene == false || this.world.debugSettings.viewsSettings.unityParticlesProviderShowOnScene == false) {
                
                particleSystem.gameObject.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                
            }
            particleSystem.Stop(withChildren: true);
            particleSystem.Pause(withChildren: true);
            particleSystem.useAutoRandomSeed = false;
            particleSystem.randomSeed = 1u;
            
            var main = particleSystem.main;
            main.loop = false;
            main.prewarm = false;
            main.playOnAwake = false;
            main.startSpeed = 0f;
            #if UNITY_WEBGL
            main.duration = 10000;
            main.maxParticles = 10000;
            main.startLifetime = 10000;
            #else
            main.duration = float.MaxValue;
            main.maxParticles = int.MaxValue;
            main.startLifetime = float.MaxValue;
            #endif
            main.ringBufferMode = UnityEngine.ParticleSystemRingBufferMode.PauseUntilReplaced;
            main.simulationSpace = UnityEngine.ParticleSystemSimulationSpace.World;
                    
            var emission = particleSystem.emission;
            emission.enabled = false;
                    
            var shape = particleSystem.shape;
            shape.enabled = false;
            
            particleSystemRenderer = particleSystem.GetComponent<UnityEngine.ParticleSystemRenderer>();
            particleSystemRenderer.alignment = UnityEngine.ParticleSystemRenderSpace.World;
                    
            particleSystemRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            particleSystemRenderer.receiveShadows = true;
            
            #if UNITY_WEBGL
            particleSystemRenderer.enableGPUInstancing = false;
            #endif

            //particleSystem.Play(true);

        }
        
        public override IView Spawn(IView prefab, ViewId prefabSourceId) {

            var prefabSource = (ParticleViewBase)prefab;
            
            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }

            var particleViewBase = (ParticleViewBase)obj;
            particleViewBase.items = PoolArray<ParticleViewBase.Item>.Spawn(prefabSource.items.Length);
            for (int i = 0; i < particleViewBase.items.Length; ++i) {

                particleViewBase.items.arr[i] = prefabSource.items.arr[i];

            }
            
            particleViewBase.DoCopyFrom(prefabSource);

            var maxParticleCount = 0;
            long key;
            // Create PS if doesn't exist
            for (int i = 0; i < prefabSource.items.Length; ++i) {

                maxParticleCount = 0;
                ref var source = ref prefabSource.items.arr[i];
                key = source.itemData.GetKey();
                ParticleSystemItem psItem;
                if (this.psItems.TryGetValue(key, out psItem) == false) {
                    
                    psItem = source.itemData;

                    var idx = -1;
                    var idxInheritedLifetime = -1;
                    UnityEngine.ParticleSystem particleSystem;
                    if (psItem.psSource != null) {

                        var subEmittersModule = this.mainParticleSystem.subEmitters;
                        subEmittersModule.enabled = true;
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
                        emissionModule.SetBurst(0, new UnityEngine.ParticleSystem.Burst(0f, 1));*/
                        
                        /*idx = subEmittersModule.subEmittersCount;
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

                particleViewBase.items.arr[i].itemData = psItem;

            }
            
            if (maxParticleCount > this.maxParticles) this.maxParticles = maxParticleCount;

            return (IView)obj;

        }

        public override void Destroy(ref IView instance) {

            var view = (ParticleViewBase)instance;
            for (int i = 0; i < view.items.Length; ++i) {

                view.items.arr[i].particleData.startSize = 0f;

            }

            this.pool.Recycle(instance);
            instance = null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ValidateParticles() {

            if (this.particles.arr == null || this.particles.Length < this.maxParticles) {
                
                PoolArray<UnityEngine.ParticleSystem.Particle>.Recycle(ref this.particles);
                this.particles = PoolArray<UnityEngine.ParticleSystem.Particle>.Spawn(this.maxParticles);
                
            }

            if (this.particlesStatic.arr == null || this.particlesStatic.Length < this.maxParticles) {
                
                PoolArray<UnityEngine.ParticleSystem.Particle>.Recycle(ref this.particlesStatic);
                this.particlesStatic = PoolArray<UnityEngine.ParticleSystem.Particle>.Spawn(this.maxParticles);

            }

        }

        private struct Job : Unity.Jobs.IJobParallelFor {

            public float deltaTime;
            
            public void Execute(int index) {

                var list = UnityParticlesProvider.currentList.arr[index];
                if (list.mainView == null) return;
                
                for (int i = 0, count = list.Length; i < count; ++i) {

                    var instance = list[i] as ParticleViewBase;
                    if (instance == null) continue;
                    
                    instance.ApplyStateJob(this.deltaTime, immediately: false);
                    
                }
                
            }

        }

        private static BufferArray<Views> currentList;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateViews(BufferArray<Views> list, float deltaTime) {

            if (this.world.settings.useJobsForViews == true && this.world.settings.viewsSettings.unityParticlesProviderDisableJobs == false) {

                UnityParticlesProvider.currentList = list;
                if (list.arr != null) {

                    var job = new Job() {
                        deltaTime = deltaTime
                    };
                    var handle = job.Schedule(list.Length, 16);
                    handle.Complete();

                }

            } else {

                for (int j = 0, cnt = list.Length; j < cnt; ++j) {

                    var item = list.arr[j];
                    for (int i = 0, count = item.Length; i < count; ++i) {

                        var instance = item[i] as ParticleViewBase;
                        if (instance == null) continue;

                        instance.ApplyStateJob(deltaTime, immediately: false);

                    }

                }

            }

        }

        public override void Update(BufferArray<Views> list, float deltaTime, bool hasChanged) {

            this.UpdateViews(list, deltaTime);
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
                for (var id = 0; id < list.Length; ++id) {
                    
                    var itemsList = list.arr[id];
                    if (itemsList.mainView == null) continue;
                    
                    var count = itemsList.Length;
                    for (int i = 0; i < count; ++i) {

                        var view = itemsList[i] as ParticleViewBase;
                        if (view == null) continue;

                        for (int j = 0; j < view.items.Length; ++j) {

                            ref var element = ref view.items.arr[j];
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
                                    this.particles.arr[k] = element.particleData;

                                    // Just trigger sub system
                                    if (element.itemData.r == 1) {

                                        if (element.itemData.simulation == true) {

                                            this.particles.arr[k].remainingLifetime = element.itemData.lifetime / element.itemData.startLifetime;
                                            this.mainParticleSystem.TriggerSubEmitter(element.itemData.subEmitterIdxInheritedLifetime, ref this.particles.arr[k]);

                                        } else {

                                            this.mainParticleSystem.TriggerSubEmitter(element.itemData.subEmitterIdx, ref this.particles.arr[k]);

                                        }

                                    }

                                    ++element.itemData.r;

                                    ++k;
                                    ++dynamicK;

                                } else {

                                    // Static mesh with material
                                    #if UNITY_WEBGL
                                    element.particleData.remainingLifetime = 10000;
                                    element.particleData.startLifetime = 10000;
                                    #else
                                    element.particleData.remainingLifetime = float.MaxValue;
                                    element.particleData.startLifetime = float.MaxValue;
                                    #endif

                                    this.particlesStatic.arr[staticK] = element.particleData;
                                    ++staticK;

                                }

                            }

                        }

                    }
                }

                if (staticK > 0 || dynamicK == 0) ps.SetParticles(this.particlesStatic.arr, staticK, 0);

            }

            this.mainParticleSystem.SetParticles(this.particles.arr, k, 0);
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct UnityParticlesProviderInitializer : IViewsProviderInitializer {

        int System.IComparable<IViewsProviderInitializerBase>.CompareTo(IViewsProviderInitializerBase other) { return 0; }

        public IViewsProvider Create() {

            return PoolClass<UnityParticlesProvider>.Spawn();

        }

        public void Destroy(IViewsProvider instance) {

            PoolClass<UnityParticlesProvider>.Recycle((UnityParticlesProvider)instance);

        }

    }

}
#endif