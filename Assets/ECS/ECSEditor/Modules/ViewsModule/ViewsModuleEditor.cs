#if VIEWS_MODULE_SUPPORT
using ME.ECS.Views;

namespace ME.ECSEditor {

    [ComponentCustomEditor(typeof(IViewComponent))]
    public class ViewComponentEditor : ME.ECSEditor.IGUIEditor<IViewComponent> {

        public IViewComponent target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {
                
            UnityEngine.GUILayout.Label("Prefab Source Id: " + this.target.GetViewInfo().prefabSourceId.ToString());

            return false;

        }

    }

    [ComponentCustomEditor(typeof(IViewModuleBase))]
    public class ViewsModuleEditor : ME.ECSEditor.IGUIEditor<IViewModuleBase> {

        public IViewModuleBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {
                
            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label);
            style.richText = true;

            var renderersCount = 0;
            var data = this.target.GetData();
            if (data.arr != null) {

                for (int i = 0; i < data.Length; ++i) {

                    var views = data.arr[i];
                    renderersCount += views.Length;

                }

            }

            UnityEngine.GUILayout.Label("<b>Alive Views:</b> " + renderersCount.ToString(), style);

            return false;

        }

    }

}
#endif