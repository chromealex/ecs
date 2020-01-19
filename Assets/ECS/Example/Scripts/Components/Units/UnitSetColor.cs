using ME.ECS;

public class UnitSetColor : IComponentOnce<State, Unit> {

    public UnityEngine.Color color;
    
    public void AdvanceTick(State state, ref Unit data, float deltaTime, int index) {

        data.color = this.color;
        
    }

    void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

        this.color = ((UnitSetColor)other).color;

    }
    
}