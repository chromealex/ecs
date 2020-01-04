using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStats : MonoBehaviour {

    public Game game;
    public TMPro.TMP_Text text;

    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            var world = (ME.ECS.IWorldBase)this.game.world;
            var tick = world.GetTick();
            var time = world.GetTimeSinceStart();

            this.text.text = "Tick: " + tick.ToString() + "\nTime: " + time.ToString();

        }

    }

}
