using ME.ECS;
using Unity.Jobs;

public class PointsSystem : ISystem<State> {

    //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
    private struct TestJob : IJobParallelFor {

        public float deltaTime;
        
        void IJobParallelFor.Execute(int index) {
            
            var data = Worlds<State>.currentWorld.RunComponents(Worlds<State>.currentState.points[index], this.deltaTime, index);
            Worlds<State>.currentState.points[index] = data;
            Worlds<State>.currentWorld.UpdateEntityCache(data);

        }

    }

    public IWorld<State> world { get; set; }

    void ISystem<State>.OnConstruct() { }
    void ISystem<State>.OnDeconstruct() { }

    void ISystem<State>.AdvanceTick(State state, float deltaTime) {

        /*var world = this.world;
        for (int i = 0, count = state.points.Count; i < count; ++i) {

            state.points[i] = world.RunComponents(state.points[i], deltaTime, i);

        }*/

        var job = new TestJob() {
            deltaTime = deltaTime
        };
        var jobHandle = job.Schedule(state.points.Count, 64);
        jobHandle.Complete();
        
        this.world.RemoveComponents<IncreaseUnitsOnce>(Entity.Create<Point>(1));
        
    }

    void ISystem<State>.Update(State state, float deltaTime) { }

}
