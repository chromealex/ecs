using UnityEngine;
using Warcraft.Entities;
using Warcraft.Components;

[CreateAssetMenu(menuName = "Info/Tower Unit Info")]
public class TowerUnitInfo : UnitInfo {

    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);

        world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterAutoTarget>(unitEntity);

    }

    public override void OnUpgrade(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnUpgrade(world, unitEntity);

        world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterAutoTarget>(unitEntity);

    }

}