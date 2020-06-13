#if VIEWS_MODULE_SUPPORT
using UnityEditor;
using ME.ECS.Views;

namespace ME.ECSEditor {

    [CanEditMultipleObjects]
    public class ViewBaseEditor : Editor {

        public void DrawInspectorGUI(ME.ECS.Entity entity, bool drawDefaultInspector = false) {

            var isMultiple = false;
            string output = string.Empty;
            if (this.targets.Length > 1) {

                foreach (var t in this.targets) {

                    var str = t.ToString();
                    if (output != string.Empty && output != str) {

                        isMultiple = true;

                    }

                    output = str;

                }

            }

            if (entity != ME.ECS.Entity.Empty) {

                GUILayoutExt.Box(2f, 2f, () => {

                    EditorGUILayout.HelpBox(isMultiple == true ? "-" : this.target.ToString(), MessageType.Info);

                    if (isMultiple == false) {

                        GUILayoutExt.DrawEntitySelection(ME.ECS.Worlds.currentWorld, in entity, checkAlive: true);

                    }

                }, UnityEngine.GUIStyle.none);
                GUILayoutExt.Separator();

            }

            EditorGUILayout.BeginHorizontal();
            UnityEngine.GUILayout.FlexibleSpace();
            
            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button);
            style.fontSize = 12;
            style.fixedWidth = 230;
            style.fixedHeight = 23;            
            if (UnityEngine.GUILayout.Button("Refresh Data", style) == true) {

                var targets = this.targets;
                foreach (var target in targets) {
                    
                    ((IDoValidate)target).DoValidate();
                    UnityEditor.EditorUtility.SetDirty(target);
    
                }
                
            }

            UnityEngine.GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (drawDefaultInspector == true) {

                EditorGUILayout.Space();

                this.DrawDefaultInspector();

            }

        }

    }

}
#endif