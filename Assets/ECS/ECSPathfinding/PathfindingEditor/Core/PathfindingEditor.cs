using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ME.ECS.Pathfinding.Editor {

    [UnityEditor.CustomEditor(typeof(Pathfinding))]
    public class PathfindingEditor : UnityEditor.Editor {

        public bool editorFoldout {
            get {
                return EditorPrefs.GetBool("ME.ECS.Pathfinding.main.editorFoldout", false);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.Pathfinding.main.editorFoldout", value);
            }
        }
        
        private Dictionary<System.Type, IGraphGUIEditor<Graph>> graphEditors;
        
        public override void OnInspectorGUI() {

            ME.ECSEditor.GUILayoutExt.CollectEditors<IGraphGUIEditor<Graph>, GraphCustomEditorAttribute>(ref this.graphEditors, System.Reflection.Assembly.GetExecutingAssembly());
            
            var target = (Pathfinding)this.target;
            
            for (int i = 0; i < target.graphs.Length; ++i) {

                var graph = target.graphs[i];
                graph.index = i;
                
                if (this.graphEditors.TryGetValue(graph.GetType(), out var editor) == true) {

                    editor.target = graph;
                    
                    var state = ((GraphEditor)editor).foldout;
                    ME.ECSEditor.GUILayoutExt.FoldOut(ref state, graph.graphName, () => {

                        EditorGUI.BeginChangeCheck();
                        editor.OnDrawGUI();
                        if (EditorGUI.EndChangeCheck() == true) {

                            EditorUtility.SetDirty(graph);
                            EditorUtility.SetDirty(this.target);
                            SceneView.RepaintAll();

                        }

                    });
                    ((GraphEditor)editor).foldout = state;

                } else {
                        
                    GUILayout.Label("No editor found for " + graph.GetType());
                        
                }

            }

            { // Editor

                var state = this.editorFoldout;
                ME.ECSEditor.GUILayoutExt.FoldOut(ref state, "Editor", () => {

                    EditorGUI.BeginChangeCheck();
                    target.logLevel = (LogLevel)EditorGUILayout.EnumFlagsField("Log Level", target.logLevel);
                    if (EditorGUI.EndChangeCheck() == true) {

                        EditorUtility.SetDirty(this.target);
                        SceneView.RepaintAll();

                    }

                });
                this.editorFoldout = state;

            }
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Build All", GUILayout.Width(200f), GUILayout.Height(24f)) == true) {

                target.BuildAll();
                SceneView.RepaintAll();

            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
        }

    }

}
