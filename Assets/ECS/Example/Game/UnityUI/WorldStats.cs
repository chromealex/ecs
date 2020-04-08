using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.Example.Game.UI {

    public class WorldStats : MonoBehaviour {

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
                var hash = world.GetStateHash();
                var time = world.GetTimeSinceStart();
                var historyModule = world.GetModule<ME.ECS.StatesHistory.IStatesHistoryModule<State>>();
                var eventsAddedCount = historyModule.GetEventsAddedCount();
                var eventsPlayedCount = historyModule.GetEventsPlayedCount();
                var networkModule = world.GetModule<ME.ECS.Network.NetworkModule<State>>();
                var eventsSentCount = networkModule.GetEventsSentCount();
                var eventsReceivedCount = networkModule.GetEventsReceivedCount();

                var roomInfo = string.Empty;
                if (Photon.Pun.PhotonNetwork.InRoom == true) {

                    roomInfo += "Name: " + Photon.Pun.PhotonNetwork.CurrentRoom.Name;
                    roomInfo += ", Players: " + Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount.ToString();

                }

                this.text.text = "Connection Status: " + Photon.Pun.PhotonNetwork.NetworkClientState.ToString() + ", roomInfo: " + roomInfo + 
                                 "\nWorld Id: " + world.id.ToString() +
                                 "\nDT Multiplier: " + this.game.deltaTimeMultiplier.ToString() +
                                 "\nTick: " + tick.ToString() +
                                 "\nState Hash: " + hash.ToString() +
                                 "\nTime: " + time.ToString() +
                                 "\nEvents Added: " + eventsAddedCount.ToString() +
                                 "\nEvents Played: " + eventsPlayedCount.ToString() +
                                 "\nSent: " + eventsSentCount.ToString() +
                                 "\nReceived: " + eventsReceivedCount.ToString() +
                                 "\nSyncTick: " + networkModule.GetSyncTick().ToString() +
                                 "\nSyncTickSent: " + networkModule.GetSyncSentTick().ToString();

            }

        }

    }

}