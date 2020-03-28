using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {

    using TState = State;
    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    
    public class UnitsSystem : ISystemFilter<State>, ISystemAdvanceTick<State> {

        public IWorld<State> world { get; set; }

        private IFilter<State, Unit> unitsFilter;

        private class CustomFilter : IFilterNode {

            public bool Execute(Entity data) {

                if (Worlds<State>.currentWorld.GetEntityData(data, out Unit entityData) == true) {

                    if (Worlds<State>.currentWorld.GetEntityData(entityData.pointTo, out Point toData) == true) {

                        return ((toData.position - entityData.position).sqrMagnitude <= 0.01f);

                    }

                }

                return false;

            }

        }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() { }

        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<State> ISystemFilter<State>.CreateFilter() {
            
            return Filter<State, Unit>.Create("Filter-UnitsSystemFilter").WithComponent<UnitGravity>().WithComponent<UnitFollowFromTo>().Custom<CustomFilter>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);
            var from = data.pointFrom;
            var to = data.pointTo;
            data.pointTo = from;
            data.pointFrom = to;
            this.world.RemoveComponents<Unit, UnitFollowFromTo>(data.entity);
            var comp = this.world.AddComponent<Unit, UnitFollowFromTo>(data.entity);
            comp.@from = data.pointFrom;
            comp.to = data.pointTo;
                
            --data.lifes;

        }

        void ISystemAdvanceTick<State>.AdvanceTick(State state, float deltaTime) {

            this.world.Checkpoint("Update Units");
            foreach (var index in state.units) {

                ref var data = ref state.units[index];
                this.world.RunComponents(ref data, deltaTime, index);

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

    }

}