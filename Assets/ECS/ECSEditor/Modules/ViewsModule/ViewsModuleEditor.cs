#if VIEWS_MODULE_SUPPORT
using ME.ECS.Views;

namespace ME.ECSEditor {

    [CustomEditor(typeof(IViewComponent))]
    public class ViewComponentEditor : ME.ECSEditor.IGUIEditor<IViewComponent> {

        public IViewComponent target { get; set; }

        void IGUIEditorBase.OnDrawGUI() {
                
            UnityEngine.GUILayout.Label("Prefab Source Id: " + this.target.GetViewInfo().prefabSourceId.ToString());
                
        }

    }

    [CustomEditor(typeof(IViewModuleBase))]
    public class ViewsModuleEditor : ME.ECSEditor.IGUIEditor<IViewModuleBase> {

        public IViewModuleBase target { get; set; }

        void IGUIEditorBase.OnDrawGUI() {
                
            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label);
            style.richText = true;

            var renderersCount = 0;
            foreach (System.Collections.DictionaryEntry ren in this.target.GetData()) {

                renderersCount += ((System.Collections.IList)ren.Value).Count;

            }
            
            UnityEngine.GUILayout.Label("<b>Alive Views:</b> " + renderersCount.ToString(), style);
            
        }

    }

}
#endif