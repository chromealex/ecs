using ME.ECS;
using Unity.Jobs;

namespace ME.GameExample.Game.Systems {

    using ME.GameExample.Game.Components;
    using ME.GameExample.Game.Entities;
    
    public class UnitsSystem : ISystem<GameState> {

        //[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Deterministic, FloatPrecision = FloatPrecision.Standard)]
        private struct TestJob : IJobParallelFor {

            public float deltaTime;

            void IJobParallelFor.Execute(int index) {

                var data = Worlds<GameState>.currentState.units[index];
                data = Worlds<GameState>.currentWorld.RunComponents(data, deltaTime, index);
                Point toData;
                if (Worlds<GameState>.currentWorld.GetEntityData(data.pointTo.id, out toData) == true) {

                    if ((toData.position - data.position).sqrMagnitude <= 0.1f) {

                        var from = data.pointFrom;
                        var to = data.pointTo;
                        data.pointTo = from;
                        data.pointFrom = to;
                        Worlds<GameState>.currentWorld.RemoveComponents<UnitFollowFromTo>(data.entity);
                        var comp = Worlds<GameState>.currentWorld.AddComponent<Unit, UnitFollowFromTo>(data.entity);
                        comp.@from = data.pointFrom;
                        comp.to = data.pointTo;

                        --data.lifes;

                    }

                }

                Worlds<GameState>.currentState.units[index] = data;
                Worlds<GameState>.currentWorld.UpdateEntityCache(data);

            }

        }

        public IWorld<GameState> world { get; set; }

        void ISystemBase.OnConstruct() { }
        void ISystemBase.OnDeconstruct() { }

        void ISystem<GameState>.AdvanceTick(GameState state, float deltaTime) {

            var job = new TestJob() {
                deltaTime = deltaTime
            };

            var jobHandle = job.Schedule(state.units.Count, 1000);
            jobHandle.Complete();

            /*
            for (int index = state.units.Count - 1; index >= 0; --index) {
                
                var data = Worlds<GameState>.currentState.units[index];
                data = Worlds<GameState>.currentWorld.RunComponents(data, deltaTime, index);
                Point toData;
                if (Worlds<GameState>.currentWorld.GetEntityData(data.pointTo.id, out toData) == true) {

                    if ((toData.position - data.position).sqrMagnitude <= 0.1f) {

                        var from = data.pointFrom;
                        var to = data.pointTo;
                        data.pointTo = from;
                        data.pointFrom = to;
                        Worlds<GameState>.currentWorld.RemoveComponents<UnitFollowFromTo>(data.entity);
                        var comp = Worlds<GameState>.currentWorld.AddComponent<Unit, UnitFollowFromTo>(data.entity);
                        comp.@from = data.pointFrom;
                        comp.to = data.pointTo;

                        --data.lifes;

                    }

                }

                Worlds<GameState>.currentState.units[index] = data;
                Worlds<GameState>.currentWorld.UpdateEntityCache(data);
                
            }*/

            for (int i = state.units.Count - 1; i >= 0; --i) {

                if (state.units[i].lifes <= 0) {

                    this.world.RemoveEntity<Unit>(state.units[i].entity);

                }

            }

        }

        void ISystem<GameState>.Update(GameState state, float deltaTime) { }

    }

}