using ME.ECS;
using UnityEngine;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using Warcraft.Entities;

    public class BuildingView : UnitView {

        public Sprite[] destroyProgressSprites;

        public override void ApplyState(in UnitEntity data, float deltaTime, bool immediately) {
            
            
            
            base.ApplyState(in data, deltaTime, immediately);
            
        }

    }
    
}