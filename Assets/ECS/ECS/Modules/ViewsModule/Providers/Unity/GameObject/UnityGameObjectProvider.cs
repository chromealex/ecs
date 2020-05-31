#if GAMEOBJECT_VIEWS_MODULE_SUPPORT
namespace ME.ECS {

    using ME.ECS.Views;
    using ME.ECS.Views.Providers;
    
    public partial struct WorldViewsSettings {

        public bool unityGameObjectProviderDisableJobs;

    }

    public partial struct WorldDebugViewsSettings {

        public bool unityGameObjectProviderShowOnScene;

    }

    public partial interface IWorldBase {
        
        ViewId RegisterViewSource(UnityEngine.GameObject prefab);
        ViewId RegisterViewSource(MonoBehaviourViewBase prefab);
        void InstantiateView(UnityEngine.GameObject prefab, Entity entity);
        void InstantiateView(MonoBehaviourViewBase prefab, Entity entity);

    }
    
    public partial class World {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(UnityEngine.GameObject prefab) {

            return this.RegisterViewSource(new UnityGameObjectProviderInitializer(), prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(UnityGameObjectProviderInitializer providerInitializer, UnityEngine.GameObject prefab) {

            IView component;
            if (prefab.TryGetComponent(out component) == true) {

                return this.RegisterViewSource(providerInitializer, component);

            }

            return ViewId.Zero;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(MonoBehaviourViewBase prefab) {

            return this.RegisterViewSource(new UnityGameObjectProviderInitializer(), (IView)prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(UnityEngine.GameObject prefab, Entity entity) {

            IView component;
            if (prefab.TryGetComponent(out component) == true) {

                this.InstantiateView(component, entity);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(MonoBehaviourViewBase prefab, Entity entity) {

            this.InstantiateView((IView)prefab, entity);

        }

    }

}

namespace ME.ECS.Views {

    using ME.ECS.Views.Providers;

    public partial interface IViewModule {

        ViewId RegisterViewSource(UnityEngine.GameObject prefab);
        void UnRegisterViewSource(UnityEngine.GameObject prefab);
        void InstantiateView(UnityEngine.GameObject prefab, Entity entity);

    }

    public partial class ViewsModule {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(UnityEngine.GameObject prefab) {

            return this.RegisterViewSource(new UnityGameObjectProviderInitializer(), prefab.GetComponent<MonoBehaviourView>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(UnityEngine.GameObject prefab) {

            this.UnRegisterViewSource(prefab.GetComponent<MonoBehaviourView>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(UnityEngine.GameObject prefab, Entity entity) {
            
            var viewSource = prefab.GetComponent<MonoBehaviourView>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

}

namespace ME.ECS.Views.Providers {

    using ME.ECS;
    using ME.ECS.Views;
    using Unity.Jobs;
    using UnityEngine.Jobs;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class MonoBehaviourViewBase : ViewBase, IDoValidate {

        public ParticleSystemSimulation particleSystemSimulation;
        new protected UnityEngine.Transform transform;

        public virtual bool applyStateJob => true;
        public virtual void ApplyStateJob(TransformAccess transform, float deltaTime, bool immediately) { }

        internal void InitializeTransform() {

            this.transform = base.transform;

        }

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

        public virtual void OnValidate() {
            
            this.DoValidate();
            
        }

        public override string ToString() {

            return this.particleSystemSimulation.ToString();
            
        }

    }

    public abstract class PhysicsView : MonoBehaviourView {
        
        new public UnityEngine.Rigidbody rigidbody;

        public void OnCollisionEnter(UnityEngine.Collision other) {

            this.entity.SetData(new ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionEnter() {
                collision = other
            });
            
        }

        public void OnCollisionExit(UnityEngine.Collision other) {

            this.entity.SetData(new ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionExit() {
                collision = other
            });
            
        }

        public void OnCollisionStay(UnityEngine.Collision other) {

            this.entity.SetData(new ME.ECS.Features.PhysicsDeterministic.Components.PhysicsOnCollisionStay() {
                collision = other
            });
            
        }
        
        public override void ApplyPhysicsState(float deltaTime) {
            
            this.entity.SetPosition(this.rigidbody.position);
            this.entity.SetRotation(this.rigidbody.rotation);
            
        }

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class MonoBehaviourView : MonoBehaviourViewBase, IView {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }
        public Tick creationTick { get; set; }

        void IView.DoInitialize() {
            
            this.InitializeTransform();
            this.OnInitialize();
            
        }

        void IView.DoDeInitialize() {
            
            this.OnDeInitialize();
            
        }
        
        public virtual void OnInitialize() { }
        public virtual void OnDeInitialize() { }
        public virtual void ApplyState(float deltaTime, bool immediately) { }
        public virtual void ApplyPhysicsState(float deltaTime) {}

        public override string ToString() {

            var info = string.Empty;
            info += "Entity: " + this.entity.ToString() + "\n";
            info += "Prefab Source Id: " + this.prefabSourceId + "\n";
            info += "Creation Tick: " + this.creationTick + "\n";
            return info + base.ToString();
            
        }

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class UnityGameObjectProvider : ViewsProvider {

        private PoolGameObject<MonoBehaviourView> pool;
        
        public override void OnConstruct() {

            this.pool = new PoolGameObject<MonoBehaviourView>();
            
        }

        public override void OnDeconstruct() {

            this.pool = null;
            this.currentTransformArray.Dispose();

        }
        
        public override IView Spawn(IView prefab, ViewId prefabSourceId) {

            var view = this.pool.Spawn((MonoBehaviourView)prefab, prefabSourceId);
            if (this.world.debugSettings.showViewsOnScene == false || this.world.debugSettings.viewsSettings.unityGameObjectProviderShowOnScene == false) {
                
                view.gameObject.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                
            }

            return view;

        }

        public override void Destroy(ref IView instance) {

            var instanceTyped = (MonoBehaviourView)instance;
            this.pool.Recycle(ref instanceTyped);
            instance = null;

        }
        
        private struct Job : IJobParallelForTransform {

            public float deltaTime;
            
            public void Execute(int index, TransformAccess transform) {

                var list = UnityGameObjectProvider.currentList;
                var k = 0;
                for (int i = 0, length = list.Length; i < length; ++i) {

                    var item = list[i];
                    if (item == null) continue;
                    
                    for (int j = 0, count = item.Count; j < count; ++j) {

                        var instance = item[j] as MonoBehaviourViewBase;
                        if (instance == null) continue;
                        
                        if (instance.applyStateJob == true && k++ == index) {
                            
                            instance.ApplyStateJob(transform, this.deltaTime, immediately: false);
                            return;

                        }

                    }

                }

            }

        }

        private static System.Collections.Generic.List<IView>[] currentList;
        private UnityEngine.Transform[] currentTransforms;
        private TransformAccessArray currentTransformArray;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateViews(System.Collections.Generic.List<IView>[] list, float deltaTime) {

            if (this.world.settings.useJobsForViews == false || this.world.settings.viewsSettings.unityGameObjectProviderDisableJobs == true) return;
            
            UnityGameObjectProvider.currentList = list;
            if (list != null) {

                if (this.currentTransformArray.isCreated == false) this.currentTransformArray = new TransformAccessArray(list.Length);
                var changed = ArrayUtils.Resize(list.Length - 1, ref this.currentTransforms);

                var k = 0;
                for (int i = 0, length = list.Length; i < length; ++i) {

                    var item = list[i];
                    if (item == null) continue;

                    for (int j = 0, count = item.Count; j < count; ++j) {

                        var view = item[j] as MonoBehaviourViewBase;
                        if (view == null) continue;
                        if (view.applyStateJob == true) {

                            changed |= ArrayUtils.Resize(k, ref this.currentTransforms);
                            ref var tr = ref this.currentTransforms[k++];
                            if (tr != view.transform) {

                                changed = true;
                                tr = view.transform;

                            }

                        }

                    }

                }

                if (changed == true) this.currentTransformArray.SetTransforms(this.currentTransforms);

                if (this.currentTransforms.Length > 0) {

                    var job = new Job() {
                        deltaTime = deltaTime
                    };

                    var handle = job.Schedule(this.currentTransformArray);
                    handle.Complete();
                    UnityGameObjectProvider.currentList = null;

                }
                
            }

        }

        public override void Update(System.Collections.Generic.List<IView>[] list, float deltaTime) {
            
            this.UpdateViews(list, deltaTime);
            
        }

    }

    public struct UnityGameObjectProviderInitializer : IViewsProviderInitializer {

        public IViewsProvider Create() {
            
            return PoolClass<UnityGameObjectProvider>.Spawn();

        }

        public void Destroy(IViewsProvider instance) {

            PoolClass<UnityGameObjectProvider>.Recycle((UnityGameObjectProvider)instance);
            
        }

    }

}
#endif