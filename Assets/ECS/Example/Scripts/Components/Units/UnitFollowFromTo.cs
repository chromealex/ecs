using ME.ECS;

public class UnitFollowFromTo : IComponent<State, Unit> {

    public Entity from;
    public Entity to;
    
    public void AdvanceTick(State state, ref Unit data, float deltaTime, int index) {

        Point toData;
        if (this.GetEntityData(this.to.id, out toData) == true) {
    
            data.position = UnityEngine.Vector3.MoveTowards(data.position, toData.position, data.speed * deltaTime);

        }

    }

    void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

        var otherUnit = ((UnitFollowFromTo)other);
        this.from = otherUnit.from;
        this.to = otherUnit.to;

    }
    
}