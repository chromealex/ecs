using UnityEditor;
using ME.ECS.Views.Providers;

namespace ME.ECSEditor {

    [UnityEditor.CustomEditor(typeof(ParticleViewSourceBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ParticleViewSourceBaseEditor : Editor {

        public override void OnInspectorGUI() {

            var isMultiple = false;
            var _target = (ParticleViewSourceBase)this.target;
            if (this.targets.Length > 1) {

                isMultiple = true;

            }

            EditorGUILayout.HelpBox(isMultiple == true ? "-" : _target.ToString(), MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            UnityEngine.GUILayout.FlexibleSpace();
            
            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button);
            style.fontSize = 12;
            style.fixedWidth = 230;
            style.fixedHeight = 23;            
            if (UnityEngine.GUILayout.Button("Refresh Data", style) == true) {

                var targets = this.targets;
                foreach (var target in targets) {
                    
                    ((ParticleViewSourceBase)target).DoValidate();
                    
                }

            }

            UnityEngine.GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
        }

    }

}
