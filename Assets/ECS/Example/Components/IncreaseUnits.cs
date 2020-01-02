using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseUnits : IComponent<State, Point> {
    
    public Point AdvanceTick(State state, Point data, float deltaTime, int index) {

        data.unitsCount += data.increaseRate * deltaTime;
        return data;

    }
    
}
