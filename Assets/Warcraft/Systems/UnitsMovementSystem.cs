using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    using Warcraft.Markers;
    
    public class UnitsMovementSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {

        private const float REACH_DESTINATION_DISTANCE = 0.01f;
        
        private InputRightClick moveUnitsMarker;
        private bool moveUnitsMarkerExists;

        private Warcraft.Features.MapFeature mapFeature;
        
        private IFilter<Warcraft.WarcraftState, UnitEntity> unitsSelectedFilter;
        private IFilter<Warcraft.WarcraftState, UnitEntity> unitsHasTargetFilter;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.moveUnitsMarkerExists = false;

            this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            
            Filter<Warcraft.WarcraftState, UnitEntity>.Create(ref this.unitsSelectedFilter, "unitsSelectedFilter").WithComponent<UnitSelectedComponent>().WithoutComponent<UnitPeasantComponent>().WithComponent<CharacterComponent>().WithoutComponent<UnitDeathState>().Push();
            Filter<Warcraft.WarcraftState, UnitEntity>.Create(ref this.unitsHasTargetFilter, "unitsHasTargetFilter").WithComponent<CharacterManualTarget>().WithComponent<UnitSpeedComponent>().WithoutComponent<UnitPeasantComponent>().WithoutComponent<UnitDeathState>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {
            
            var playerFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            var pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();

            foreach (var unitEntity in this.unitsHasTargetFilter) {

                ref var unit = ref this.world.GetEntityDataRef<UnitEntity>(unitEntity);
                var target = this.world.GetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                var unitSpeed = this.world.GetComponent<UnitEntity, UnitSpeedComponent>(unit.entity);
                if (pathfindingFeature.MoveTowards(unit.entity, ref unit.position, ref target.target, unitSpeed.speed * deltaTime, deltaTime, out var failed, checkLastPoint: true) == true) {
                
                    this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterManualTarget>(unit.entity);
                    this.world.AddOrGetComponent<UnitEntity, CharacterAutoTarget>(unit.entity);
                    pathfindingFeature.StopMovement(unit.entity, repath: true);

                } else if (failed == true) {
                        
                    this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterManualTarget>(unit.entity);
                    this.world.AddOrGetComponent<UnitEntity, CharacterAutoTarget>(unit.entity);
                    pathfindingFeature.StopMovement(unit.entity, repath: true);

                }

            }

            if (this.moveUnitsMarkerExists == true) {

                this.moveUnitsMarkerExists = false;
                var marker = this.moveUnitsMarker;

                var pointsList = PoolList<UnityEngine.Vector3>.Spawn(10);

                var toPos = marker.worldPosition;
                var center = this.mapFeature.GetMapPositionFromWorld(toPos);
                var idx = 0;

                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.unitsSelectedFilter.Contains(unit) == true) {

                        var ownerComp = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        if (ownerComp.player == playerFeature.GetActivePlayer()) {

                            var failedTests = 0;
                            while (failedTests < 50) {

                                var spiralPoint = MathUtils.GetSpiralPointByIndex(center, idx);
                                ++idx;
                                var spiralPointWorld = this.mapFeature.GetWorldPositionFromMap(spiralPoint);
                                if (pathfindingFeature.IsPathExists(unit.position, spiralPointWorld) == false) {

                                    ++failedTests;

                                } else {

                                    toPos = spiralPointWorld;
                                    break;

                                }

                            }

                            this.world.RemoveComponents<UnitEntity, CharacterAutoTarget>(unit.entity);
                            var target = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                            target.target = toPos;
                            pathfindingFeature.StopMovement(unit.entity, repath: true);
                            
                        }

                    }

                }
                PoolList<UnityEngine.Vector3>.Recycle(ref pointsList);

            }

        }

        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {

            if (this.world.GetMarker(out InputRightClick moveUnitsMarker) == true) {

                this.moveUnitsMarker = moveUnitsMarker;
                this.moveUnitsMarkerExists = true;

            }

        }
        
    }
    
}