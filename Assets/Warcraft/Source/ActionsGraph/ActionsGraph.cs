using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions Graph/Graph")]
public class ActionsGraph : ScriptableObject {

    public ActionsGraphNode[] allNodes;
    public ActionsGraphNode[] roots;

}
