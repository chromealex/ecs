using UnityEditor;
using ME.ECS.Views.Providers;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(ParticleViewSourceBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ParticleViewSourceBaseEditor : ViewBaseEditor {

        public override void OnInspectorGUI() {

            this.DrawInspectorGUI(drawDefaultInspector: false);

        }

    }

}
