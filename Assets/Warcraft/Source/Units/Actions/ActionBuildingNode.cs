using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ME.ECS;
using Warcraft.Entities;
using Warcraft.Components;

[CreateAssetMenu(menuName = "Actions Graph/Actions/Building")]
public class ActionBuildingNode : ActionsGraphNode {

    public bool isUpgrade;
    public UnitInfo building;

    public override Sprite GetIcon() {

        if (this.building == null) return base.GetIcon();
        
        return this.building.icon;

    }
    
    public override bool IsOpened(Warcraft.Entities.PlayerEntity playerEntity) {
        
        var world = Worlds<Warcraft.WarcraftState>.currentWorld;
        var playerBuildingsComponent = world.GetComponent<PlayerEntity, PlayerUnitsComponent>(playerEntity.entity);
        return playerBuildingsComponent.HasUnit(this.building.unitTypeId);

    }
    
}
