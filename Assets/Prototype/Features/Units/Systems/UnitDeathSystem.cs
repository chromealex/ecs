using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitDeathSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitDeathSystem")
                                          .WithStructComponent<IsActive>()
                                          .WithStructComponent<Health>()
                                          .WithoutStructComponent<IsDead>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var health = entity.GetData<Health>().value;
            if (health <= 0f) {

                if (entity.HasData<Squad>() == true) {

                    var squad = entity.GetData<Squad>();
                    var squadChilds = squad.entity.GetData<SquadChilds>();
                    for (int i = 0; i < squadChilds.childs.Length; ++i) {

                        if (squadChilds.childs[i] == entity) {
                            
                            squadChilds.childs[i] = Entity.Empty;
                            //UnityEngine.Debug.LogWarning("Set death in squad: " + entity);
                            break;

                        }

                    }

                    var k = 0;
                    for (int i = 0; i < squadChilds.childs.Length; ++i) {

                        if (squadChilds.childs[i] != Entity.Empty) {

                            //UnityEngine.Debug.LogWarning("Child[" + i + "]: " + squadChilds.childs[i]);
                            squadChilds.childs[i].SetData(new Squad() {
                                entity = squad.entity,
                                index = k++
                            });

                        }

                    }
                    squad.entity.SetData(squadChilds);

                    if (k == 0) {
                        
                        // Remove squad
                        this.world.RemoveEntity<TEntity>(squad.entity);

                    }

                }
                
                entity.RemoveData<IsActive>();
                entity.SetData(new IsDead());

            }

        }

    }
    
}