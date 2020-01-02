using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : ISystem<State> {

    public IWorld<State> world { get; set; }
    
    void ISystem<State>.AdvanceTick(State state, float deltaTime) {
        
    }

}
