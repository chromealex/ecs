using ME.ECS;

public class SetPosition : IComponent<State, Point> {

    public UnityEngine.Vector3 position;
    
    public Point AdvanceTick(State state, Point data, float deltaTime, int index) {

        data.position += this.position * deltaTime;
        return data;

    }

    void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) {

        this.position = ((SetPosition)other).position;

    }
    
}