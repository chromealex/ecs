using ME.ECS;
using UnityEngine;

public class InputSystem : ISystem<State> {

    public IWorld<State> world { get; set; }
    
    void ISystem<State>.OnConstruct() { }
    void ISystem<State>.OnDeconstruct() { }

    void ISystem<State>.AdvanceTick(State state, float deltaTime) {
        
    }
    
    void ISystem<State>.Update(State state, float deltaTime) {
        
        if (Input.GetKeyDown(KeyCode.Q) == true) {

            this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(1));
            this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(2));

        }

    }

}
