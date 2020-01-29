using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.GameExample.Game.UI {

    public class UIEventMoveRight : UIEventBase {

        public void Do() {

            this.game.world.AddMarker(new ME.Example.Game.Components.UI.UIMove() {
                color = this.game.playerColor,
                moveSide = -this.game.moveSide,
                pointId = this.game.worldId,
            });
            
        }

    }

}
