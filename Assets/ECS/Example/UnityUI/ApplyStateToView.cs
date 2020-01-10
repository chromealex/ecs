using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyStateToView : MonoBehaviour {

    public Game game;
    public int entityId;
    public TMPro.TMP_Text text;

    private float prevUnitsCount;
    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            Point data;
            if (this.game.world.GetEntityData(this.entityId, out data) == true) {

                if (this.prevUnitsCount != data.unitsCount) {
                
                    this.text.text = data.unitsCount.ToString();
                    this.prevUnitsCount = data.unitsCount;

                }
                
            }

        }

    }

}
