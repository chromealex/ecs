using ME.ECS;

public class UnitSetColor : IComponentOnce<State, Unit> {

    public UnityEngine.Color color;
    
    public Unit AdvanceTick(State state, Unit data, float deltaTime, int index) {

        data.color = this.color;
        return data;

    }

    void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

        this.color = ((UnitSetColor)other).color;

    }
    
}