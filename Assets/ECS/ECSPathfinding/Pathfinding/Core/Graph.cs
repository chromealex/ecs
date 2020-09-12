using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    using ME.ECS.Collections;
    
    public enum BuildingState : byte {

        NotBuilt = 0,
        Building,
        Built,

    }
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class Graph : MonoBehaviour {

        public Pathfinding pathfinding;
        
        public int index;
        public string graphName;
        
        public Vector3 graphCenter;

        public BuildingState buildingState;
        public List<Node> nodes;
        public List<Pathfinding.ModificatorItem> modifiers = new List<Pathfinding.ModificatorItem>();

        public float minPenalty { get; private set; }
        public float maxPenalty { get; private set; }

        public virtual void OnRecycle() {

            this.pathfinding = null;
            this.index = default;
            this.graphName = default;
            this.graphCenter = default;
            this.buildingState = default;
            this.minPenalty = default;
            this.maxPenalty = default;
            
        }

        public abstract void Recycle();
        
        public void CopyFrom(Graph other) {

            this.pathfinding = other.pathfinding;
            this.index = other.index;
            this.graphName = other.graphName;
            this.graphCenter = other.graphCenter;
            this.buildingState = other.buildingState;
            this.minPenalty = other.minPenalty;
            this.maxPenalty = other.maxPenalty;
            
            this.OnCopyFrom(other);
            
        }

        public abstract void OnCopyFrom(Graph other);
        
        public virtual T AddNode<T>() where T : Node, new() {

            var node = PoolClass<T>.Spawn();
            node.graph = this;
            node.index = this.nodes.Count;
            this.nodes.Add(node);

            return node;

        }

        public virtual void RemoveNode(ref Node node, bool bruteForceConnections = false) {

            if (bruteForceConnections == true) {
                
                // Brute force all connections from all nodes and remove them to this node
                for (int i = 0; i < this.nodes.Count; ++i) {

                    var connections = this.nodes[i].GetConnections();
                    for (int j = 0; j < connections.Length; ++j) {

                        var connection = connections[j];
                        if (connection.index == node.index) {

                            connections[j] = Node.Connection.NoConnection;

                        }
                        
                    }
                    
                }
                
            } else {

                // Remove all connections to this node from neighbours only
                var connections = node.GetConnections();
                for (int i = 0; i < connections.Length; ++i) {

                    var connection = connections[i];
                    if (connection.index >= 0) {

                        var connectedTo = this.nodes[connection.index].GetConnections();
                        for (int j = 0; j < connectedTo.Length; ++j) {

                            if (connectedTo[j].index == node.index) {

                                connectedTo[j] = Node.Connection.NoConnection;

                            }

                        }

                    }

                }

            }

            // Remove node from list
            this.nodes.RemoveAt(node.index);
            
        }

        public T GetNodeByIndex<T>(int index) where T : Node {

            if (index < 0 || index >= this.nodes.Count) return null;

            return (T)this.nodes[index];

        }

        public Node GetNearest(Vector3 worldPosition) {

            return this.GetNearest(worldPosition, Constraint.Default);

        }

        public abstract Node GetNearest(Vector3 worldPosition, Constraint constraint);

        public T GetNearest<T>(Vector3 worldPosition) where T : Node {

            return this.GetNearest<T>(worldPosition, Constraint.Default);

        }

        public abstract T GetNearest<T>(Vector3 worldPosition, Constraint constraint) where T : Node;

        public abstract void GetNodesInBounds(ListCopyable<Node> output, Bounds bounds);

        private Dictionary<int, Color> areaColors = new Dictionary<int, Color>();
        protected Color GetAreaColor(int area) {

            if (this.areaColors.TryGetValue(area, out var color) == false) {

                color = this.GetSColor();
                color.a = 0.4f;
                this.areaColors.Add(area, color);
                return color;

            } else {

                return this.areaColors[area];

            }
            
        }

        protected void FloodFillAreas(Node node, int area) {

            var connections = node.GetConnections();
            for (int j = 0; j < connections.Length; ++j) {
                
                var connection = connections[j];
                if (connection.index >= 0) {

                    var nb = this.nodes[connection.index];
                    if (nb.area == 0 && nb.walkable == true) {

                        nb.area = area;
                        this.FloodFillAreas(nb, area);

                    }

                }

            }

        }

        protected Color GetSColor() {

            var rgb = new Vector3Int();
            rgb[0] = Random.Range(0, 256);  // red
            rgb[1] = Random.Range(0, 256);  // green
            rgb[2] = Random.Range(0, 256);  // blue
            
            int max, min;
            if (rgb[0] > rgb[1]) {
                
                max = (rgb[0] > rgb[2]) ? 0 : 2;
                min = (rgb[1] < rgb[2]) ? 1 : 2;
                
            } else {
                
                max = (rgb[1] > rgb[2]) ? 1 : 2;
                int notmax = 1 + max % 2;
                min = (rgb[0] < rgb[notmax]) ? 0 : notmax;
                
            }
            rgb[max] = 255;
            rgb[min] = 0;

            return new Color32((byte)rgb[0], (byte)rgb[1], (byte)rgb[2], 255);

        }
        
        protected Color GetPenaltyColor(float penalty) {

            var min = this.minPenalty;
            var max = this.maxPenalty;
            
            var from = new Color(0f, 1f, 0f, 0.5f);
            var to = new Color(1f, 0f, 0f, 0.5f);

            var t = Mathf.Clamp01((penalty - min) / (min == max ? 1f : max - min));
            return Color.Lerp(from, to, t);

        }

        public void DoBuild() {

            var pathfinding = this.pathfinding;
            
            System.Diagnostics.Stopwatch sw = null;
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) sw = System.Diagnostics.Stopwatch.StartNew();
            
            this.buildingState = BuildingState.Building;
            
            this.minPenalty = float.MaxValue;
            this.maxPenalty = float.MinValue;

            System.Diagnostics.Stopwatch swBuildNodes = null;
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBuildNodes = System.Diagnostics.Stopwatch.StartNew();

            this.Validate();
            this.BuildNodes();

            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBuildNodes.Stop();

            System.Diagnostics.Stopwatch swBeforeConnections = null;
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBeforeConnections = System.Diagnostics.Stopwatch.StartNew();

            this.RunModifiersBeforeConnections();
            
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBeforeConnections.Stop();
            
            System.Diagnostics.Stopwatch swBuildConnections = null;
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBuildConnections = System.Diagnostics.Stopwatch.StartNew();

            for (var i = 0; i < this.nodes.Count; ++i) {

                var p = this.nodes[i].penalty;
                if (p < this.minPenalty) this.minPenalty = p;
                if (p > this.maxPenalty) this.maxPenalty = p;

            }

            this.BuildConnections();
            
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBuildConnections.Stop();

            System.Diagnostics.Stopwatch swAfterConnections = null;
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swAfterConnections = System.Diagnostics.Stopwatch.StartNew();

            this.RunModifiersAfterConnections();
            
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swAfterConnections.Stop();

            System.Diagnostics.Stopwatch swBuildAreas = null;
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBuildAreas = System.Diagnostics.Stopwatch.StartNew();

            this.BuildAreas();
            
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) swBuildAreas.Stop();

            this.buildingState = BuildingState.Built;
            
            if (pathfinding.HasLogLevel(LogLevel.GraphBuild) == true) {

                Logger.Log(string.Format("Graph built {0} nodes in {1}ms:\nBuild Nodes: {2}ms\nBefore Connections: {3}ms\nBuild Connections: {4}ms\nAfter Connections: {5}ms\nBuild Areas: {6}ms", this.nodes.Count, sw.ElapsedMilliseconds, swBuildNodes.ElapsedMilliseconds, swBeforeConnections.ElapsedMilliseconds, swBuildConnections.ElapsedMilliseconds, swAfterConnections.ElapsedMilliseconds, swBuildAreas.ElapsedMilliseconds));

            }
            
        }

        public abstract bool BuildNodePhysics(Node node);
        protected abstract void Validate();
        protected abstract void BuildNodes();
        protected abstract void RunModifiersBeforeConnections();
        protected abstract void BuildConnections();
        protected abstract void RunModifiersAfterConnections();

        public virtual void BuildAreas() {
            
            var area = 0;
            for (int i = 0; i < this.nodes.Count; ++i) {

                this.nodes[i].area = 0;

            }

            for (int i = 0; i < this.nodes.Count; ++i) {

                var node = this.nodes[i];
                if (node.walkable == true && node.area == 0) {

                    var currentArea = ++area;
                    node.area = currentArea;
                    this.FloodFillAreas(node, currentArea);
                    
                }

            }

        }

        public void DoDrawGizmos() {

            this.DrawGizmos();
            
        }

        protected abstract void DrawGizmos();

    }

}