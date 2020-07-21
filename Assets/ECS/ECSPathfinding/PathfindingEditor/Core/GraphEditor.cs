using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ME.ECS.Pathfinding.Editor {

    [CustomEditor(typeof(Graph), editorForChildClasses: true)]
    public class UnityGraphEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            
            ME.ECSEditor.GUILayoutExt.Box(4f, 4f, () => {
                
                GUILayout.Label("This class is a part of Pathfinding component. To change values use Pathfinding component.");
                
            });
            
        }

    }

    public interface IGraphGUIEditor<T> : ME.ECSEditor.IGUIEditor<T> {
    }

    public class GraphCustomEditorAttribute : ME.ECSEditor.CustomEditorAttribute {

        public GraphCustomEditorAttribute(System.Type type, int order = 0) : base(type, order) {
            
        }

    }
    
    [GraphCustomEditor(typeof(Graph))]
    public class GraphEditor : IGraphGUIEditor<Graph> {

        private Dictionary<Graph, UnityEditorInternal.ReorderableList> list = new Dictionary<Graph, UnityEditorInternal.ReorderableList>();
        
        public bool foldout {
            get {
                return EditorPrefs.GetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".Foldout", false);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".Foldout", value);
            }
        }
        
        public Graph target { get; set; }
        
        public T GetTarget<T>() {
            return (T)(object)this.target;
        }

        public virtual bool OnDrawGUI() {

            ME.ECSEditor.GUILayoutExt.Box(6f, 2f, () => {

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("Current State: " + this.target.buildingState.ToString(), EditorStyles.miniLabel);
                        if (this.target.buildingState == BuildingState.Built) {

                            GUILayout.Label("Nodes count: " + (this.target.nodes != null ? this.target.nodes.Count.ToString() : "-"), EditorStyles.miniLabel);

                        }
                    }
                    GUILayout.EndVertical();
                    if (GUILayout.Button("Build") == true) {

                        this.target.DoBuild();
                        SceneView.RepaintAll();

                    }
                }
                GUILayout.EndHorizontal();

            });
            
            this.target.graphName = EditorGUILayout.TextField("Graph Name", this.target.graphName);
            this.target.graphCenter = EditorGUILayout.Vector3Field("Center", this.target.graphCenter);

            if (this.list.TryGetValue(this.target, out var list) == false) {
                
                list = new UnityEditorInternal.ReorderableList(this.target.modifiers, typeof(Pathfinding.ModificatorItem), true, true, true, true);
                list.drawElementCallback = this.OnDrawListItem;
                list.drawHeaderCallback = this.OnDrawHeader;
                list.onChangedCallback = this.OnChanged;
                list.elementHeightCallback = this.GetElementHeight;
                
                this.list.Add(this.target, list);

            }
            
            list.DoLayoutList();
            
            return false;

        }

        private float GetElementHeight(int index) {
            
            return 18f;

        }

        private void OnDrawHeader(Rect rect) {
            
            GUI.Label(rect, "Graph Modifiers");
            
        }

        private void OnChanged(UnityEditorInternal.ReorderableList reorderableList) {
            
            EditorUtility.SetDirty(this.target);
            
        }

        private void OnDrawListItem(Rect rect, int index, bool isActive, bool isFocused) {

            var data = (Pathfinding.ModificatorItem)this.list[this.target].list[index];
            rect.height = 18f;
            
            var rectCheckBox = new Rect(rect);
            rectCheckBox.width = 20f;

            var isDirty = false;
            {

                rect.y += 1f;
                rect.height -= 2f;
                
                var rectObjectField = new Rect(rect);
                rectObjectField.x += rectCheckBox.width;
                rectObjectField.width -= rectCheckBox.width;
            
                var oldColor = GUI.color;
                if (data.enabled == false) {

                    GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.5f);

                }

                var obj = (GraphModifierBase)EditorGUI.ObjectField(rectObjectField, data.modifier, typeof(GraphModifierBase), allowSceneObjects: true);
                if (obj != data.modifier) {

                    data.modifier = obj;

                    var count = 0;
                    for (int i = 0; i < this.list[this.target].count; ++i) {
                        
                        var mod = (Pathfinding.ModificatorItem)this.list[this.target].list[i];
                        if (mod.modifier != null && data.modifier != null && data.modifier == mod.modifier) {

                            ++count;

                        }
                        
                    }

                    if (count > 1) {

                        data.modifier = null;

                    }
                    
                    isDirty = true;

                }

                GUI.color = oldColor;

                if (data.modifier == null) {
                    
                    data.enabled = false;
                    
                }
                
                EditorGUI.BeginDisabledGroup(data.modifier == null);
                var flag = GUI.Toggle(rectCheckBox, data.enabled, string.Empty);
                if (flag != data.enabled) {

                    data.enabled = flag;
                    isDirty = true;

                }
                EditorGUI.EndDisabledGroup();

            }

            if (isDirty == true) {
                
                EditorUtility.SetDirty(this.target);
                
            }
            
        }

    }

}
