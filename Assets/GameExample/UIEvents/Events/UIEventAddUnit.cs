using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.GameExample.Game.UI {

    public class UIEventAddUnit : UIEventBase {

        public bool topSpawn;
        
        public void Do() {

            this.game.world.AddMarker(new ME.Example.Game.Components.UI.UIAddUnit() {
                color = this.game.playerColor,
                count = this.game.spawnUnitsCount,
                viewSourceId = this.game.unitViewSourceId,
                viewSourceId2 = this.game.unitViewSourceId2,
                topSpawn = this.topSpawn,
            });
            
        }

    }

}
