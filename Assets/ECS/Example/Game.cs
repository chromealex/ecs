using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EntityId = System.Int32;
using ME.ECS;

public class Game : MonoBehaviour {

    public World<State> world;
    
    private IState<State> savedState;
    
    public void Update() {

        if (Input.GetKeyDown(KeyCode.A) == true) {

            this.world = new World<State>();
            this.world.SetState(new State());
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            this.world.AddSystem(new InputSystem());
            this.world.AddSystem(new PointsSystem());

        }

        if (Input.GetKeyDown(KeyCode.Q) == true) {

            this.world.AddComponent<IncreaseUnits, Point>(Entity.Create<Point>(1));

        }

        if (Input.GetKeyDown(KeyCode.Z) == true) {

            this.world = new World<State>();
            this.world.SetState(new State());
            this.world.SetCapacity<Point>(1000000);
            for (int i = 0; i < 1000000; ++i) {
                this.world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f }, updateFilters: false);
            }
            this.world.UpdateFilters<Point>();
            this.world.AddSystem(new InputSystem());
            this.world.AddSystem(new PointsSystem());

        }

        if (Input.GetKeyDown(KeyCode.R) == true) {
            
            var newState = (IState<State>)new State();
            newState.Initialize(this.world, freeze: true, restore: false);
            newState.CopyFrom((State)this.savedState);
            this.world.SetState((State)newState);

        }

        if (Input.GetKeyDown(KeyCode.S) == true) {
            
            var state = this.world.GetState();
            this.savedState = new State();
            this.savedState.Initialize(this.world, freeze: true, restore: false);
            this.savedState.CopyFrom(state);
            
        }

        if (this.world != null) {

            var dt = Time.deltaTime;
            this.world.Update(dt);

        }

    }

}
