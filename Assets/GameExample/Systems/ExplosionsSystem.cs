using ME.ECS;
using Unity.Jobs;

namespace ME.GameExample.Game.Systems {

    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    
    public class ExplosionsSystem : ISystem<GameState> {

        //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
        /*private struct TestJob : IJobParallelFor {

            public float deltaTime;

            void IJobParallelFor.Execute(int index) {

                var data = Worlds<State>.currentState.units[index];
                data = Worlds<State>.currentWorld.RunComponents(data, this.deltaTime, index);
                Point toData;
                if (Worlds<State>.currentWorld.GetEntityData(data.pointTo.id, out toData) == true) {

                    if ((toData.position - data.position).sqrMagnitude <= 0.01f) {

                        var from = data.pointFrom;
                        var to = data.pointTo;
                        data.pointTo = from;
                        data.pointFrom = to;
                        Worlds<State>.currentWorld.RemoveComponents<UnitFollowFromTo>(data.entity);
                        var comp = Worlds<State>.currentWorld.AddComponent<Unit, UnitFollowFromTo>(data.entity);
                        comp.@from = data.pointFrom;
                        comp.to = data.pointTo;

                        --data.lifes;

                    }

                }

                Worlds<State>.currentState.units[index] = data;
                Worlds<State>.currentWorld.UpdateEntityCache(data);

            }

        }*/

        public IWorld<GameState> world { get; set; }

        void ISystemBase.OnConstruct() { }
        void ISystemBase.OnDeconstruct() { }

        void ISystem<GameState>.AdvanceTick(GameState state, float deltaTime) {

            /*var job = new TestJob() {
                deltaTime = deltaTime
            };

            var jobHandle = job.Schedule(state.units.Count, 1000);
            jobHandle.Complete();*/

            for (int index = state.explosions.Count - 1; index >= 0; --index) {
                
                var data = Worlds<GameState>.currentState.explosions[index];
                data = Worlds<GameState>.currentWorld.RunComponents(data, deltaTime, index);
                Worlds<GameState>.currentState.explosions[index] = data;
                Worlds<GameState>.currentWorld.UpdateEntityCache(data);

                if (data.lifetime <= 0f) {
                
                    Worlds<GameState>.currentWorld.RemoveEntity<ME.GameExample.Game.Entities.Explosion>(data.entity);

                }

            }

        }

        void ISystem<GameState>.Update(GameState state, float deltaTime) { }

    }

}