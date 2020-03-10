using System.Collections;
using System.Collections.Generic;
using ME.ECS;
using UnityEngine;

public class TestBuildingPosition : MonoBehaviour {

    public Vector2Int size;

    public void OnDrawGizmos() {

        var mapFeature = ME.ECS.Worlds<Warcraft.WarcraftState>.currentWorld.GetFeature<Warcraft.Features.MapFeature>();
        var pos = mapFeature.GetWorldBuildingPosition(this.transform.position, this.size);
        var pos2 = mapFeature.GetWorldBuildingPosition(pos, this.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, pos.XY());
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, pos2.XY());

    }

}
