using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitMoveToTargetSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-UnitMoveToTargetSystem").WithStructComponent<IsActive>().WithStructComponent<MoveToTarget>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var position = entity.GetPosition();
            var rotation = entity.GetRotation();
            var target = entity.GetData<MoveToTarget>().value;
            
            var maxSpeed = entity.GetData<MaxSpeed>().value;
            var speed = entity.GetData<Speed>().value;
            var slowdownDistance = entity.GetData<SlowdownDistance>().value;
            if ((target - position).sqrMagnitude > slowdownDistance * slowdownDistance) {
            
                speed = UnityEngine.Mathf.Lerp(speed, maxSpeed, deltaTime * entity.GetData<Acceleration>().value);

            } else {
                
                speed = UnityEngine.Mathf.Lerp(speed, maxSpeed * 0.25f, deltaTime * entity.GetData<SlowdownSpeed>().value);

            }

            entity.SetData(new Speed() { value = speed });

            UnityEngine.Quaternion targetRotation;
            if (target == position) {

                targetRotation = rotation;

            } else {

                targetRotation = UnityEngine.Quaternion.LookRotation((target - position).normalized, UnityEngine.Vector3.up);

            }
            
            position = UnityEngine.Vector3.MoveTowards(position, target, deltaTime * speed);
            rotation = UnityEngine.Quaternion.Slerp(rotation, targetRotation, deltaTime * entity.GetData<RotationSpeed>().value);
            entity.SetPosition(position);
            entity.SetRotation(rotation);

            if ((target - position).sqrMagnitude <= 0.01f) {

                entity.SetData(new IsMoveToTargetComplete());

            }

        }

    }
    
}