using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {
    
    [ExecuteInEditMode]
    public class GraphDynamicModifier : MonoBehaviour {

        public bool modifyWalkability;
        public bool walkable;
        public Bounds bounds;

        private Bounds prevBounds;
        private Vector3 prevPosition;
        private bool prevModifyWalkability;
        private bool prevWalkable;

        public void OnEnable() {

            if (Pathfinding.active == null) return;

            Pathfinding.active.RegisterDynamic(this);

        }

        public void OnDisable() {
            
            if (Pathfinding.active == null) return;

            Pathfinding.active.UnRegisterDynamic(this);
            
        }

        public bool Apply() {

            if (Pathfinding.active == null) return false;

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
            
            if (Pathfinding.active == null) return;

            var prevBounds = this.prevBounds;
            prevBounds.center += this.prevPosition;
            var prevNodes = PoolList<Node>.Spawn(10);
            Pathfinding.active.GetNodesInBounds(prevNodes, prevBounds);
            foreach (var node in prevNodes) {

                if (this.prevModifyWalkability == true) {
                    
                    node.walkable = true;
                    Pathfinding.active.BuildNodePhysics(node);

                }

            }

            if (disabled == false) {

                var bounds = this.bounds;
                bounds.center += this.transform.position;
                var nodes = PoolList<Node>.Spawn(10);
                Pathfinding.active.GetNodesInBounds(nodes, bounds);
                foreach (var node in nodes) {

                    if (this.modifyWalkability == true) {

                        node.walkable = this.walkable;
                        Pathfinding.active.BuildNodePhysics(node);

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
