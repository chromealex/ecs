using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    public class PathCornerModifierSeeker : PathModifierSeeker {

        public PathCornersModifier modifier;

        public override TMod GetModifier<TMod>() {

            return (TMod)(object)this.modifier;

        }

    }
    
    public struct PathCornersModifier : IPathModifier {
        
        public Path Run(Path path, Constraint constraint) {

            ref var nodes = ref path.nodes;
            var corners = PoolList<Node>.Spawn(10);
            var prevDir = -1;
            for (int i = 0; i < nodes.Count - 1; ++i) {

                var node = nodes[i];
                var next = nodes[i + 1];
                var dir = 0;
                var connections = node.GetConnections();
                for (int j = 0; j < connections.Length; ++j) {

                    if (connections[j].index == next.index) {

                        dir = j;
                        break;
                        
                    }
                    
                }

                if (prevDir != dir) {
                    
                    corners.Add(node);
                    prevDir = dir;

                }

            }
            corners.Add(nodes[nodes.Count - 1]);

            for (int iter = 0; iter < corners.Count; ++iter) {

                for (int i = 0; i < corners.Count - 2; ++i) {

                    var c = corners[i];
                    var next = corners[i + 2];
                    var allWalkable = true;
                    for (float d = 0f; d < 1f; d += 0.01f) {

                        var pos = Vector3.Lerp(c.worldPosition, next.worldPosition, d);
                        var node = path.graph.GetNearest(pos);
                        if (node.walkable == false ||
                            node.penalty != c.penalty ||
                            node.IsSuitable(constraint) == false) {

                            allWalkable = false;
                            break;

                        }

                    }

                    if (allWalkable == true) {

                        if (i + 1 < corners.Count) {
                        
                            corners.RemoveAt(i + 1);
        
                        }
                        
                    }

                    //var distance = (next.worldPosition - c.worldPosition).magnitude;
                    /*if (Physics.CapsuleCast(
                            c.worldPosition,
                            c.worldPosition + Vector3.up * this.agentHeight,
                            this.agentRadius,
                            next.worldPosition - c.worldPosition,
                            distance,
                            this.collisionMask
                        ) == false) {
                        
                        nodes.RemoveAt(i + 1);
                        --i;

                    }*/

                }

            }

            path.nodesModified = corners;

            return path;

        }

    }

}
