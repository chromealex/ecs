using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {
    
    [ExecuteInEditMode]
    public class GraphDynamicModifier : MonoBehaviour {

        public Pathfinding pathfinding;
        
        public bool modifyWalkability;
        public bool walkable;
        public Bounds bounds;

        private Bounds prevBounds;
        private Vector3 prevPosition;
        private bool prevModifyWalkability;
        private bool prevWalkable;

        public void OnEnable() {

            this.pathfinding.RegisterDynamic(this);

        }

        public void OnDisable() {
            
            this.pathfinding.UnRegisterDynamic(this);
            
        }

        public bool Apply() {

            if (this.pathfinding == null) return false;

            if (this.prevPosition != this.transform.position ||
                this.prevBounds != this.bounds ||
                this.prevModifyWalkability != this.modifyWalkability ||
                this.prevWalkable != this.walkable) {

                this.ApplyForced();
                
                this.prevPosition = this.transform.position;
                this.prevBounds = this.bounds;
                this.prevModifyWalkability = this.modifyWalkability;
                this.prevWalkable = this.walkable;

                return true;

            }

            return false;

        }
        
        public void ApplyForced(bool disabled = false) {
            
            if (this.pathfinding == null) return;

            var prevBounds = this.prevBounds;
            prevBounds.center += this.prevPosition;
            var prevNodes = PoolList<Node>.Spawn(10);
            this.pathfinding.GetNodesInBounds(prevNodes, prevBounds);
            foreach (var node in prevNodes) {

                if (this.prevModifyWalkability == true) {
                    
                    node.walkable = true;
                    this.pathfinding.BuildNodePhysics(node);

                }

            }

            if (disabled == false) {

                var bounds = this.bounds;
                bounds.center += this.transform.position;
                var nodes = PoolList<Node>.Spawn(10);
                this.pathfinding.GetNodesInBounds(nodes, bounds);
                foreach (var node in nodes) {

                    if (this.modifyWalkability == true) {

                        node.walkable = this.walkable;
                        this.pathfinding.BuildNodePhysics(node);

                    }

                }

            }

        }
        
        public void OnDrawGizmos() {

            var bounds = this.bounds;
            bounds.center += this.transform.position;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        }

    }

}
