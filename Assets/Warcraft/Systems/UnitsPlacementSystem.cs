using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Markers;
    using Warcraft.Components;
    
    public class UnitsPlacementSystem : ISystem<TState> {
        
        private IFilter<TState, UnitEntity> ghostersFilter;
        
        private InputUnitPlacement placementMarker;
        private bool placementMarkerExists;

        private InputMove inputMove;
        private bool inputMoveExists;

        private InputLeftClick inputLeftClick;
        private bool inputClickExists;

        private CancelMarker cancelMarker;
        private bool cancelMarkerExists;

        private Warcraft.Features.PlayersFeature playersFeature;
        private Warcraft.Features.UnitsFeature unitsFeature;
        private Warcraft.Features.MapFeature mapFeature;
        private Warcraft.Features.PathfindingFeature pathfindingFeature;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.placementMarkerExists = false;
            this.inputMoveExists = false;
            this.inputClickExists = false;
            this.cancelMarkerExists = false;
            
            Filter<TState, UnitEntity>.Create(ref this.ghostersFilter, "ghostersFilter").WithComponent<UnitGhosterComponent>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            if (this.playersFeature == null) this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            if (this.unitsFeature == null) this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            if (this.mapFeature == null) this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            if (this.pathfindingFeature == null) this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();

            if (this.cancelMarkerExists == true) {

                this.cancelMarkerExists = false;
                
                var activePlayer = this.playersFeature.GetActivePlayer();

                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.ghostersFilter.Contains(unit) == true) {
                        
                        var playerOwnerComponent = this.world.GetComponent<Warcraft.Entities.UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        if (playerOwnerComponent.player == activePlayer) {

                            this.world.RemoveEntity<Warcraft.Entities.UnitEntity>(unit.entity);
                            
                        }
                        
                    }

                }

            }

            if (this.placementMarkerExists == true) {

                this.placementMarkerExists = false;
                var marker = this.placementMarker;

                var activePlayer = this.playersFeature.GetActivePlayer();
                
                var currentGhoster = this.unitsFeature.SpawnUnit(activePlayer, marker.unitInfo.unitTypeId, UnityEngine.Vector3.zero, ghost: true);
                var ghoster = this.world.GetComponent<UnitEntity, UnitGhosterComponent>(currentGhoster);
                ghoster.actionInfo = marker.actionInfo;

            }

            if (this.inputMoveExists == true) {

                this.inputMoveExists = false;
                var marker = this.inputMove;
                
                var activePlayer = this.playersFeature.GetActivePlayer();

                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.ghostersFilter.Contains(unit) == true) {

                        var playerOwnerComponent = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        if (playerOwnerComponent.player == activePlayer) {

                            if (this.world.GetEntityData(activePlayer, out PlayerEntity playerEntity) == true) {

                                unit.position = this.mapFeature.GetWorldCellPosition(marker.worldPosition, unit.size);
                                var ghosterComponent = this.world.GetComponent<UnitEntity, UnitGhosterComponent>(unit.entity);
                                ghosterComponent.isValid = ghosterComponent.actionInfo.IsEnabled(playerEntity) && this.pathfindingFeature.IsValid(unit.position, unit.size);

                            }

                        }

                    }

                }

            }

            if (this.inputClickExists == true) {

                this.inputClickExists = false;
                var marker = this.inputLeftClick;

                var activePlayer = this.playersFeature.GetActivePlayer();

                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.ghostersFilter.Contains(unit) == true) {

                        var playerOwnerComponent = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        if (playerOwnerComponent.player == activePlayer) {

                            var ghosterComponent = this.world.GetComponent<UnitEntity, UnitGhosterComponent>(unit.entity);
                            if (ghosterComponent.isValid == true) {

                                if (this.world.GetEntityData(activePlayer, out Warcraft.Entities.PlayerEntity playerEntity) == true) {

                                    if (ghosterComponent.actionInfo.IsEnabled(playerEntity) == false) continue;
                                
                                }

                                var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                                playerResources.resources.gold -= ghosterComponent.actionInfo.cost.gold;
                                playerResources.resources.wood -= ghosterComponent.actionInfo.cost.wood;

                                var unitInfoComponent = this.world.GetComponent<UnitEntity, UnitInfoComponent>(unit.entity);
                                
                                unit.position = this.mapFeature.GetWorldCellPosition(marker.worldPosition, unit.size);
                                this.unitsFeature.SpawnUnit(activePlayer, unitInfoComponent.unitInfo.unitTypeId, unit.position);
                                this.world.RemoveEntity<UnitEntity>(unit.entity);

                            }
                            
                        }

                    }

                }
                
            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {
            
            if (this.world.GetMarker(out InputUnitPlacement marker) == true) {

                this.placementMarker = marker;
                this.placementMarkerExists = true;

            }

            if (this.world.GetMarker(out InputMove inputMove) == true) {
                
                this.inputMove = inputMove;
                this.inputMoveExists = true;

            }

            if (this.world.GetMarker(out InputLeftClick inputClick) == true) {
                
                this.inputLeftClick = inputClick;
                this.inputClickExists = true;

            }

            if (this.world.GetMarker(out CancelMarker cancelMarker) == true) {
                
                this.cancelMarker = cancelMarker;
                this.cancelMarkerExists = true;

            }

        }
        
    }
    
}