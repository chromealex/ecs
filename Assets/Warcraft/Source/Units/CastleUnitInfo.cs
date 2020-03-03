using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Warcraft.Entities;
using Warcraft.Components;

public class CastleUnitInfo : UnitInfo {

    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);
        
        world.AddComponent<UnitEntity, CastleComponent>(unitEntity);
        
    }

}
