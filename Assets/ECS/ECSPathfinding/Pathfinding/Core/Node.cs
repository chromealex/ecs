using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    [System.Serializable]
    public abstract class Node {

        [System.Serializable]
        public struct Connection {

            public static Connection NoConnection => new Connection() { index = -1 };
            
            public int index;
            public float cost;

        }

        public Graph graph;
        public int index;
        public Vector3 worldPosition;
        public float penalty;
        public bool walkable;
        public int area;
        public int tag;

        internal readonly Node[] parent = new Node[Pathfinding.THREADS_COUNT];
        internal readonly float[] startToCurNodeLen = new float[Pathfinding.THREADS_COUNT];
        internal readonly bool[] isOpened = new bool[Pathfinding.THREADS_COUNT];
        internal readonly bool[] isClosed = new bool[Pathfinding.THREADS_COUNT];

        public Node() {}

        public virtual void CopyFrom(Node other) {

            this.graph = other.graph;
            this.index = other.index;
            this.worldPosition = other.worldPosition;
            this.penalty = other.penalty;
            this.walkable = other.walkable;
            this.area = other.area;
            this.tag = other.tag;

        }
        
        public virtual Connection[] GetConnections() {

            return null;

        }
        
        internal virtual void Reset(int threadIndex) {

            this.parent[threadIndex] = null;
            this.startToCurNodeLen[threadIndex] = 0f;
            this.isOpened[threadIndex] = false;
            this.isClosed[threadIndex] = false;
            
        }

        public virtual bool IsSuitable(Constraint constraint) {

            if (constraint.checkWalkability == true && this.walkable != constraint.walkable) return false;
            if (constraint.checkArea == true && (constraint.areaMask & (1 << this.area)) == 0) return false;
            if (constraint.checkTags == true && (constraint.tagsMask & (1 << this.tag)) == 0) return false;
            if (constraint.graphMask >= 0 && (constraint.graphMask & (1 << this.graph.index)) == 0) return false;

            if (constraint.tagsMask > 0L &&
                (constraint.agentSize.x > 0f ||
                 constraint.agentSize.y > 0f ||
                 constraint.agentSize.z > 0f)) {

                var result = PoolList<Node>.Spawn(1);
                this.graph.GetNodesInBounds(result, new Bounds(this.worldPosition, constraint.agentSize));
                for (int e = 0, cnt = result.Count; e < cnt; ++e) {

                    var node = result[e];
                    var constraintErosion = constraint;
                    constraintErosion.agentSize = Vector3.zero;
                    if (node.IsSuitable(constraintErosion) == false) return false;

                }
                PoolList<Node>.Recycle(ref result);

            }
                
            return true;

        }
            
    }

}