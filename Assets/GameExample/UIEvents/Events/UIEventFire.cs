using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.GameExample.Game.UI {

    public class UIEventFire : UIEventBase {

        public void Do() {

            this.game.world.AddMarker(new ME.Example.Game.Components.UI.UIFire() {
                viewSourceId = this.game.explosionSourceId,
            });
            
        }

    }

}
