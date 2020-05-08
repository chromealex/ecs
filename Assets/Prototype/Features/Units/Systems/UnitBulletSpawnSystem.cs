using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitBulletSpawnSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitBulletSpawnSystem")
                                          .WithStructComponent<InitializeBullet>()
                                          .Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var data = entity.GetData<InitializeBullet>();
            
            entity.SetData(new IsBullet());
            entity.SetData(new IsActive());
            entity.SetData(new Owner() { value = data.owner });
            entity.SetData(new AttackTarget() {
                entity = data.target
            });
            entity.SetData(new Acceleration() { value = data.data.accelerationSpeed });
            entity.SetData(new SlowdownSpeed() { value = data.data.slowdownSpeed });
            entity.SetData(new Speed() { value = 0f });
            entity.SetData(new MaxSpeed() { value = data.data.movementSpeed });
            entity.SetData(new RotationSpeed() { value = data.data.rotationSpeed });
            entity.SetData(new Damage() { value = data.data.damage });
            entity.SetPosition(data.position);
            entity.SetData(new MoveToTarget() {
                value = data.target.GetPosition()
            });

            if (data.data.viewSource != null) {

                var viewId = this.world.RegisterViewSource<Unit>(data.data.viewSource);
                this.world.InstantiateView<Unit>(viewId, entity);

            }
            
            entity.RemoveData<InitializeBullet>();

        }

    }
    
}