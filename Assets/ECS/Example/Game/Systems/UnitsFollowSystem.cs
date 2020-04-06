using ME.ECS;

namespace ME.Example.Game.Systems {

    using Entities;
    using Components;
    
    using TState = State;
    using TEntity = Entities.Unit;
    
    public class UnitsFollowSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitsFollowSystem").WithComponent<ME.Example.Game.Components.UnitFollowFromTo>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            ref var data = ref this.world.GetEntityDataRef<Unit>(entity);

            { // Follow
                    
                var follow = this.world.GetComponent<Unit, UnitFollowFromTo>(entity);
                Point toData;
                if (this.world.GetEntityData(follow.to, out toData) == true) {

                    var toPos = toData.position;
                    data.position = UnityEngine.Vector3.MoveTowards(data.position, toPos, data.speed * deltaTime);
                    data.position.y = toPos.y;

                }

            }

        }

    }
    
}