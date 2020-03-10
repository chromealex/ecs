using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPath : MonoBehaviour {

    public Transform target;

    public void Update() {

        if (this.target == null) return;
        
        var path = Pathfinding.ABPath.Construct(this.transform.position, this.target.position);
        AstarPath.StartPath(path);
        AstarPath.BlockUntilCalculated(path);

        var nodes = path.path;
        for (int i = 0; i < nodes.Count - 1; ++i) {
            
            Debug.DrawLine((Vector3)nodes[i].position, (Vector3)nodes[i + 1].position);
            
        }

    }

}
