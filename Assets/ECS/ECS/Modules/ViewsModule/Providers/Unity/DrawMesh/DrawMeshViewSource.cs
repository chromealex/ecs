#if DRAWMESH_VIEWS_MODULE_SUPPORT
namespace ME.ECS.Views.Providers {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class DrawMeshViewSourceBase : ViewBase, IDoValidate {

        public abstract IView GetSource();

        [UnityEngine.ContextMenu("Validate")]
        public virtual void DoValidate() {
            
        }

        public void OnValidate() {

            this.DoValidate();
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class DrawMeshViewSource<T> : DrawMeshViewSourceBase where T : DrawMeshViewBase, new() {

        public T data;
        
        public override IView GetSource() {

            return (IView)this.data;

        }

        public override void DoValidate() {
            
            base.DoValidate();
            
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
#endif