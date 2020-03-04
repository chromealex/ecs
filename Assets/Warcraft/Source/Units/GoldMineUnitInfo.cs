using Warcraft.Entities;
using Warcraft.Components;

public class GoldMineUnitInfo : UnitInfo {

    public override void OnCreate(ME.ECS.IWorld<Warcraft.WarcraftState> world, ME.ECS.Entity unitEntity) {
        
        base.OnCreate(world, unitEntity);

        var c = world.AddComponent<UnitEntity, GoldMineComponent>(unitEntity);
        c.capacity = 10000;
        var cMax = world.AddComponent<UnitEntity, GoldMineUnitsMaxPerMine>(unitEntity);
        cMax.current = 0;
        cMax.max = 10;

    }

}
