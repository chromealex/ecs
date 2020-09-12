using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {
    
    using ME.ECS.Collections;

    [ExecuteInEditMode]
    #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Pathfinding : MonoBehaviour {

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Serializable]
        public sealed class ModificatorItem {

            public bool enabled;
            public GraphModifierBase modifier;
            
        }

        public const int THREADS_COUNT = 8;

        public PathfindingProcessor processor = new PathfindingProcessor();
        public List<Graph> graphs;

        public LogLevel logLevel;
        
        private HashSet<GraphDynamicModifier> dynamicModifiers = new HashSet<GraphDynamicModifier>();

        private struct CopyGraph : IArrayElementCopy<Graph> {

            public void Copy(Graph @from, ref Graph to) {
                
                to.CopyFrom(from);
                
            }

            public void Recycle(Graph item) {

                item.Recycle();
                
            }

        }

        public Pathfinding Clone() {

            var instance = Object.Instantiate(this);
            for (int i = 0; i < this.graphs.Count; ++i) {

                this.graphs[i].pathfinding = instance;

            }
            instance.CopyFrom(this);
            return instance;

        }

        public void Recycle() {

            if (this != null && this.gameObject != null) {
                
                Object.Destroy(this.gameObject);
                
            }
            
        }

        private void OnRecycle() {
            
            for (int i = 0; i < this.graphs.Count; ++i) {

                this.graphs[i].Recycle();

            }

        }
        
        public void CopyFrom(Pathfinding other) {

            this.processor = other.processor;
            this.logLevel = other.logLevel;

            ArrayUtils.Copy(other.graphs, ref this.graphs, new CopyGraph());
            
        }
        
        public bool HasLogLevel(LogLevel level) {

            return (this.logLevel & level) != 0;

        }
        
        public void RegisterDynamic(GraphDynamicModifier modifier) {

            if (this.dynamicModifiers.Contains(modifier) == false) {

                if (this.dynamicModifiers.Add(modifier) == true) {
                    
                    modifier.ApplyForced();
                    this.BuildAreas();
                    
                }
                
            }

        }

        public void UnRegisterDynamic(GraphDynamicModifier modifier) {

            if (this.dynamicModifiers.Contains(modifier) == true) {

                if (this.dynamicModifiers.Remove(modifier) == true) {
                    
                    modifier.ApplyForced(disabled: true);
                    this.BuildAreas();

                }
                
            }
            
        }

        public void AdvanceTick(float deltaTime) {

            var anyUpdated = false;
            foreach (var mod in this.dynamicModifiers) {
                
                anyUpdated |= mod.Apply();
                
            }

            if (anyUpdated == true) {
                
                this.BuildAreas();

            }
            
        }

        public Node GetNearest(Vector3 worldPosition) {

            return this.GetNearest(worldPosition, Constraint.Default);

        }

        public Node GetNearest(Vector3 worldPosition, Constraint constraint) {

            Node nearest = null;
            if (this.graphs != null) {

                float dist = float.MaxValue;
                for (int i = 0; i < this.graphs.Count; ++i) {

                    var node = this.graphs[i].GetNearest(worldPosition, constraint);
                    if (node == null) continue;
                    
                    var d = (node.worldPosition - worldPosition).sqrMagnitude;
                    if (d < dist) {

                        dist = d;
                        nearest = node;

                    }

                }

            }

            return nearest;

        }

        public Path CalculatePath(Vector3 from, Vector3 to) {

            var constraint = Constraint.Default;
            return this.CalculatePath(from, to, constraint);
            
        }

        public Path CalculatePath(Vector3 from, Vector3 to, Constraint constraint) {

            return this.CalculatePath(from, to, constraint, new PathModifierEmpty());
            
        }

        public Path CalculatePath<TMod>(Vector3 from, Vector3 to, TMod pathModifier) where TMod : IPathModifier {

            var constraint = Constraint.Default;
            return this.CalculatePath(from, to, constraint, pathModifier);
            
        }

        public Path CalculatePath<TMod>(Vector3 from, Vector3 to, Constraint constraint, TMod pathModifier) where TMod : IPathModifier {

            var graph = this.GetNearest(from, constraint).graph;
            return this.CalculatePath(from, to, constraint, graph, pathModifier);
            
        }

        public Path CalculatePath<TMod>(Vector3 from, Vector3 to, Constraint constraint, Graph graph, TMod pathModifier) where TMod : IPathModifier {

            return this.processor.Run(this, from, to, constraint, graph, pathModifier);

        }

        public void BuildAreas() {
            
            if (this.graphs != null) {

                for (int i = 0; i < this.graphs.Count; ++i) {

                    this.graphs[i].BuildAreas();

                }

            }
            
        }
        
        public bool BuildNodePhysics(Node node) {
            
            return node.graph.BuildNodePhysics(node);
            
        }
        
        public void GetNodesInBounds(ListCopyable<Node> result, Bounds bounds) {
            
            if (this.graphs != null) {

                for (int i = 0; i < this.graphs.Count; ++i) {

                    this.graphs[i].GetNodesInBounds(result, bounds);

                }

            }
            
        }
        
        public void BuildAll() {

            if (this.graphs != null) {

                for (int i = 0; i < this.graphs.Count; ++i) {

                    this.graphs[i].DoBuild();

                }

            }

        }

        public void OnDrawGizmos() {

            if (this.graphs != null) {

                for (int i = 0; i < this.graphs.Count; ++i) {

                    this.graphs[i].DoDrawGizmos();

                }

            }

        }

    }

}
