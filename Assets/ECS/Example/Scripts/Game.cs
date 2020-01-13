using UnityEngine;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ME.ECS;

public class Game : MonoBehaviour {

    public int worldId;
    public int worldConnectionId;
    [Range(0, 100)]
    public int dropPercent;
    public float deltaTimeMultiplier = 1f;
    public Color playerColor;
    public float moveSide;
    
    public World<State> world;
    private IState<State> savedState;

    public void Update() {

        if (Input.GetKeyDown(KeyCode.A) == true && this.world == null) {

            // Loading level
            
            WorldUtilities.CreateWorld(ref this.world, 0.133f, this.worldId);
            this.world.AddModule<StatesHistoryModule>();
            this.world.AddModule<NetworkModule>();

            if (this.worldConnectionId > 0) {

                var network = this.world.GetModule<NetworkModule>();
                network.SetWorldConnection(this.worldConnectionId);
                network.SetDropPercent(this.dropPercent);

            }

            this.world.SetState(this.world.CreateState());
            var p1 = this.world.AddEntity(new Point() { position = new Vector3(0f, 0f, 3f), unitsCount = 99f, increaseRate = 1f });
            var p2 = this.world.AddEntity(new Point() { position = new Vector3(0f, 0f, -3f), unitsCount = 1f, increaseRate = 1f });
            /*{ // Add unit
                var unitSpeed = 1f;
                var unit = this.world.AddEntity(new Unit() { position = new Vector3(0f, 0f, 0f), rotation = Quaternion.identity, speed = unitSpeed, pointFrom = p1, pointTo = p2 });
                var followComponent = this.world.AddComponent<Unit, UnitFollowFromTo>(unit);
                followComponent.@from = p1;
                followComponent.to = p2;
            }*/
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();
            this.world.AddSystem<UnitsSystem>();
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
                network.SetDropPercent(this.dropPercent);

            }

            this.world.SetState(this.world.CreateState());
            this.world.SetCapacity<Point>(1000000);
            for (int i = 0; i < 1000000; ++i) {
                this.world.AddEntity(new Point() { position = Vector3.zero, unitsCount = 99f, increaseRate = 1f }, updateFilters: false);
            }
            this.world.UpdateFilters<Point>();
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();
            this.world.AddSystem<UnitsSystem>();
            this.world.SaveResetState();

        }

        if (this.world != null) {

            var dt = Time.deltaTime * this.deltaTimeMultiplier;
            this.world.Update(dt);

        }

    }

    public void AddEventUIButtonClick(int pointId) {

        var input = this.world.GetSystem<InputSystem>();
        input.AddEventUIButtonClick(pointId, this.playerColor, this.moveSide);

    }

    public void AddUnitButtonClick() {
        
        var input = this.world.GetSystem<InputSystem>();
        input.AddUnitButtonClick(this.playerColor);
        
    }

}
