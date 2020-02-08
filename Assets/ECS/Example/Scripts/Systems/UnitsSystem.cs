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

        private class CustomFilter : INode<Unit> {

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

            for (int index = state.units.ToIndex - 1; index >= state.units.FromIndex; --index) {
                
                if (state.units.IsFree(index) == true) continue;
                
                ref var data = ref state.units[index];
                this.world.RunComponents(ref data, deltaTime, index);
                
                if (this.unitsFilter3.Contains(data) == true) {

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

                //state.units[index] = data;
                this.world.UpdateEntityCache(data);

            }

            for (int index = state.units.ToIndex - 1; index >= state.units.FromIndex; --index) {

                if (state.units.IsFree(index) == true) continue;

                if (state.units[index].lifes <= 0) {

                    this.world.RemoveEntity<Unit>(state.units[index].entity);

                }

            }

        }

        void ISystem<State>.Update(State state, float deltaTime) { }

    }

}