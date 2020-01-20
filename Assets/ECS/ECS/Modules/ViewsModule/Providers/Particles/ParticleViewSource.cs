
namespace ME.ECS {

    public abstract class ParticleViewSourceBase : UnityEngine.MonoBehaviour {

        public abstract IView<TEntity> GetSource<TEntity>() where TEntity : struct, IEntity;

    }

    public abstract class ParticleViewSource<T> : ParticleViewSourceBase where T : ParticleViewBase, new() {

        public T data;
        
        public UnityEngine.MeshFilter[] filters;
        public UnityEngine.Renderer[] renderers;

        public override IView<TEntityInner> GetSource<TEntityInner>() {

            return (IView<TEntityInner>)this.data;

        }

        [UnityEngine.ContextMenu("Validate")]
        public void ValidateForced() {
            
            this.filters = this.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
            this.renderers = this.GetComponentsInChildren<UnityEngine.Renderer>(true);

            var renderer = this.renderers[0];
            var filter = this.filters[0];
            
            this.data = new T();
            var itemData = new ParticleSystemItem();
            itemData.material = renderer.sharedMaterial;
            itemData.mesh = filter.sharedMesh;
            this.data.itemData = itemData;

        }

        public void OnValidate() {

            this.ValidateForced();
            
        }

    }

}
