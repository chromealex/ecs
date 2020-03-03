using UnityEngine;
using Warcraft.Entities;
using Warcraft.Components;

[CreateAssetMenu(menuName = "Info/Swordman Unit Info")]
public class SwordmanUnitInfo : CharacterUnitInfo {

    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);

        world.AddComponent<UnitEntity, UnitSwordmanComponent>(unitEntity);
        
    }

}
