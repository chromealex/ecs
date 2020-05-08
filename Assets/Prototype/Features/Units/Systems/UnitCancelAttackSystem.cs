using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitCancelAttackSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitCancelAttackSystem")
                                          .WithStructComponent<IsSquad>()
                                          .WithStructComponent<IsPlayerCommand>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {
            
            var childs = entity.GetData<SquadChilds>().childs;
            for (int i = 0; i < childs.Length; ++i) {

                var child = childs[i];
                if (child == Entity.Empty) continue;

                child.RemoveData<AttackAction>();
                child.RemoveData<IsAttack>();
                child.RemoveData<AttackTarget>();
                
            }
            
            entity.RemoveData<AttackTarget>();
                
        }

    }
    
}