using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {
    
    public class PointsSystem : ISystem<State>, ISystemAdvanceTick<State>, ISystemUpdate<State> {

        //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
        /*private struct TestJob : IJobParallelFor {

            public float deltaTime;

            void IJobParallelFor.Execute(int index) {

                var data = Worlds<State>.currentState.points[index];
                Worlds<State>.currentWorld.RunComponents(ref data, this.deltaTime, index);
                Worlds<State>.currentState.points[index] = data;
                Worlds<State>.currentWorld.UpdateEntityCache(data);

            }

        }*/

        public IWorld<State> world { get; set; }

        void ISystemBase.OnConstruct() { }
        void ISystemBase.OnDeconstruct() { }

        void ISystemAdvanceTick<State>.AdvanceTick(State state, float deltaTime) {

            /*var job = new TestJob() {
                deltaTime = deltaTime
            };
            this.world.Checkpoint("Update Points");
            var jobHandle = job.Schedule(state.points.Count, 64);
            jobHandle.Complete();
            this.world.Checkpoint("Update Points");*/

            foreach (var index in state.points) {
                
                ref var data = ref state.points[index];
                this.world.RunComponents(ref data, deltaTime, index);
                
            }

        }

        void ISystemUpdate<State>.Update(State state, float deltaTime) { }

    }

}