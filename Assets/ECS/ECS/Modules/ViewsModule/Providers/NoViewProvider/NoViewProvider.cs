namespace ME.ECS {

    using ME.ECS.Views;
    using ME.ECS.Views.Providers;
    
    public partial struct WorldViewsSettings {

        public bool unityNoViewProviderDisableJobs;

    }
    
    public partial interface IWorld {

        ViewId RegisterViewSource(NoViewBase prefab);
        void InstantiateView(NoViewBase prefab, Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(NoViewBase prefab) {

            return this.RegisterViewSource(new NoViewProviderInitializer(), (IView)prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(NoViewBase prefab, Entity entity) {

            this.InstantiateView((IView)prefab, entity);

        }

    }

}

namespace ME.ECS.Views.Providers {

    using ME.ECS;
    using ME.ECS.Views;
    using Unity.Jobs;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class NoViewBase {

        public void SimulateParticles(float time, uint seed) {
            
        }

        public void UpdateParticlesSimulation(float deltaTime) {
            
        }

        public override string ToString() {

            return "NoView";
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class NoView : NoViewBase, IView, IViewBaseInternal {

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
        public virtual void ApplyStateJob(float deltaTime, bool immediately) { }
        public virtual void ApplyState(float deltaTime, bool immediately) { }
        public virtual void ApplyPhysicsState(float deltaTime) { }

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class NoViewProvider : ViewsProvider {

        private PoolInternalBase pool;
        
        public override void OnConstruct() {

            this.pool = new PoolInternalBase(null, null);
            
        }

        public override void OnDeconstruct() {
            
            this.pool = null;
            
        }
        
        public override IView Spawn(IView prefab, ViewId prefabSourceId) {

            var prefabSource = (NoView)prefab;
            
            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }
            
            var particleViewBase = (IViewBaseInternal)obj;
            particleViewBase.Setup(this.world, new ViewInfo(prefabSource.entity, prefabSource.prefabSourceId, prefabSource.creationTick));
            
            return (IView)obj;

        }

        public override void Destroy(ref IView instance) {

            this.pool.Recycle(instance);
            instance = null;
            
        }

        private struct Job : Unity.Jobs.IJobParallelFor {

            public float deltaTime;
            
            public void Execute(int index) {

                var list = NoViewProvider.currentList.arr[index];
                if (list.mainView == null) return;
                
                for (int i = 0, count = list.Length; i < count; ++i) {

                    var instance = list[i] as NoView;
                    if (instance == null) continue;
                    
                    instance.ApplyStateJob(this.deltaTime, immediately: false);
                    
                }
                
            }

        }

        private static ME.ECS.Collections.BufferArray<Views> currentList;
        public override void Update(ME.ECS.Collections.BufferArray<Views> list, float deltaTime, bool hasChanged) {
            
            if (this.world.settings.useJobsForViews == true && this.world.settings.viewsSettings.unityNoViewProviderDisableJobs == false) {

                NoViewProvider.currentList = list;
                
                var job = new Job() {
                    deltaTime = deltaTime
                };
                var handle = job.Schedule(list.Length, 16);
                handle.Complete();

            } else {

                for (int j = 0; j < list.Length; ++j) {

                    var item = list.arr[j];
                    for (int i = 0, count = item.Length; i < count; ++i) {

                        var instance = item[i] as NoView;
                        if (instance == null) continue;

                        instance.ApplyStateJob(deltaTime, immediately: false);
                    
                    }
                    
                }
                
            }

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct NoViewProviderInitializer : IViewsProviderInitializer {

        int System.IComparable<IViewsProviderInitializerBase>.CompareTo(IViewsProviderInitializerBase other) { return 0; }

        public IViewsProvider Create() {

            return PoolClass<NoViewProvider>.Spawn();

        }

        public void Destroy(IViewsProvider instance) {

            PoolClass<NoViewProvider>.Recycle((NoViewProvider)instance);
            
        }

    }

}