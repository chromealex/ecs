using ME.ECS;
using EntityId = System.Int32;

public class State : IState<State> {

    public EntityId entityId { get; set; }

    public Filter<Point> points;
    public Filter<Unit> units;

    public Components<Point, State> pointComponents;
    public Components<Unit, State> unitComponents;

    void IState<State>.Initialize(IWorld<State> world, bool freeze, bool restore) {

        world.Register(ref this.points, freeze, restore);
        world.Register(ref this.units, freeze, restore);

        world.Register(ref this.pointComponents, freeze, restore);
        world.Register(ref this.unitComponents, freeze, restore);
        
    }

    void IState<State>.CopyFrom(State other) {

        this.entityId = other.entityId;
        
        this.points.CopyFrom(other.points);
        this.units.CopyFrom(other.units);
        
        this.pointComponents.CopyFrom(other.pointComponents);
        this.unitComponents.CopyFrom(other.unitComponents);

    }

    void IPoolableRecycle.OnRecycle() {

        WorldUtilities.Release(ref this.points);
        WorldUtilities.Release(ref this.units);
        
        WorldUtilities.Release(ref this.pointComponents);
        WorldUtilities.Release(ref this.unitComponents);

    }

}
