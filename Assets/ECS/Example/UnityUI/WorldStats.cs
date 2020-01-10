using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStats : MonoBehaviour {

    public Game game;
    public TMPro.TMP_Text text;

    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            var world = (ME.ECS.IWorld<State>)this.game.world;
            var tick = world.GetTick();
            var time = world.GetTimeSinceStart();
            var historyModule = world.GetModule<ME.ECS.StatesHistory.IStatesHistoryModule<State>>();
            var eventsAddedCount = historyModule.GetEventsAddedCount();
            var networkModule = world.GetModule<ME.ECS.Network.NetworkModule<State>>();
            var eventsSentCount = networkModule.GetEventsSentCount();
            var eventsReceivedCount = networkModule.GetEventsReceivedCount();

            this.text.text = "World Id: " + world.id.ToString() +
                             "\nDT Multiplier: " + this.game.deltaTimeMultiplier.ToString() +
                             "\nTick: " + tick.ToString() +
                             "\nTime: " + time.ToString() +
                             "\nEvents Added: " + eventsAddedCount.ToString() +
                             "\nSent: " + eventsSentCount.ToString() +
                             "\nReceived: " + eventsReceivedCount.ToString();

        }

    }

}
