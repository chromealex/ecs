using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitSpawnSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitSpawnSystem").WithStructComponent<InitializeUnit>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var initializationData = entity.GetData<InitializeUnit>();
            var data = initializationData.data;
            {

                var viewId = this.world.RegisterViewSource<Unit>(data.viewSource);
                this.world.InstantiateView<Unit>(viewId, entity);
                
                entity.SetData(new Squad() { entity = initializationData.squad, index = initializationData.indexInSquad });
                entity.SetData(new AttackRange() { value = data.attackRange });
                entity.SetData(new AttackSpeed() { value = data.attackSpeed });
                entity.SetData(new Acceleration() { value = data.accelerationSpeed });
                entity.SetData(new SlowdownSpeed() { value = data.slowdownSpeed });
                entity.SetData(new Speed() { value = 0f });
                entity.SetData(new MaxSpeed() { value = data.movementSpeed });
                entity.SetData(new RotationSpeed() { value = data.rotationSpeed });
                entity.SetData(new Health() { value = data.health });
                entity.SetData(new MaxHealth() { value = data.health });
                entity.SetData(new Owner() { value = initializationData.owner });
                entity.SetData(new UnitData() {
                    data = initializationData.data
                });
                entity.SetData(new FirePoint() {
                    position = data.viewSource.bulletPoint.position
                });
                entity.SetData(new IsActive());
                entity.SetData(new IsUnit());

                var squad = initializationData.squad;
                var squadChilds = squad.GetData<SquadChilds>();
                for (int i = 0; i < squadChilds.childs.Length; ++i) {

                    if (squadChilds.childs[i] == Entity.Empty) {

                        squadChilds.childs[i] = entity;
                        break;

                    }

                }
                squad.SetData(squadChilds);

                //entity.SetParent(initializationData.squad);
                entity.SetPosition(initializationData.squad.GetPosition());

            }
            entity.RemoveData<InitializeUnit>();

        }

    }
    
}