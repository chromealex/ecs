#if DRAWMESH_VIEWS_MODULE_SUPPORT
namespace ME.ECS {
    
    using ME.ECS.Views;
    using ME.ECS.Views.Providers;

    public partial struct WorldViewsSettings {

        public bool unityDrawMeshProviderDisableJobs;

    }

    public partial struct WorldDebugViewsSettings {

    }
    
    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

        ViewId RegisterViewSource(DrawMeshViewSourceBase prefab);
        void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity);

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

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
    
    public partial interface IViewModule<TState> where TState : class, IState<TState>, new() {

        ViewId RegisterViewSource(DrawMeshViewSourceBase prefab);
        void UnRegisterViewSource(DrawMeshViewSourceBase prefab);
        void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity);

    }

    public partial class ViewsModule<TState> where TState : class, IState<TState>, new() {
        
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

        public Item[] items;

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<DrawMeshViewBase.Item>.Recycle(ref this.items);
            
        }

        public virtual void ApplyStateJob(float deltaTime, bool immediately) { }
        
        public override string ToString() {
            
            return "Renderers Count: " + this.items.Length.ToString();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref DrawMeshData GetRootData() {
            
            return ref this.items[0].drawMeshData;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetRootData(ref DrawMeshData data) {
            
            this.items[0].drawMeshData = data;
            for (int i = 1; i < this.items.Length; ++i) {

                ref var item = ref this.items[i];
                item.drawMeshData.position = data.position + item.itemData.localPosition;
                item.drawMeshData.rotation = data.rotation + item.itemData.localRotation;
                item.drawMeshData.scale = UnityEngine.Vector3.Scale(data.scale, item.itemData.localScale);

            }
            
        }

        public void SetItems(UnityEngine.Vector3 rootPosition, UnityEngine.Vector3 rootRotation, UnityEngine.Vector3 rootScale, UnityEngine.MeshFilter[] filters, UnityEngine.Renderer[] renderers) {
            
            this.items = new Item[filters.Length];
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
                this.items[i] = item;

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
    public abstract class DrawMeshView<T> : DrawMeshViewBase, IView where T : DrawMeshView<T> {

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
        public virtual void ApplyState(float deltaTime, bool immediately) { }

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
        private UnityEngine.Matrix4x4[] matrices;
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

                particleViewBase.items[i] = prefabSource.items[i];

            }
            particleViewBase.DoCopyFrom(prefabSource);
            
            long key;
            for (int i = 0; i < prefabSource.items.Length; ++i) {

                ref var source = ref prefabSource.items[i];
                key = source.itemData.GetKey();
                DrawMeshSystemItem psItem;
                if (this.psItems.TryGetValue(key, out psItem) == false) {
                    
                    psItem = source.itemData;
                    this.psItems.Add(key, psItem);
                    
                }

                particleViewBase.items[i].itemData = psItem;

            }
            
            return (IView)obj;

        }

        public override void Destroy(ref IView instance) {

            var view = (DrawMeshViewBase)instance;
            for (int i = 0; i < view.items.Length; ++i) {

                view.items[i].drawMeshData.scale = UnityEngine.Vector3.zero;

            }

            this.pool.Recycle(instance);
            instance = null;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void ValidateMatrices() {

            if (this.matrices == null || this.matrices.Length < this.maxMatrices) {
                
                if (this.matrices != null) PoolArray<UnityEngine.Matrix4x4>.Recycle(ref this.matrices);
                this.matrices = PoolArray<UnityEngine.Matrix4x4>.Spawn(this.maxMatrices);
                
            }

        }

        private struct Job : Unity.Jobs.IJobParallelFor {

            public float deltaTime;
            
            public void Execute(int index) {

                var list = UnityDrawMeshProvider.currentList[index];
                if (list == null) return;
                
                for (int i = 0, count = list.Count; i < count; ++i) {

                    var instance = list[i] as DrawMeshViewBase;
                    if (instance == null) continue;

                    instance.ApplyStateJob(this.deltaTime, immediately: false);
                    
                }
                
            }

        }

        private static System.Collections.Generic.List<IView>[] currentList;
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateViews(System.Collections.Generic.List<IView>[] list, float deltaTime) {

            if (list == null) return;
            
            if (this.world.settings.useJobsForViews == true && this.world.settings.viewsSettings.unityDrawMeshProviderDisableJobs == false) {

                UnityDrawMeshProvider.currentList = list;
                
                var job = new Job() {
                    deltaTime = deltaTime
                };
                var handle = job.Schedule(list.Length, 16);
                handle.Complete();
                UnityDrawMeshProvider.currentList = null;

            } else {

                for (int j = 0; j < list.Length; ++j) {

                    var item = list[j];
                    for (int i = 0, count = item.Count; i < count; ++i) {

                        var instance = item[i] as DrawMeshViewBase;
                        if (instance == null) continue;

                        instance.ApplyStateJob(deltaTime, immediately: false);
                    
                    }
                    
                }
                
            }

        }

        public override void Update(System.Collections.Generic.List<IView>[] list, float deltaTime) {
            
            this.UpdateViews(list, deltaTime);
            this.ValidateMatrices();
            
            foreach (var item in this.psItems) {
                
                var k = 0;
                var psItem = item.Value;
                var mesh = psItem.GetMesh();
                var material = psItem.material;
                for (var id = 0; id < list.Length; ++id) {
                    
                    var itemsList = list[id];
                    var count = itemsList.Count;
                    for (int i = 0; i < count; ++i) {

                        var view = (DrawMeshViewBase)itemsList[i];
                        for (int j = 0; j < view.items.Length; ++j) {

                            ref var element = ref view.items[j];
                            if (element.itemData.GetMesh() == mesh && element.itemData.material == material) {

                                if (k >= this.maxMatrices) {

                                    this.maxMatrices = k * 2;
                                    return;

                                }

                                this.matrices[k++] = element.drawMeshData.matrix;

                            }

                        }

                    }
                }

                if (mesh != null && material != null) UnityEngine.Graphics.DrawMeshInstanced(mesh, 0, psItem.material, this.matrices, k);

            }
            
        }

    }

    public struct UnityDrawMeshProviderInitializer : IViewsProviderInitializer {

        public IViewsProvider Create() {

            return PoolClass<UnityDrawMeshProvider>.Spawn();

        }

        public void Destroy(IViewsProvider instance) {

            PoolClass<UnityDrawMeshProvider>.Recycle((UnityDrawMeshProvider)instance);

        }

    }

}
#endif