using UnityEditor;
using ME.ECS.Views.Providers;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(MonoBehaviourViewBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class MonoBehaviourViewBaseEditor : ViewBaseEditor {

        public override void OnInspectorGUI() {
            
            this.DrawInspectorGUI(drawDefaultInspector: true);
            
        }

    }

}
