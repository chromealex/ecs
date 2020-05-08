using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class SquadSearchAttackTargetSystem : ISystemFilter<TState> {

        private IFilter<TState, TEntity> allSquads;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            Filter<TState, TEntity>.Create(ref this.allSquads, "Filter-SquadSearchAttackTargetSystem-All")
                                   .WithStructComponent<IsActive>()
                                   .WithStructComponent<IsSquad>()
                                   .Push();
            
        }
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-SquadSearchAttackTargetSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<IsSquad>()
                                          .WithoutStructComponent<IsPlayerCommand>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            if (entity.HasData<AttackTarget>() == true) {

                entity.SetData(new IsPathComplete()); // just trigger swap system
                entity.RemoveData<AttackTarget>();

            }

            var attackRangeInNodes = (int)entity.GetData<AttackRange>().value;
            var nodeSize = (AstarPath.active.graphs[0] as Pathfinding.GridGraph).nodeSize;
            foreach (var squad in this.allSquads) {

                var owner = squad.GetData<Owner>().value;
                if (owner == entity.GetData<Owner>().value) continue;
                
                var attackRange = nodeSize * attackRangeInNodes + 0.45f * nodeSize;
                var dist = (squad.GetPosition() - entity.GetPosition()).sqrMagnitude;
                //UnityEngine.Debug.DrawLine(entity.GetPosition(), entity.GetPosition() + (squad.GetPosition() - entity.GetPosition()).normalized * attackRange, UnityEngine.Color.red);
                if (dist <= attackRange * attackRange) {

                    //UnityEngine.Debug.Log("Squad set target: " + entity + " >> " + squad + " :: " + entity.GetData<Owner>().value + " :: " + owner);
                    entity.RemoveData<PathTraverse>();
                    entity.SetData(new MoveToTarget() { value = entity.GetData<NearestNode>().position });
                    entity.SetData(new AttackTarget() { entity = squad });
                    break;

                }

            }

        }

    }
    
}