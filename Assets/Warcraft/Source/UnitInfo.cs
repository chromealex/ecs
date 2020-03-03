using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ME.ECS.Views.Providers;
using Warcraft.Views;

[System.Serializable]
public struct ResourcesStorage {

    public int gold;
    public int wood;

}

[CreateAssetMenu]
public class UnitInfo : ScriptableObject {

    public int unitTypeId;
    public UnitView viewSource;
    public Sprite icon;

    public ActionsGraph actions;

    public float buildingTime = 5f;
    public bool walkable;
    public bool hasEntrancePoint = true;
    public Vector2Int entrancePoint;

    public virtual void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
    }

    public virtual void OnUpgrade(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
    }

}
