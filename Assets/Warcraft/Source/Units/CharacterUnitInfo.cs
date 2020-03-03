using UnityEngine;
using Warcraft.Entities;
using Warcraft.Components;

[CreateAssetMenu(menuName = "Info/Character Unit Info")]
public class CharacterUnitInfo : UnitInfo {

    public float movementSpeed = 0.5f;
    
    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);

        var speedComponent = world.AddComponent<UnitEntity, UnitSpeedComponent>(unitEntity);
        speedComponent.speed = this.movementSpeed;
        
        world.AddComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(unitEntity);
        world.AddComponent<UnitEntity, CharacterComponent>(unitEntity);

    }

}
