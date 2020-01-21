
namespace ME.ECS.Views.Providers {

    public abstract class ParticleViewSourceBase : UnityEngine.MonoBehaviour {

        public abstract IView<TEntity> GetSource<TEntity>() where TEntity : struct, IEntity;

        [UnityEngine.ContextMenu("Validate")]
        public virtual void DoValidate() {
            
        }

        public void OnValidate() {

            this.DoValidate();
            
        }

        public override string ToString() {

            return string.Empty;

        }

    }

    public abstract class ParticleViewSource<T> : ParticleViewSourceBase where T : ParticleViewBase, new() {

        public T data;
        
        public override IView<TEntityInner> GetSource<TEntityInner>() {

            return (IView<TEntityInner>)this.data;

        }

        public override void DoValidate() {
            
            var filters = this.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
            var renderers = this.GetComponentsInChildren<UnityEngine.Renderer>(true);

            this.data = new T();
            this.data.SetItems(this.transform.position, this.transform.rotation.eulerAngles, this.transform.localScale, filters, renderers);

        }

        public override string ToString() {

            return this.data.ToString();

        }
        
    }

}
