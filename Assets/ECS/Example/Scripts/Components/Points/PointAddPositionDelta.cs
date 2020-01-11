using ME.ECS;

public class PointAddPositionDelta : IComponent<State, Point> {

    public UnityEngine.Vector3 positionDelta;
    
    public Point AdvanceTick(State state, Point data, float deltaTime, int index) {

        data.position += this.positionDelta * deltaTime;
        return data;

    }

    void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) {

        this.positionDelta = ((PointAddPositionDelta)other).positionDelta;

    }
    
}