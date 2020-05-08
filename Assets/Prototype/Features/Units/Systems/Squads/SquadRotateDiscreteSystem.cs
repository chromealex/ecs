using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class SquadRotateDiscreteSystem : ISystemFilter<TState> {
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-SquadRotateDiscreteSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().WithStructComponent<IsPathComplete>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            var rotation = entity.GetRotation();
            var angles = rotation.eulerAngles;
            angles.y = (UnityEngine.Mathf.RoundToInt(angles.y / 90f) * 90f);
            rotation.eulerAngles = angles;
            entity.SetRotation(rotation);


        }

    }
    
}