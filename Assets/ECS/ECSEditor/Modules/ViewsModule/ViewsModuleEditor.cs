#if VIEWS_MODULE_SUPPORT
using ME.ECS.Views;

namespace ME.ECSEditor {

    [CustomEditor(typeof(IViewComponent))]
    public class ViewComponentEditor : ME.ECSEditor.IGUIEditor<IViewComponent> {

        public IViewComponent target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        void IGUIEditorBase.OnDrawGUI() {
                
            UnityEngine.GUILayout.Label("Prefab Source Id: " + this.target.GetViewInfo().prefabSourceId.ToString());
                
        }

    }

    [CustomEditor(typeof(IViewModuleBase))]
    public class ViewsModuleEditor : ME.ECSEditor.IGUIEditor<IViewModuleBase> {

        public IViewModuleBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        void IGUIEditorBase.OnDrawGUI() {
                
            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label);
            style.richText = true;

            var renderersCount = 0;
            var data = this.target.GetData();
            if (data != null) {

                foreach (var views in data) {

                    if (views == null) continue;
                    renderersCount += ((System.Collections.IList)views).Count;

                }

            }

            UnityEngine.GUILayout.Label("<b>Alive Views:</b> " + renderersCount.ToString(), style);
            
        }

    }

}
#endif