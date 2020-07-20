using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ME.ECS.Pathfinding.Editor {

    [GraphCustomEditor(typeof(GridGraph))]
    public class GridGraphEditor : GraphEditor, IGraphGUIEditor<Graph> {

        public bool showGrid {
            get {
                return EditorPrefs.GetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".showGrid", true);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".showGrid", value);
            }
        }

        public bool showAgent {
            get {
                return EditorPrefs.GetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".showAgent", true);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".showAgent", value);
            }
        }

        public bool showEditor {
            get {
                return EditorPrefs.GetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".showEditor", true);
            }
            set {
                EditorPrefs.SetBool("ME.ECS.Pathfinding.Graph." + this.target.index + ".showEditor", value);
            }
        }

        new public GridGraph target {
            get {
                return (GridGraph)base.target;
            }
        }

        new public T GetTarget<T>() {
            return (T)(object)this.target;
        }
        
        public override bool OnDrawGUI() {

            base.OnDrawGUI();

            var state = this.showGrid;
            ME.ECSEditor.GUILayoutExt.FoldOut(ref state, "Grid", () => {
                
                ME.ECSEditor.GUILayoutExt.Box(6f, 2f, () => {
                    
                    this.target.size = EditorGUILayout.Vector3IntField("Grid Size", this.target.size);
                    this.target.nodeSize = EditorGUILayout.FloatField("Node Size", this.target.nodeSize);
                    
                    this.target.initialPenalty = EditorGUILayout.FloatField("Initial Penalty", this.target.initialPenalty);
                    this.target.diagonalCostFactor = EditorGUILayout.FloatField("Diagonal Cost Factor", this.target.diagonalCostFactor);
                    this.target.connectionsType = (GridGraph.ConnectionsType)EditorGUILayout.EnumPopup("Connections Type", this.target.connectionsType);
                    
                });
                
            });
            this.showGrid = state;

            state = this.showAgent;
            ME.ECSEditor.GUILayoutExt.FoldOut(ref state, "Agent", () => {
                
                ME.ECSEditor.GUILayoutExt.Box(6f, 2f, () => {
                    
                    this.target.agentHeight = EditorGUILayout.FloatField("Agent Height", this.target.agentHeight);
                    
                    this.target.checkMask = ME.ECSEditor.GUILayoutExt.DrawLayerMaskField("Ground Mask", this.target.checkMask);
                    this.target.collisionMask = ME.ECSEditor.GUILayoutExt.DrawLayerMaskField("Collision Mask", this.target.collisionMask);
                    this.target.collisionCheckRadius = EditorGUILayout.FloatField("Collision Check Radius", this.target.collisionCheckRadius);

                });
                
            });
            this.showAgent = state;
            
            state = this.showEditor;
            ME.ECSEditor.GUILayoutExt.FoldOut(ref state, "Editor", () => {
                
                ME.ECSEditor.GUILayoutExt.Box(6f, 2f, () => {
                    
                    this.target.drawMode = (GridGraph.DrawMode)EditorGUILayout.EnumPopup("Draw Mode", this.target.drawMode);

                    if (this.target.drawMode == GridGraph.DrawMode.Penalty) {

                        ME.ECSEditor.GUILayoutExt.DrawGradient(10f, Color.green, Color.red, this.target.minPenalty.ToString("0"), this.target.maxPenalty.ToString("0"));

                    }
                    
                    this.target.drawNonwalkableNodes = EditorGUILayout.ToggleLeft("Draw Non-walkable Nodes", this.target.drawNonwalkableNodes);
                    this.target.drawConnections = EditorGUILayout.ToggleLeft("Draw Connections", this.target.drawConnections);
                    this.target.drawConnectionsToUnwalkable = EditorGUILayout.ToggleLeft("Draw Connections to Non-walkable", this.target.drawConnectionsToUnwalkable);

                });
                
            });
            this.showEditor = state;
            
            return false;

        }

    }

}
