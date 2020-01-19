using ME.ECS;

public class PointIncreaseUnits : IComponent<State, Point> {

    public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

        data.unitsCount += data.increaseRate * deltaTime;

    }

    void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) {}

}

public class PointIncreaseUnitsOnce : IComponentOnce<State, Point> {

    public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

        data.unitsCount += data.increaseRate * deltaTime;

    }
    
    void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) {}
    
}
