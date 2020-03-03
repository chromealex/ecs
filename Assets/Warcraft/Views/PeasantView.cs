using ME.ECS;
using UnityEngine;
using Warcraft.Features;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using Warcraft.Entities;
    using Warcraft.Components;
    using Warcraft.Components.PeasantStates;
    
    public class PeasantView : CharacterView {

        public DirectionSprites walkWithGoldSprites;
        public DirectionSprites walkWithWoodSprites;

        protected override ref DirectionSprites GetWalkState(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            if (world.HasComponent<UnitEntity, PeasantCarryGold>(data.entity) == true) {

                return ref this.walkWithGoldSprites;

            } else if (world.HasComponent<UnitEntity, PeasantCarryWood>(data.entity) == true) {

                return ref this.walkWithWoodSprites;

            }
            
            return ref base.GetWalkState(in data);

        }

        protected override bool IsIdle(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, PeasantIdleState>(data.entity);
            
        }

        protected override bool IsWalk(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, PeasantGoToCastleState>(data.entity) || world.HasComponent<UnitEntity, PeasantGoToWorkState>(data.entity);

        }

        protected override bool IsDead(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, UnitDeathState>(data.entity);

        }

        protected override bool IsAttack(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, PeasantWorkingState>(data.entity);

        }

    }
    
}