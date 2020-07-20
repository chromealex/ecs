using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    public enum BuildingState : byte {

        NotBuilt = 0,
        Building,
        Built,

    }
    
    public abstract class Graph : MonoBehaviour {

        public int index;
        public string graphName;
        
        public Vector3 graphCenter;

        public BuildingState buildingState;
        public Node[] nodes;
        public List<Pathfinding.ModificatorItem> modifiers = new List<Pathfinding.ModificatorItem>();

        public float minPenalty { get; private set; }
        public float maxPenalty { get; private set; }

        public T GetNodeByIndex<T>(int index) where T : Node {

            if (index < 0 || index >= this.nodes.Length) return null;

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

        public abstract void GetNodesInBounds(List<Node> output, Bounds bounds);

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

            System.Diagnostics.Stopwatch sw = null;
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) sw = System.Diagnostics.Stopwatch.StartNew();
            
            this.buildingState = BuildingState.Building;
            
            this.minPenalty = float.MaxValue;
            this.maxPenalty = float.MinValue;

            System.Diagnostics.Stopwatch swBuildNodes = null;
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBuildNodes = System.Diagnostics.Stopwatch.StartNew();

            this.Validate();
            this.BuildNodes();

            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBuildNodes.Stop();

            System.Diagnostics.Stopwatch swBeforeConnections = null;
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBeforeConnections = System.Diagnostics.Stopwatch.StartNew();

            this.RunModifiersBeforeConnections();
            
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBeforeConnections.Stop();
            
            System.Diagnostics.Stopwatch swBuildConnections = null;
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBuildConnections = System.Diagnostics.Stopwatch.StartNew();

            for (var i = 0; i < this.nodes.Length; ++i) {

                var p = this.nodes[i].penalty;
                if (p < this.minPenalty) this.minPenalty = p;
                if (p > this.maxPenalty) this.maxPenalty = p;

            }

            this.BuildConnections();
            
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBuildConnections.Stop();

            System.Diagnostics.Stopwatch swAfterConnections = null;
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swAfterConnections = System.Diagnostics.Stopwatch.StartNew();

            this.RunModifiersAfterConnections();
            
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swAfterConnections.Stop();

            System.Diagnostics.Stopwatch swBuildAreas = null;
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBuildAreas = System.Diagnostics.Stopwatch.StartNew();

            this.BuildAreas();
            
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) swBuildAreas.Stop();

            this.buildingState = BuildingState.Built;
            
            if (Pathfinding.active.HasLogLevel(LogLevel.GraphBuild) == true) {

                Logger.Log(string.Format("Graph built {0} nodes in {1}ms:\nBuild Nodes: {2}ms\nBefore Connections: {3}ms\nBuild Connections: {4}ms\nAfter Connections: {5}ms\nBuild Areas: {6}ms", this.nodes.Length, sw.ElapsedMilliseconds, swBuildNodes.ElapsedMilliseconds, swBeforeConnections.ElapsedMilliseconds, swBuildConnections.ElapsedMilliseconds, swAfterConnections.ElapsedMilliseconds, swBuildAreas.ElapsedMilliseconds));

            }
            
        }

        public abstract bool BuildNodePhysics(Node node);
        protected abstract void Validate();
        protected abstract void BuildNodes();
        protected abstract void RunModifiersBeforeConnections();
        protected abstract void BuildConnections();
        protected abstract void RunModifiersAfterConnections();
        public abstract void BuildAreas();

        public void DoDrawGizmos() {

            this.DrawGizmos();
            
        }

        protected abstract void DrawGizmos();

    }

}