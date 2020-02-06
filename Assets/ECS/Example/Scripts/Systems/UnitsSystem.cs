using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {

    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    
    public class UnitsSystem : ISystem<State> {

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

        public IWorld<State> world { get; set; }

        void ISystemBase.OnConstruct() { }
        void ISystemBase.OnDeconstruct() { }

        void ISystem<State>.AdvanceTick(State state, float deltaTime) {

            /*var job = new TestJob() {
                deltaTime = deltaTime
            };

            var jobHandle = job.Schedule(state.units.Count, 1000);
            jobHandle.Complete();*/

            for (int index = state.units.Count - 1; index >= 0; --index) {
                
                var data = state.units[index];
                this.world.RunComponents(ref data, deltaTime, index);
                Point toData;
                if (Worlds<State>.currentWorld.GetEntityData(data.pointTo.id, out toData) == true) {

                    if ((toData.position - data.position).sqrMagnitude <= 0.01f) {

                        var from = data.pointFrom;
                        var to = data.pointTo;
                        data.pointTo = from;
                        data.pointFrom = to;
                        Worlds<State>.currentWorld.RemoveComponents<UnitFollowFromTo>(data.entity);
                        var comp = this.world.AddComponent<Unit, UnitFollowFromTo>(data.entity);
                        comp.@from = data.pointFrom;
                        comp.to = data.pointTo;

                        --data.lifes;

                    }

                }

                state.units[index] = data;
                this.world.UpdateEntityCache(data);
                
            }

            for (int i = state.units.Count - 1; i >= 0; --i) {

                if (state.units[i].lifes <= 0) {

                    this.world.RemoveEntity<Unit>(state.units[i].entity);

                }

            }

        }

        void ISystem<State>.Update(State state, float deltaTime) { }

    }

}