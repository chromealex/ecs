using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    public enum PathCompleteState {

        NotCalculated = 0,
        Complete,
        NotExist,

    }
 
    public struct Path {

        public PathCompleteState result;
        public Graph graph;
        public List<Node> nodes;
        public List<Node> nodesModified;

        public void Recycle() {
            
            if (this.nodes != null) PoolList<Node>.Recycle(ref this.nodes);
            if (this.nodesModified != null) PoolList<Node>.Recycle(ref this.nodesModified);
            
        }

    }

}