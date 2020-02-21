using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {

    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    
    public class UnitsSystem : ISystem<State> {

        public IWorld<State> world { get; set; }

        private IFilter<State, Unit> unitsFilter;
        private IFilter<State, Unit> unitsFilter2;
        private IFilter<State, Unit> unitsFilter3;

        private class CustomFilter : IFilterNode<Unit> {

            public bool Execute(Unit data) {

                Point toData;
                if (Worlds<State>.currentWorld.GetEntityData(data.pointTo, out toData) == true) {

                    return ((toData.position - data.position).sqrMagnitude <= 0.01f);

                }

                return false;

            }

        }
        
        void ISystemBase.OnConstruct() {

            Filter<State, Unit>.Create(ref this.unitsFilter, nameof(this.unitsFilter)).WithoutComponent<UnitFollowFromTo>().Push();
            Filter<State, Unit>.Create(ref this.unitsFilter2, nameof(this.unitsFilter2)).WithComponent<UnitGravity>().WithComponent<UnitFollowFromTo>().Push();
            Filter<State, Unit>.Create(ref this.unitsFilter3, nameof(this.unitsFilter3)).WithComponent<UnitGravity>().WithComponent<UnitFollowFromTo>().Custom<CustomFilter>().Push();
            
        }
        
        void ISystemBase.OnDeconstruct() { }

        void ISystem<State>.AdvanceTick(State state, float deltaTime) {

            this.world.Checkpoint("Update Units");
            foreach (var index in state.units) {
                
                ref var data = ref state.units[index];
                this.world.RunComponents(ref data, deltaTime, index);
                
                if (this.unitsFilter3.Contains(data) == true) {
                
                    var from = data.pointFrom;
                    var to = data.pointTo;
                    data.pointTo = from;
                    data.pointFrom = to;
                    this.world.RemoveComponents<UnitFollowFromTo>(data.entity);
                    var comp = this.world.AddComponent<Unit, UnitFollowFromTo>(data.entity);
                    comp.@from = data.pointFrom;
                    comp.to = data.pointTo;
                
                    --data.lifes;

                }

                this.world.UpdateFilters(data);

            }
            this.world.Checkpoint("Update Units");

            this.world.Checkpoint("Remove Units");
            foreach (var index in state.units) {

                if (state.units[index].lifes <= 0) {

                    this.world.RemoveEntity<Unit>(state.units[index].entity);

                }

            }
            this.world.Checkpoint("Remove Units");

        }

        void ISystem<State>.Update(State state, float deltaTime) { }

    }

}