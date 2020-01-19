using ME.ECS;

public class PointSetColor : IComponentOnce<State, Point> {

    public UnityEngine.Color color;
    
    public void AdvanceTick(State state, ref Point data, float deltaTime, int index) {

        data.color = this.color;
        
    }

    void IComponent<State, Point>.CopyFrom(IComponent<State, Point> other) {

        this.color = ((PointSetColor)other).color;

    }
    
}