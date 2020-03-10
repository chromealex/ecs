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

    public string title;
    
    public int unitTypeId;
    public UnitView viewSource;
    public UnitView viewSourceDead;
    public Sprite icon;

    public float sightRange = 3f;
    public float attackRange;
    public float attackDelay;
    public float attackTime;
    public Vector2Int attackDamageRange;

    public int health;

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
