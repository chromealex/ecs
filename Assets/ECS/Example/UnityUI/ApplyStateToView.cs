using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyStateToView : MonoBehaviour {

    public Game game;
    public int entityId;
    public TMPro.TMP_Text text;

    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            Point data;
            if (this.game.world.GetEntityData(this.entityId, out data) == true) {

                this.text.text = data.unitsCount.ToString();

            }

        }

    }

}
