using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    public class Seeker : MonoBehaviour {

        public Pathfinding pathfinding;
        public Graph graph;
        public PathModifierSeeker modifier;

        public Path CalculatePath(Vector3 from, Vector3 to, Constraint constraint) {
            
            return this.pathfinding.CalculatePath(from, to, constraint, this.graph, (this.modifier != null ? this.modifier.GetModifier<IPathModifier>() : null));
            
        }
        
    }

}
