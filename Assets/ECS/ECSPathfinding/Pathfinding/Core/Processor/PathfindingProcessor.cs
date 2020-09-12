using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {
    
    using ME.ECS.Collections;

    public class PathfindingProcessor {
        
        public Path Run<TMod>(Pathfinding pathfinding, Vector3 from, Vector3 to, Constraint constraint, Graph graph, TMod pathModifier, int threadIndex = 0) where TMod : IPathModifier {

            if (threadIndex < 0 || threadIndex > Pathfinding.THREADS_COUNT) threadIndex = 0;

            var constraintStart = constraint;
            constraintStart.checkWalkability = true;
            constraintStart.walkable = true;
            var startNode = graph.GetNearest(from, constraintStart);
            if (startNode == null) return new Path();

            var constraintEnd = constraintStart;
            constraintEnd.checkArea = true;
            constraintEnd.areaMask = (1 << startNode.area);
            
            var endNode = graph.GetNearest(to, constraintEnd);
            if (endNode == null) return new Path();
            
            var visited = PoolList<Node>.Spawn(10);
            System.Diagnostics.Stopwatch swPath = null;
            if (pathfinding.HasLogLevel(LogLevel.Path) == true) swPath = System.Diagnostics.Stopwatch.StartNew();
            var nodesPath = this.AstarSearch(graph, visited, startNode, endNode, constraint, threadIndex);

            var statVisited = visited.Count;
            var statLength = 0;
            
            var path = new Path();
            path.graph = graph;
            path.result = PathCompleteState.NotCalculated;

            if (nodesPath == null) {

                path.result = PathCompleteState.NotExist;

            } else {

                statLength = nodesPath.Count;
                
                path.result = PathCompleteState.Complete;
                path.nodes = nodesPath;

            }
            
            for (int i = 0; i < visited.Count; ++i) {

                visited[i].Reset(threadIndex);

            }

            PoolList<Node>.Recycle(ref visited);

            System.Diagnostics.Stopwatch swModifier = null;
            if (pathfinding.HasLogLevel(LogLevel.PathMods) == true) swModifier = System.Diagnostics.Stopwatch.StartNew();
            if (path.result == PathCompleteState.Complete) {

                path = pathModifier.Run(path, constraint);

            }
            
            if (pathfinding.HasLogLevel(LogLevel.Path) == true) {
                
                Logger.Log(string.Format("Path result {0}, built in {1}ms. Path length: {2} (visited: {3})\nThread Index: {4}", path.result, swPath.ElapsedMilliseconds, statLength, statVisited, threadIndex));
                
            }

            if (pathfinding.HasLogLevel(LogLevel.PathMods) == true) {

                Logger.Log(string.Format("Path Mods: {0}ms", swModifier.ElapsedMilliseconds));

            }

            return path;

        }

        protected ListCopyable<Node> AstarSearch(Graph graph, ListCopyable<Node> visited, Node startNode, Node endNode, Constraint constraint, int threadIndex) {
            
            var openList = PoolQueue<Node>.Spawn(10);
            
            startNode.startToCurNodeLen[threadIndex] = 0f;
            
            openList.Enqueue(startNode);
            startNode.isOpened[threadIndex] = true;

            while (openList.Count > 0) {
                
                var node = openList.Dequeue();
                node.isClosed[threadIndex] = true;
                
                visited.Add(node);

                if (node.index == endNode.index) {
                    
                    PoolQueue<Node>.Recycle(ref openList);
                    return this.RetracePath(threadIndex, endNode);
                    
                }

                var neighbors = node.GetConnections();
                foreach(var conn in neighbors) {
                    
                    if (conn.index < 0) continue;
                    
                    var neighbor = graph.nodes[conn.index];
                    if (neighbor.isClosed[threadIndex] == true) continue;
                    if (neighbor.IsSuitable(constraint) == false) continue;
                    
                    float ng = node.startToCurNodeLen[threadIndex] + conn.cost; 
                    if (neighbor.isOpened[threadIndex] == false || ng < neighbor.startToCurNodeLen[threadIndex]) {
                        
                        neighbor.startToCurNodeLen[threadIndex] = ng;
                        neighbor.parent[threadIndex] = node;
                        if (neighbor.isOpened[threadIndex] == false) {
                            
                            openList.Enqueue(neighbor);
                            visited.Add(neighbor);
                            neighbor.isOpened[threadIndex] = true;
                            
                        }
                        
                    }
                    
                }
                
            }

            PoolQueue<Node>.Recycle(ref openList);
            return null;

        }
        
        private ListCopyable<Node> RetracePath(int threadIndex, Node endNode) {
            
            var path = PoolList<Node>.Spawn(10);
            path.Add(endNode);
            while (endNode.parent[threadIndex] != null) {
                
                endNode = endNode.parent[threadIndex];
                path.Add(endNode);
                
            }
            path.Reverse();
            return path;
            
        }

    }

}
