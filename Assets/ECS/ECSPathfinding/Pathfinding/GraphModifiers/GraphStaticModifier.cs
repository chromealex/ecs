using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {
    
    public class GraphStaticModifier : GraphModifierBase {
    
        new public int tag;
        public int penaltyDelta;
        public bool modifyWalkability;
        public bool walkable;
        public LayerMask layerMask;
        public Bounds bounds;

        public override void ApplyAfterConnections(Graph graph) {
            
            var nodes = PoolList<Node>.Spawn(10);
            var bounds = this.bounds;
            bounds.center += this.transform.position;
            graph.GetNodesInBounds(nodes, this.bounds);
            foreach (var node in nodes) {

                if (this.modifyWalkability == true) {
                    
                    node.walkable = this.walkable;
                    
                }

            }

        }

        public override void ApplyBeforeConnections(Graph graph) {

            var layer = this.gameObject.layer;
            var bounds = this.bounds;
            bounds.center += this.transform.position;

            var nodes = PoolList<Node>.Spawn(10);
            graph.GetNodesInBounds(nodes, bounds);
            foreach (var node in nodes) {
        
                var ray = new Ray(node.worldPosition + Vector3.up * 10f, Vector3.down);
                if (Physics.Raycast(ray, out var hit, 1000f, this.layerMask) == true) {

                    if (hit.collider.gameObject.layer == layer) {

                        var dt = this.penaltyDelta;
                        if (dt < 0) {

                            node.penalty -= (uint)(-this.penaltyDelta);

                        } else {
                        
                            node.penalty += (uint)this.penaltyDelta;
                        
                        }

                        node.tag = this.tag;

                    }
            
                }
        
            }
            PoolList<Node>.Recycle(ref nodes);

        }

        public override void OnDrawGizmos() {

            var bounds = this.bounds;
            bounds.center += this.transform.position;
            
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        }

    }

}
