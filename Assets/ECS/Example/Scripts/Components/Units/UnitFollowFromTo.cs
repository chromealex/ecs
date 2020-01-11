using ME.ECS;

public class UnitFollowFromTo : IComponent<State, Unit> {

    public Entity from;
    public Entity to;
    
    public Unit AdvanceTick(State state, Unit data, float deltaTime, int index) {

        var world = Worlds<State>.currentWorld;
        Point toData;
        if (world.GetEntityData(this.to.id, out toData) == true) {
    
            data.position = UnityEngine.Vector3.MoveTowards(data.position, toData.position, data.speed * deltaTime);

        }

        return data;

    }

    void IComponent<State, Unit>.CopyFrom(IComponent<State, Unit> other) {

        var otherUnit = ((UnitFollowFromTo)other);
        this.from = otherUnit.from;
        this.to = otherUnit.to;

    }
    
}