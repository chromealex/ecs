using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {
    
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

            var job = new TestJob() {
                deltaTime = deltaTime
            };
            var jobHandle = job.Schedule(state.points.Count, 64);
            jobHandle.Complete();

        }

        void ISystem<State>.Update(State state, float deltaTime) { }

    }

}