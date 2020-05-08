using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitSearchAttackTargetSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitSearchAttackTargetSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<IsUnit>()
                                          .WithStructComponent<Squad>()
                                          .WithoutStructComponent<AttackAction>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var squad = entity.GetData<Squad>();
            if (squad.entity.HasData<AttackTarget>() == true && squad.entity.HasData<IsPlayerCommand>() == false) {

                var targetSquad = squad.entity.GetData<AttackTarget>().entity;
                
                var target = Entity.Empty;
                var dist = float.MaxValue;
                var childs = targetSquad.GetData<SquadChilds>().childs;
                for (int i = 0; i < childs.Length; ++i) {

                    var child = childs[i];
                    if (child == Entity.Empty) continue;
                    
                    var d = (child.GetPosition() - entity.GetPosition()).sqrMagnitude;
                    if (d < dist) {

                        dist = d;
                        target = child;

                    }

                }

                if (target != Entity.Empty) {

                    //UnityEngine.Debug.Log("Set new target: " + target + ", isActive: " + target.HasData<IsActive>());
                    entity.SetData(new AttackTarget() { entity = target });
                    entity.SetData(new MoveToTarget() { value = target.GetPosition() });
                    
                }

            } else {
                
                entity.RemoveData<IsAttack>();
                entity.RemoveData<AttackAction>();
                entity.RemoveData<AttackTarget>();

            }

        }

    }
    
}