using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions Graph/Actions/Group")]
public class ActionGroupNode : ActionsGraphNode {

    public ActionsGraph actionsGraph;
    
    public override ActionsGraphNode[] GetNext() {

        if (this.actionsGraph != null) return this.actionsGraph.roots;
        
        return base.GetNext();

    }
    
}
