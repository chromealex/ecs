using ME.ECS;

namespace Warcraft.Components {

    public class UnitHealthComponent : IComponent<WarcraftState, Warcraft.Entities.UnitEntity> {

        public int maxValue;
        public int value;

    }
    
}