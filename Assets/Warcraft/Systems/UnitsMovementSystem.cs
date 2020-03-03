using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    using Warcraft.Markers;
    
    public class UnitsMovementSystem : ISystem<TState> {

        private const float REACH_DESTINATION_DISTANCE = 0.01f;
        
        private InputRightClick moveUnitsMarker;
        private bool moveUnitsMarkerExists;
        
        private IFilter<Warcraft.WarcraftState, UnitEntity> unitsSelectedFilter;
        private IFilter<Warcraft.WarcraftState, UnitEntity> unitsHasTargetFilter;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.moveUnitsMarkerExists = false;
            
            Filter<Warcraft.WarcraftState, UnitEntity>.Create(ref this.unitsSelectedFilter, "unitsSelectedFilter").WithComponent<UnitSelectedComponent>().WithoutComponent<UnitPeasantComponent>().Push();
            Filter<Warcraft.WarcraftState, UnitEntity>.Create(ref this.unitsHasTargetFilter, "unitsHasTargetFilter").WithComponent<CharacterManualTarget>().WithoutComponent<UnitPeasantComponent>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            var playerFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            var pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];
                if (this.unitsHasTargetFilter.Contains(unit) == true) {
                    
                    var target = this.world.GetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                    var unitSpeed = this.world.GetComponent<UnitEntity, UnitSpeedComponent>(unit.entity);
                    unit.position = pathfindingFeature.MoveTowards(unit.entity, unit.position, ref target.target, unitSpeed.speed * deltaTime);
                    
                    if ((unit.position - target.target).sqrMagnitude <= UnitsMovementSystem.REACH_DESTINATION_DISTANCE * UnitsMovementSystem.REACH_DESTINATION_DISTANCE) {

                        this.world.RemoveComponents<Warcraft.Components.CharacterStates.CharacterMoveState>(unit.entity);
                        this.world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(unit.entity);

                    }

                }

            }

            if (this.moveUnitsMarkerExists == true) {

                this.moveUnitsMarkerExists = false;
                var marker = this.moveUnitsMarker;

                var firstPos = UnityEngine.Vector2.zero;
                var firstGot = false;
                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.unitsSelectedFilter.Contains(unit) == true) {

                        var ownerComp = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        if (ownerComp.player == playerFeature.GetActivePlayer()) {

                            if (firstGot == false) {

                                firstPos = unit.position;
                                firstGot = true;

                            }

                            var deltaPos = unit.position - firstPos;
                            var toPos = marker.worldPosition + deltaPos;
                            var target = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                            target.target = toPos;
                            this.world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(unit.entity);
                            pathfindingFeature.StopMovement(unit.entity);
                            
                        }

                    }

                }

            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {

            if (this.world.GetMarker(out InputRightClick moveUnitsMarker) == true) {

                this.moveUnitsMarker = moveUnitsMarker;
                this.moveUnitsMarkerExists = true;

            }

        }
        
    }
    
}