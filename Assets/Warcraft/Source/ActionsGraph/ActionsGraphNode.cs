using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ME.ECS;
using Warcraft.Entities;
using Warcraft.Components;

public class ActionsGraphNode : ScriptableObject {

    [SerializeField]
    private ActionsGraphNode[] next;
    [SerializeField]
    private ActionsGraphNode[] requiredToOpen;

    [SerializeField]
    private Sprite icon;
    public ResourcesStorage cost;

    public virtual ActionsGraphNode[] GetNext() {

        return this.next;

    }

    public virtual Sprite GetIcon() {

        return this.icon;

    }

    public virtual bool IsOpened(PlayerEntity playerEntity) {

        return false;

    }

    public virtual bool IsEnabled(PlayerEntity playerEntity) {

        var world = Worlds<Warcraft.WarcraftState>.currentWorld;
        var playerResourcesComponent = world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
        
        var canBuy = this.cost.gold < playerResourcesComponent.resources.gold && this.cost.wood < playerResourcesComponent.resources.wood;
        if (canBuy == true) {

            foreach (var node in this.requiredToOpen) {

                if (node.IsOpened(playerEntity) == false) return false;

            }

        }

        return canBuy;

    }

}
