using UnityEditor;
using ME.ECS.Views.Providers;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(DrawMeshViewSourceBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class DrawMeshViewSourceBaseEditor : ViewBaseEditor {

        public override void OnInspectorGUI() {

            this.DrawInspectorGUI(drawDefaultInspector: false);

        }

    }

}
