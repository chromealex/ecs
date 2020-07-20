using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    public struct Constraint {

        public static Constraint Empty => new Constraint() {
            graphMask = -1,
        };

        public static Constraint Default => new Constraint() {
            checkWalkability = true,
            walkable = true,
            graphMask = -1,
        };

        public Vector3 agentSize;

        public bool checkArea;
        public long areaMask;

        public bool checkTags;
        public long tagsMask;
        
        public bool checkWalkability;
        public bool walkable;

        public long graphMask;

    }

}