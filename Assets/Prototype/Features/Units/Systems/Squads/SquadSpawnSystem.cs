using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class SquadSpawnSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-SquadSpawnSystem").WithStructComponent<IsSquad>().WithStructComponent<InitializeSquad>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var squadData = entity.GetData<InitializeSquad>();
            
            entity.SetPosition(squadData.position);
            entity.SetData(new SquadSize() { value = squadData.data.squadCount });
            entity.SetData(new SquadChilds() { childs = new ME.ECS.Collections.StackArray10<Entity>(10) });

            for (int k = 0; k < squadData.data.squadCount; ++k) {
                                
                var unit = this.world.AddEntity(new Unit());
                unit.SetData(new Prototype.Features.Units.Components.InitializeUnit() {
                    owner = squadData.owner,
                    squad = entity,
                    indexInSquad = k,
                    data = squadData.data.unitData
                });

            }
            
            entity.SetData(new Speed() { value = 0f });
            entity.SetData(new MaxSpeed() { value = squadData.data.movementSpeed });
            entity.SetData(new RotationSpeed() { value = squadData.data.rotationSpeed });
            entity.SetData(new PickNextPointDistance() { value = squadData.data.pickNextPointDistance });
            entity.SetData(new Acceleration() { value = squadData.data.accelerationSpeed });
            entity.SetData(new SlowdownSpeed() { value = squadData.data.slowdownSpeed });
            entity.SetData(new AttackRange() { value = squadData.data.attackRangeInNodes });
            entity.SetData(new Owner() { value = squadData.owner });
            entity.SetData(new IsActive());
            entity.SetData(new IsPathComplete()); // just trigger Swap system

            entity.RemoveData<InitializeSquad>();

        }

    }
    
}