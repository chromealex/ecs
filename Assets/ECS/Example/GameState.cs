using ME.ECS;
using EntityId = System.Int32;

public class State : IState<State> {

    public EntityId entityId { get; set; }

    public Filter<Point> points;
    public Filter<Unit> units;

    public Components<Point, State> pointComponents;

    void IState<State>.Initialize(IWorld<State> world, bool freeze, bool restore) {

        world.Register(ref this.points, freeze, restore);
        world.Register(ref this.units, freeze, restore);

        world.Register(ref this.pointComponents, freeze, restore);
        
    }

    void IState<State>.CopyFrom(State other) {

        this.entityId = other.entityId;
        
        this.points.CopyFrom(other.points);
        this.units.CopyFrom(other.units);
        
        this.pointComponents.CopyFrom(other.pointComponents);

    }

}
