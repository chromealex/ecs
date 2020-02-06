using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {
    
    public class PointsSystem : ISystem<State> {

        //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
        private struct TestJob : IJobParallelFor {

            public float deltaTime;

            void IJobParallelFor.Execute(int index) {

                var data = Worlds<State>.currentState.points[index];
                Worlds<State>.currentWorld.RunComponents(ref data, this.deltaTime, index);
                Worlds<State>.currentState.points[index] = data;
                Worlds<State>.currentWorld.UpdateEntityCache(data);

            }

        }

        IWorld<State> ISystem<State>.world { get; set; }

        void ISystemBase.OnConstruct() { }
        void ISystemBase.OnDeconstruct() { }

        void ISystem<State>.AdvanceTick(State state, float deltaTime) {

            var job = new TestJob() {
                deltaTime = deltaTime
            };
            var jobHandle = job.Schedule(state.points.Count, 64);
            jobHandle.Complete();

        }

        void ISystem<State>.Update(State state, float deltaTime) { }

    }

}