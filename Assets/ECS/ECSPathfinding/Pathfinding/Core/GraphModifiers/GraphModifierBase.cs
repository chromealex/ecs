using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Pathfinding {
    
    public abstract class GraphModifierBase : MonoBehaviour {

        public abstract void ApplyAfterConnections(Graph graph);

        public abstract void ApplyBeforeConnections(Graph graph);

        public virtual void OnDrawGizmos() {}

    }

}
