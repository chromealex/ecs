using ME.ECS;
using Unity.Jobs;

namespace ME.GameExample.Game.Systems {
    
    public class PointsSystem : ISystem<GameState> {

        //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
        private struct TestJob : IJobParallelFor {

            public float deltaTime;

            void IJobParallelFor.Execute(int index) {

                var data = Worlds<GameState>.currentWorld.RunComponents(Worlds<GameState>.currentState.points[index], this.deltaTime, index);
                Worlds<GameState>.currentState.points[index] = data;
                Worlds<GameState>.currentWorld.UpdateEntityCache(data);

            }

        }

        IWorld<GameState> ISystem<GameState>.world { get; set; }

        void ISystemBase.OnConstruct() { }
        void ISystemBase.OnDeconstruct() { }

        void ISystem<GameState>.AdvanceTick(GameState state, float deltaTime) {

            var job = new TestJob() {
                deltaTime = deltaTime
            };
            var jobHandle = job.Schedule(state.points.Count, 64);
            jobHandle.Complete();

        }

        void ISystem<GameState>.Update(GameState state, float deltaTime) { }

    }

}