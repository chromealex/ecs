using UnityEngine;
using EntityId = System.Int32;
using ME.ECS;

public class Game : MonoBehaviour {

    public World<State> world;
    
    private IState<State> savedState;
    
    public void Update() {

        if (Input.GetKeyDown(KeyCode.A) == true) {

            WorldUtilities.CreateWorld(ref this.world, 0.05f);
            this.world.AddModule<StatesHistoryModule>();
            
            this.world.SetState(this.world.CreateState());
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();

        }

        if (Input.GetKeyDown(KeyCode.Q) == true) {

            this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(1));
            this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(2));

        }

        if (Input.GetKeyDown(KeyCode.Z) == true) {

            WorldUtilities.CreateWorld(ref this.world, 1f);
            this.world.AddModule<StatesHistoryModule>();
            
            this.world.SetState(this.world.CreateState());
            this.world.SetCapacity<Point>(100000);
            for (int i = 0; i < 100000; ++i) {
                this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f }, updateFilters: false);
            }
            this.world.UpdateFilters<Point>();
            this.world.AddSystem<InputSystem>();
            this.world.AddSystem<PointsSystem>();

        }

        if (Input.GetKeyDown(KeyCode.R) == true) {
            
            var newState = (IState<State>)this.world.CreateState();
            newState.Initialize(this.world, freeze: true, restore: false);
            newState.CopyFrom((State)this.savedState);
            this.world.SetState((State)newState);

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

}
