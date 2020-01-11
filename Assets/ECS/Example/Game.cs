using UnityEngine;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ME.ECS;

public class Game : MonoBehaviour {

    public int worldId;
    public int worldConnectionId;
    public float deltaTimeMultiplier = 1f;
    
    public World<State> world;
    private IState<State> savedState;

    public void Update() {

        if (Input.GetKeyDown(KeyCode.A) == true && this.world == null) {

            // Loading level
            
            WorldUtilities.CreateWorld(ref this.world, 0.033f, this.worldId);
            this.world.AddModule<StatesHistoryModule>();
            this.world.AddModule<NetworkModule>();

            if (this.worldConnectionId > 0) {

                var network = this.world.GetModule<NetworkModule>();
                network.SetWorldConnection(this.worldConnectionId);

            }

            this.world.SetState(this.world.CreateState());
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();
            this.world.SaveResetState();

        }

        if (Input.GetKeyDown(KeyCode.Z) == true && this.world == null) {

            // Loading level
            
            WorldUtilities.CreateWorld(ref this.world, 1f, this.worldId);
            this.world.AddModule<StatesHistoryModule>();
            this.world.AddModule<NetworkModule>();
            
            if (this.worldConnectionId > 0) {

                var network = this.world.GetModule<NetworkModule>();
                network.SetWorldConnection(this.worldConnectionId);

            }

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

        if (this.world != null) {

            var dt = Time.deltaTime * this.deltaTimeMultiplier;
            this.world.Update(dt);

        }

    }

    public void AddEventUIButtonClick() {

        var input = this.world.GetSystem<InputSystem>();
        input.AddEventUIButtonClick();

    }

}
