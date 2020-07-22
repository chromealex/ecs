#if DRAWMESH_VIEWS_MODULE_SUPPORT
namespace ME.ECS {
    
    using ME.ECS.Views;
    using ME.ECS.Views.Providers;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial struct WorldViewsSettings {

        public bool unityDrawMeshProviderDisableJobs;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial struct WorldDebugViewsSettings {

    }
    
    public partial interface IWorldBase {
        
        ViewId RegisterViewSource(DrawMeshViewSourceBase prefab);
        void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(DrawMeshViewSourceBase prefab) {

            return this.RegisterViewSource(new UnityDrawMeshProviderInitializer(), prefab.GetSource());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity) {

            this.InstantiateView(prefab.GetSource(), entity);
            
        }

    }

}

namespace ME.ECS.Views {
    
    using ME.ECS.Views.Providers;
    
    public partial interface IViewModule {

        ViewId RegisterViewSource(DrawMeshViewSourceBase prefab);
        void UnRegisterViewSource(DrawMeshViewSourceBase prefab);
        void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class ViewsModule {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(DrawMeshViewSourceBase prefab) {

            return this.RegisterViewSource(new UnityDrawMeshProviderInitializer(), prefab.GetSource());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(DrawMeshViewSourceBase prefab) {

            this.UnRegisterViewSource(prefab.GetSource());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity) {
            
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
    public struct DrawMeshData {

        public UnityEngine.Vector3 position;
        public UnityEngine.Vector3 rotation;
        public UnityEngine.Vector3 scale;

        public UnityEngine.Matrix4x4 matrix {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return UnityEngine.Matrix4x4.TRS(this.position, UnityEngine.Quaternion.Euler(this.rotation), this.scale);
            }
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class DrawMeshViewBase : IPoolableRecycle {

        [System.Serializable]
        public struct Item {

            public DrawMeshData drawMeshData;
            public DrawMeshSystemItem itemData;

        }

        public BufferArray<Item> items;

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<DrawMeshViewBase.Item>.Recycle(ref this.items);
            
        }

        public virtual void ApplyStateJob(float deltaTime, bool immediately) { }
        
        public override string ToString() {
            
            return "Renderers Count: " + this.items.Length.ToString();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref DrawMeshData GetRootData() {
            
            return ref this.items.arr[0].drawMeshData;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetRootData(ref DrawMeshData data) {
            
            this.items.arr[0].drawMeshData = data;
            for (int i = 1; i < this.items.Length; ++i) {

                ref var item = ref this.items.arr[i];
                item.drawMeshData.position = data.position + item.itemData.localPosition;
                item.drawMeshData.rotation = data.rotation + item.itemData.localRotation;
                item.drawMeshData.scale = UnityEngine.Vector3.Scale(data.scale, item.itemData.localScale);

            }
            
        }

        public void SetItems(UnityEngine.Vector3 rootPosition, UnityEngine.Vector3 rootRotation, UnityEngine.Vector3 rootScale, UnityEngine.MeshFilter[] filters, UnityEngine.Renderer[] renderers) {

            this.items = PoolArray<Item>.Spawn(filters.Length);//new Item[filters.Length];
            for (int i = 0; i < filters.Length; ++i) {

                var tr = filters[i].transform;
                
                var item = new Item();
                var itemData = new DrawMeshSystemItem();
                itemData.material = renderers[i].sharedMaterial;
                itemData.mesh = filters[i].sharedMesh;
                itemData.meshFilter = filters[i];
                itemData.localPosition = (i == 0 ? rootPosition : tr.position - rootPosition);
                itemData.localRotation = (i == 0 ? rootRotation : tr.rotation.eulerAngles - rootRotation);
                itemData.localScale = (i == 0 ? rootScale : UnityEngine.Vector3.Scale(tr.lossyScale, new UnityEngine.Vector3(1f / rootScale.x, 1f / rootScale.y, 1f / rootScale.z)));
                item.itemData = itemData;
                this.items.arr[i] = item;

            }

        }

        public void SimulateParticles(float time, uint seed) {
            
        }

        public void UpdateParticlesSimulation(float deltaTime) {
            
        }

        public virtual void DoCopyFrom(DrawMeshViewBase source) { }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class DrawMeshView<T> : DrawMeshViewBase, IView, IViewBaseInternal where T : DrawMeshView<T> {

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

        public sealed override void DoCopyFrom(DrawMeshViewBase source) {

            var sourceView = (T)source;
            this.entity = sourceView.entity;
            this.prefabSourceId = sourceView.prefabSourceId;
            this.creationTick = sourceView.creationTick;

            this.CopyFrom((T)source);

        }

        protected virtual void CopyFrom(T source) {}

    }
    
    [System.Serializable]
    public struct DrawMeshSystemItem {

        public UnityEngine.Mesh mesh;
        public UnityEngine.MeshFilter meshFilter;
        public UnityEngine.Material material;
        public UnityEngine.Vector3 localPosition;
        public UnityEngine.Vector3 localRotation;
        public UnityEngine.Vector3 localScale;
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public long GetKey() {
            
            return MathUtils.GetKey(this.material.GetInstanceID(), (this.mesh != null ? this.mesh.GetInstanceID() : this.meshFilter.GetInstanceID()));
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public UnityEngine.Mesh GetMesh() {

            return (this.mesh != null ? this.mesh : this.meshFilter.sharedMesh);

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class UnityDrawMeshProvider : ViewsProvider {

        private System.Collections.Generic.Dictionary<long, DrawMeshSystemItem> psItems;
        private PoolInternalBase pool;
        private BufferArray<UnityEngine.Matrix4x4> matrices;
        private int maxMatrices = 1000;

        public override void OnConstruct() {

            this.psItems = PoolDictionary<long, DrawMeshSystemItem>.Spawn(100);
            this.pool = new PoolInternalBase(null, null);
            
        }

        public override void OnDeconstruct() {

            this.pool = null;
            PoolDictionary<long, DrawMeshSystemItem>.Recycle(ref this.psItems);
            
        }

        public override IView Spawn(IView prefab, ViewId prefabSourceId) {

            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }

            var prefabSource = (DrawMeshViewBase)prefab;
            var particleViewBase = (DrawMeshViewBase)obj;
            particleViewBase.items = PoolArray<DrawMeshViewBase.Item>.Spawn(prefabSource.items.Length);
            for (int i = 0; i < particleViewBase.items.Length; ++i) {

                particleViewBase.items.arr[i] = prefabSource.items.arr[i];

            }
            particleViewBase.DoCopyFrom(prefabSource);
            
            long key;
            for (int i = 0; i < prefabSource.items.Length; ++i) {

                ref var source = ref prefabSource.items.arr[i];
                key = source.itemData.GetKey();
                DrawMeshSystemItem psItem;
                if (this.psItems.TryGetValue(key, out psItem) == false) {
                    
                    psItem = source.itemData;
                    this.psItems.Add(key, psItem);
                    
                }

                particleViewBase.items.arr[i].itemData = psItem;

            }
            
            return (IView)obj;

        }

        public override void Destroy(ref IView instance) {

            var view = (DrawMeshViewBase)instance;
            for (int i = 0; i < view.items.Length; ++i) {

                view.items.arr[i].drawMeshData.scale = UnityEngine.Vector3.zero;

            }

            this.pool.Recycle(instance);
            instance = null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ValidateMatrices() {

            if (this.matrices.arr == null || this.matrices.Length < this.maxMatrices) {
                
                PoolArray<UnityEngine.Matrix4x4>.Recycle(ref this.matrices);
                this.matrices = PoolArray<UnityEngine.Matrix4x4>.Spawn(this.maxMatrices);
                
            }

        }

        private struct Job : Unity.Jobs.IJobParallelFor {

            public float deltaTime;
            
            public void Execute(int index) {

                var list = UnityDrawMeshProvider.currentList.arr[index];
                if (list.mainView == null) return;
                
                for (int i = 0, count = list.Length; i < count; ++i) {

                    var instance = list[i] as DrawMeshViewBase;
                    if (instance == null) continue;

                    instance.ApplyStateJob(this.deltaTime, immediately: false);
                    
                }
                
            }

        }

        private static BufferArray<Views> currentList;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateViews(BufferArray<Views> list, float deltaTime) {

            if (list.arr == null) return;
            
            if (this.world.settings.useJobsForViews == true && this.world.settings.viewsSettings.unityDrawMeshProviderDisableJobs == false) {

                UnityDrawMeshProvider.currentList = list;
                
                var job = new Job() {
                    deltaTime = deltaTime
                };
                var handle = job.Schedule(list.Length, 16);
                handle.Complete();

            } else {

                for (int j = 0; j < list.Length; ++j) {

                    var item = list.arr[j];
                    for (int i = 0, count = item.Length; i < count; ++i) {

                        var instance = item[i] as DrawMeshViewBase;
                        if (instance == null) continue;

                        instance.ApplyStateJob(deltaTime, immediately: false);
                    
                    }
                    
                }
                
            }

        }

        public override void Update(BufferArray<Views> list, float deltaTime, bool hasChanged) {
            
            this.UpdateViews(list, deltaTime);
            this.ValidateMatrices();
            
            foreach (var item in this.psItems) {
                
                var k = 0;
                var psItem = item.Value;
                var mesh = psItem.GetMesh();
                var material = psItem.material;
                for (var id = 0; id < list.Length; ++id) {
                    
                    var itemsList = list.arr[id];
                    var count = itemsList.Length;
                    for (int i = 0; i < count; ++i) {

                        var view = (DrawMeshViewBase)itemsList[i];
                        for (int j = 0; j < view.items.Length; ++j) {

                            ref var element = ref view.items.arr[j];
                            if (element.itemData.GetMesh() == mesh && element.itemData.material == material) {

                                if (k >= this.maxMatrices) {

                                    this.maxMatrices = k * 2;
                                    return;

                                }

                                this.matrices.arr[k++] = element.drawMeshData.matrix;

                            }

                        }

                    }
                }

                if (mesh != null && material != null) UnityEngine.Graphics.DrawMeshInstanced(mesh, 0, psItem.material, this.matrices.arr, k);

            }
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct UnityDrawMeshProviderInitializer : IViewsProviderInitializer {

        int System.IComparable<IViewsProviderInitializerBase>.CompareTo(IViewsProviderInitializerBase other) { return 0; }

        public IViewsProvider Create() {

            return PoolClass<UnityDrawMeshProvider>.Spawn();

        }

        public void Destroy(IViewsProvider instance) {

            PoolClass<UnityDrawMeshProvider>.Recycle((UnityDrawMeshProvider)instance);

        }

    }

}
#endif