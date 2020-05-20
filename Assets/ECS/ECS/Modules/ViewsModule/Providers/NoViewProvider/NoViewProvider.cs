namespace ME.ECS {

    using ME.ECS.Views;
    using ME.ECS.Views.Providers;
    
    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

        ViewId RegisterViewSource(NoViewBase prefab);
        void InstantiateView(NoViewBase prefab, Entity entity);

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

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
    public abstract class NoView : NoViewBase, IView {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }
        public Tick creationTick { get; set; }

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
            
            var particleViewBase = (NoView)obj;
            particleViewBase.entity = prefabSource.entity;
            particleViewBase.prefabSourceId = prefabSource.prefabSourceId;
            
            return (IView)obj;

        }

        public override void Destroy(ref IView instance) {

            this.pool.Recycle(instance);
            instance = null;
            
        }

        private struct Job : Unity.Jobs.IJobParallelFor {

            public float deltaTime;
            
            public void Execute(int index) {

                var list = NoViewProvider.currentList[index];
                for (int i = 0, count = list.Count; i < count; ++i) {

                    var instance = (ParticleViewBase)list[i];
                    instance.ApplyStateJob(this.deltaTime, immediately: false);
                    
                }
                
            }

        }

        private static System.Collections.Generic.List<IView>[] currentList;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateViews(System.Collections.Generic.List<IView>[] list, float deltaTime) {

            NoViewProvider.currentList = list;
            if (list != null) {
                
                var job = new Job() {
                    deltaTime = deltaTime
                };
                var handle = job.Schedule(list.Length, 16);
                handle.Complete();
                NoViewProvider.currentList = null;
                
            }

        }

        public override void Update(System.Collections.Generic.List<IView>[] list, float deltaTime) {
            
            this.UpdateViews(list, deltaTime);
            
        }

    }

    public struct NoViewProviderInitializer : IViewsProviderInitializer {

        public IViewsProvider Create() {

            return PoolClass<NoViewProvider>.Spawn();

        }

        public void Destroy(IViewsProvider instance) {

            PoolClass<NoViewProvider>.Recycle((NoViewProvider)instance);
            
        }

    }

}