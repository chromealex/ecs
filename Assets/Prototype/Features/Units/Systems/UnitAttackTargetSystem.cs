using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitAttackTargetSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitAttackTargetSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<IsUnit>()
                                          .WithStructComponent<Squad>()
                                          .WithStructComponent<AttackTarget>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var target = entity.GetData<AttackTarget>().entity;
            var attackRange = entity.GetData<AttackRange>().value;
            var dist = (target.GetPosition() - entity.GetPosition()).sqrMagnitude;
            if (dist <= attackRange * attackRange) {

                entity.RemoveData<MoveToTarget>();
                if (entity.HasData<AttackAction>() == true) {

                    entity.SetData(new Speed() { value = 0f });
                    entity.RemoveData<IsAttack>();
                    
                    var attackAction = entity.GetData<AttackAction>();
                    attackAction.timer -= deltaTime;
                    if (attackAction.timer <= 0f) {
                        
                        // Send hit
                        entity.SetData(new IsAttack());

                        var bullet = this.world.AddEntity(new Unit());
                        bullet.SetData(new InitializeBullet() {
                            owner = entity.GetData<Owner>().value,
                            target = target,
                            data = entity.GetData<UnitData>().data.bulletData,
                            position = entity.GetPosition() + entity.GetRotation() * entity.GetData<FirePoint>().position
                        });
                        
                        entity.RemoveData<AttackAction>();
                        entity.RemoveData<AttackTarget>();

                    } else {
                    
                        entity.SetData(attackAction);

                    }
                    
                } else {

                    entity.SetData(new AttackAction() { timer = entity.GetData<AttackSpeed>().value });

                }

            } else {

                entity.RemoveData<AttackAction>();

            }

        }

    }
    
}