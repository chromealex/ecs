using UnityEngine;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ME.ECS;

public class Game : MonoBehaviour {

    public World<State> world;
    
    private IState<State> savedState;

    private RPCId testCallId;
    
    public void Update() {

        if (Input.GetKeyDown(KeyCode.A) == true) {

            WorldUtilities.CreateWorld(ref this.world, 0.033f);
            this.world.AddModule<StatesHistoryModule>();
            this.world.AddModule<NetworkModule>();
            var network = this.world.GetModule<NetworkModule>();
            this.testCallId = network.RegisterRPC(new System.Action<int, Vector3>(this.TestCall_RPC).Method);
            network.RegisterObject(this, 1);

            this.world.SetState(this.world.CreateState());
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();
            this.world.SaveResetState();

        }

        if (Input.GetKeyDown(KeyCode.Z) == true) {

            WorldUtilities.CreateWorld(ref this.world, 1f);
            this.world.AddModule<StatesHistoryModule>();
            this.world.AddModule<NetworkModule>();
            var network = this.world.GetModule<NetworkModule>();
            this.testCallId = network.RegisterRPC(new System.Action<int, Vector3>(this.TestCall_RPC).Method);
            network.RegisterObject(this, 1);
            
            this.world.SetState(this.world.CreateState());
            this.world.SetCapacity<Point>(100000);
            for (int i = 0; i < 100000; ++i) {
                this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f }, updateFilters: false);
            }
            this.world.UpdateFilters<Point>();
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();
            this.world.SaveResetState();

        }

        if (Input.GetKeyDown(KeyCode.R) == true) {
            
            var newState = (IState<State>)this.world.CreateState();
            newState.Initialize(this.world, freeze: true, restore: false);
            newState.CopyFrom((State)this.savedState);
            this.world.SetState((State)newState);
            ((IWorldBase)this.world).Simulate(newState.tick);

        }

        if (Input.GetKeyDown(KeyCode.P) == true) {

            var historyModule = this.world.GetModule<StatesHistoryModule>();
            var state = historyModule.GetStateBeforeTick(this.world.GetTick());
            Debug.Log("Tick: " + this.world.GetTick() + ", State Tick: " + state.tick);

        }

        if (Input.GetKey(KeyCode.F) == true) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.testCallId, 1, Vector3.one);
            
            /*
            var historyModule = this.world.GetModule<StatesHistoryModule>();
            
            var evt = new ME.ECS.StatesHistory.HistoryEvent() {
                tick = (ulong)Random.Range(0, this.world.GetTick()),
                order = 0,
                id = 123,
            };
            historyModule.AddEvent(evt);
            Debug.Log("Add Event for tick: " + evt.tick + ", order: " + evt.order + ", method: " + evt.id);
            */

        }

        if (Input.GetKeyDown(KeyCode.T) == true) {

            /*var historyModule = this.world.GetModule<StatesHistoryModule>();
            var evt = new ME.ECS.StatesHistory.HistoryEvent() {
                tick = 0UL,
                order = 0,
            };
            historyModule.AddEvent(evt);
            Debug.Log("Add Event for tick: " + evt.tick + ", order: " + evt.order + ", method: " + evt.id);
            */

        }

        if (Input.GetKeyDown(KeyCode.S) == true) {
            
            var state = this.world.GetState();
            this.savedState = this.world.CreateState();
            this.savedState.Initialize(this.world, freeze: true, restore: false);
            this.savedState.CopyFrom(state);
            
        }

        if (this.world != null) {

            var dt = Time.deltaTime;
            this.world.Update(dt);

        }

    }

    public void TestCall_RPC(int p1, Vector3 p2) {

        var count = this.world.GetRandomRange(1, 5);
        for (int i = 0; i < count; ++i) {

            //Debug.LogError("Called: " + p1 + "; " + p2);
            //this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(1));
            this.world.AddComponent<Point, IncreaseUnitsOnce>(Entity.Create<Point>(1));

        }

    }

}
