using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

public class PointsSystem : ISystem<State> {

    //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
    private struct TestJob : IJobParallelFor {

        public float deltaTime;
        
        void IJobParallelFor.Execute(int index) {
            
            Worlds<State>.currentState.points[index] = Worlds<State>.currentWorld.RunComponents(Worlds<State>.currentState.points[index], this.deltaTime, index);
            
        }

    }

    public IWorld<State> world { get; set; }

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
        
    }

}
