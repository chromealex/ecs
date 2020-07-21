using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private UnityEditorInternal.ReorderableList.Defaults styleDefaults;
        
        public override void OnInspectorGUI() {

            ME.ECSEditor.GUILayoutExt.CollectEditors<IGraphGUIEditor<Graph>, GraphCustomEditorAttribute>(ref this.graphEditors, System.Reflection.Assembly.GetExecutingAssembly());
            
            var target = (Pathfinding)this.target;

            if (this.styleDefaults == null) {
                
                this.styleDefaults = new UnityEditorInternal.ReorderableList.Defaults();
                
            }

            {
                GUILayout.Label(string.Empty, this.styleDefaults.headerBackground, GUILayout.ExpandWidth(true));
                var rect = GUILayoutUtility.GetLastRect();
                rect.x += 4f;
                GUI.Label(rect, "Graphs");
            }

            GUILayout.BeginVertical(this.styleDefaults.boxBackground);
            {

                for (int i = 0; i < target.graphs.Count; ++i) {

                    GUILayout.BeginHorizontal();
                    GUILayout.Width(60f);
                    GUILayout.BeginVertical();
                    
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

                        }, onHeader: (rect) => {

                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Remove"), false, () => {

                                //Undo.RecordObject(target, "Remove Graph");
                                target.graphs.Remove(graph);
                                GameObject.DestroyImmediate(graph.gameObject);
                                SceneView.RepaintAll();
                                EditorUtility.SetDirty(this.target);

                            });
                            menu.DropDown(rect);

                        });
                        ((GraphEditor)editor).foldout = state;

                    } else {

                        GUILayout.Label("No editor found for " + graph.GetType());

                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }

            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(this.styleDefaults.footerBackground);
                var rect = GUILayoutUtility.GetRect(50f, 16f);
                if (GUI.Button(rect, this.styleDefaults.iconToolbarPlusMore, this.styleDefaults.preButton) == true) {
                    
                    var menu = new GenericMenu();
                    foreach (var graph in this.graphEditors) {

                        var g = graph;
                        if (g.Key.IsAbstract == true) continue;
                        menu.AddItem(new GUIContent(graph.Key.Name), false, () => {
                            
                            var go = new GameObject("Graph", g.Key);
                            go.transform.SetParent(target.transform);
                            var comp = (Graph)go.GetComponent(g.Key);
                            comp.pathfinding = target;
                            target.graphs.Add(comp);
                            SceneView.RepaintAll();
                            EditorUtility.SetDirty(this.target);
                            
                        });

                    }
                    menu.DropDown(rect);
                    
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();

            ME.ECSEditor.GUILayoutExt.Separator();
            
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
            
            ME.ECSEditor.GUILayoutExt.Separator();

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
