using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.Example.Game.UI {

    public class UnitCounter : MonoBehaviour {

        public Game game;
        public TMPro.TMP_Text text;
        public float updatePerSeconds = 0.1f;
        private float updateTimer;

        public void LateUpdate() {

            this.updateTimer += Time.deltaTime;
            if (this.updateTimer >= this.updatePerSeconds) {

                this.UpdateInfo();
                this.updateTimer -= this.updatePerSeconds;

            }

        }

        private void UpdateInfo() {

            if (this.game != null && this.game.world != null) {

                var world = (ME.ECS.IWorld<State>)this.game.world;
                var tick = world.GetStateTick();
                var count = this.game.world.GetState().unitsCount;

                this.text.text = "Tick: " + tick +
                                 "\nUnits Count: " + count;

            }

        }

    }

}