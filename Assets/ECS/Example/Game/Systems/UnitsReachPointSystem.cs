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

        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<State> ISystemFilter<State>.CreateFilter() {
            
            return Filter<State, Unit>.Create("Filter-UnitsReachPointSystem").WithComponent<UnitFollowFromTo>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);

            var follow = this.world.GetComponent<Unit, UnitFollowFromTo>(entity);
            if (Worlds<State>.currentWorld.GetEntityData(follow.to, out Point toData) == true) {

                if ((toData.position - data.position).sqrMagnitude > 0.01f) {

                    return;

                }

                var from = follow.from;
                var to = follow.to;
                this.world.RemoveComponents<Unit, UnitFollowFromTo>(data.entity);
                var comp = this.world.AddComponent<Unit, UnitFollowFromTo>(data.entity);
                comp.@from = to;
                comp.to = from;
            
                --data.lifes;

            }

        }

    }

}