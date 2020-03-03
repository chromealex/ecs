using UnityEngine;
using Warcraft.Entities;
using Warcraft.Components;

[CreateAssetMenu(menuName = "Info/Peasant Unit Info")]
public class PeasantUnitInfo : CharacterUnitInfo {

    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);

        world.AddComponent<UnitEntity, UnitPeasantComponent>(unitEntity);
        world.AddComponent<UnitEntity, Warcraft.Components.PeasantStates.PeasantIdleState>(unitEntity);

    }

}
