using ME.ECS;
using UnityEngine;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using Warcraft.Components;
    using Warcraft.Entities;

    public class GoldMineView : UnitView {

        public Sprite emptySprite;
        public Sprite workingSprite;

        public override void ApplyState(in UnitEntity data, float deltaTime, bool immediately) {
            
            var compMax = Worlds<WarcraftState>.currentWorld.GetComponent<UnitEntity, UnitCountAtWorkPlace>(data.entity);
            if (compMax != null) {

                this.spriteRenderer.sprite = (compMax.count > 0 ? this.workingSprite : this.emptySprite);

            }

            base.ApplyState(in data, deltaTime, immediately);
            
        }

    }
    
}