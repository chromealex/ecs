using UnityEngine;
using Warcraft.Entities;
using Warcraft.Components;

[CreateAssetMenu(menuName = "Info/Archer Unit Info")]
public class ArcherUnitInfo : CharacterUnitInfo {

    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);

        world.AddComponent<UnitEntity, UnitArcherComponent>(unitEntity);
        
    }

}