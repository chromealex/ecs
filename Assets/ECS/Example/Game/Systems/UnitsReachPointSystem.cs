using ME.ECS;
using Unity.Jobs;

namespace ME.Example.Game.Systems {

    using TState = State;
    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    
    public class UnitsReachPointSystem : ISystemFilter<State> {

        public IWorld<State> world { get; set; }

        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() { }

        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 8;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<State> ISystemFilter<State>.CreateFilter() {
            
            return Filter<State, Unit>.Create("Filter-UnitsReachPointSystem").WithStructComponent<UnitFollowFromTo>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);

            var follow = this.world.GetData<UnitFollowFromTo>(entity);
            if (this.world.GetEntityData(follow.to, out Point toData) == true) {

                if ((toData.position - data.position).sqrMagnitude > 0.01f) {

                    return;

                }

                this.world.RemoveData<UnitFollowFromTo>(data.entity);
                this.world.SetData(data.entity, new UnitFollowFromTo() { from = follow.to, to = follow.from });
                
                --data.lifes;

            }

        }

    }

}