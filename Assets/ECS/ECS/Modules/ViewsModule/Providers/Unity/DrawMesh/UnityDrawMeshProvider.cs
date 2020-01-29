#if DRAWMESH_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {
    
    using ME.ECS.Views;
    using ME.ECS.Views.Providers;

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        ViewId RegisterViewSource<TEntity>(DrawMeshViewSourceBase prefab) where TEntity : struct, IEntity;
        ViewId RegisterViewSource<TEntity, TProvider>(DrawMeshViewSourceBase prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProvider;
        void InstantiateView<TEntity>(DrawMeshViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity, TProvider>(DrawMeshViewSourceBase prefab) where TEntity : struct, IEntity where TProvider : struct, IViewsProvider {

            return this.RegisterViewSource<TEntity, TProvider>(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(DrawMeshViewSourceBase prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource<TEntity, UnityDrawMeshProvider>(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(DrawMeshViewSourceBase prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView(prefab.GetSource<TEntity>(), entity);
            
        }

    }

}

namespace ME.ECS.Views {
    
    using ME.ECS.Views.Providers;
    
    public partial interface IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        ViewId RegisterViewSource(DrawMeshViewSourceBase prefab);
        void UnRegisterViewSource(DrawMeshViewSourceBase prefab);
        void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity);

    }

    public partial class ViewsModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(DrawMeshViewSourceBase prefab) {

            return this.RegisterViewSource<UnityDrawMeshProvider>(prefab.GetSource<TEntity>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UnRegisterViewSource(DrawMeshViewSourceBase prefab) {
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(DrawMeshViewSourceBase prefab, Entity entity) {
            
            var viewSource = prefab.GetSource<TEntity>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

}

namespace ME.ECS.Views.Providers {

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

    }

    public abstract class DrawMeshView<TEntity> : DrawMeshViewBase, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }
        public Tick creationTick { get; set; }

        public virtual void OnInitialize(in TEntity data) { }
        public virtual void OnDeInitialize(in TEntity data) { }
        public abstract void ApplyState(in TEntity data, float deltaTime, bool immediately);

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

    public class UnityDrawMeshProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

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

        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            var obj = this.pool.Spawn();
            if (obj == null) {
                
                obj = System.Activator.CreateInstance(prefab.GetType());
                
            }

            var prefabSource = (DrawMeshView<TEntity>)prefab;
            var particleViewBase = (DrawMeshView<TEntity>)obj;
            particleViewBase.items = PoolArray<DrawMeshViewBase.Item>.Spawn(prefabSource.items.Length);
            for (int i = 0; i < particleViewBase.items.Length; ++i) {

                particleViewBase.items[i] = prefabSource.items[i];

            }
            particleViewBase.entity = prefabSource.entity;
            particleViewBase.prefabSourceId = prefabSource.prefabSourceId;
            
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
            
            return (IView<TEntity>)obj;

        }

        public override void Destroy(ref IView<TEntity> instance) {

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
                
                this.matrices = new UnityEngine.Matrix4x4[this.maxMatrices];
                
            }

        }

        public override void Update(System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<IView<TEntity>>> list, float deltaTime) {
            
            this.ValidateMatrices();
            
            foreach (var item in this.psItems) {
                
                var k = 0;
                var psItem = item.Value;
                var mesh = psItem.GetMesh();
                var material = psItem.material;
                foreach (var itemView in list) {

                    var itemsList = itemView.Value;
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

    public struct UnityDrawMeshProvider : IViewsProvider {

        public IViewsProvider<TEntity> Create<TEntity>() where TEntity : struct, IEntity {

            return PoolClass<UnityDrawMeshProvider<TEntity>>.Spawn();

        }

        public void Destroy<TEntity>(IViewsProvider<TEntity> instance) where TEntity : struct, IEntity {

            PoolClass<UnityDrawMeshProvider<TEntity>>.Recycle((UnityDrawMeshProvider<TEntity>)instance);

        }

    }

}
#endif