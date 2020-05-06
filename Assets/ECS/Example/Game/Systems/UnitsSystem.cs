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

                    if (Worlds<State>.currentWorld.GetEntityData(entityData.entity.GetData<UnitFollowFromTo>().to, out Point toData) == true) {

                        return ((toData.position - entityData.position).sqrMagnitude <= 0.01f);

                    }

                }

                return false;

            }

        }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() { }

        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;

        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, Unit>.Create("Filter-UnitsSystemFilter").WithStructComponent<UnitGravity>().WithStructComponent<UnitFollowFromTo>().Custom<CustomFilter>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var data = entity.GetData<UnitFollowFromTo>();
            var from = data.from;
            var to = data.to;
            data.from = to;
            data.to = from;
            entity.SetData(data);

            ref var unit = ref this.world.GetEntityDataRef<Unit>(entity);
            --unit.lifes;

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