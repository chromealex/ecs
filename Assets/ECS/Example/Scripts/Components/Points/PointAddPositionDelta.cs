using ME.ECS;

public class PointAddPositionDelta : IComponentOnce<State, Point> {

    public UnityEngine.Vector3 positionDelta;
    
    public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

        data.position += this.positionDelta * deltaTime;
        
    }

    void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) {

        this.positionDelta = ((PointAddPositionDelta)other).positionDelta;

    }
    
}