using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Markers;
    using Warcraft.Components;
    
    public class UnitsPlacementSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {
        
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
        private Warcraft.Features.FogOfWarFeature fogOfWarFeature;

        private IFilter<Warcraft.WarcraftState, Warcraft.Entities.PlayerEntity> playersPlaceBuildingsFilter;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.placementMarkerExists = false;
            this.inputMoveExists = false;
            this.inputClickExists = false;
            this.cancelMarkerExists = false;
            
            Filter<TState, UnitEntity>.Create(ref this.ghostersFilter, "ghostersFilter").WithComponent<UnitGhosterComponent>().Push();
            Filter<Warcraft.WarcraftState, PlayerEntity>.Create(ref this.playersPlaceBuildingsFilter, "playersPlaceBuildingsFilter").WithComponent<Warcraft.Components.Player.PlayerPlaceBuildingComponent>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {
            
            if (this.playersFeature == null) this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            if (this.unitsFeature == null) this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            if (this.mapFeature == null) this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            if (this.pathfindingFeature == null) this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();
            if (this.fogOfWarFeature == null) this.fogOfWarFeature = this.world.GetFeature<Warcraft.Features.FogOfWarFeature>();

            foreach (var index in state.players) {

                ref var playerEntity = ref state.players[index];
                if (this.playersPlaceBuildingsFilter.Contains(playerEntity) == true) {
                    
                    var marker = this.world.GetComponent<PlayerEntity, Warcraft.Components.Player.PlayerPlaceBuildingComponent>(playerEntity.entity);
                    if (marker.actionInfo.IsEnabled(playerEntity) == true &&
                        this.pathfindingFeature.IsValid(marker.position, marker.size) == true &&
                        this.fogOfWarFeature.IsRevealed(playerEntity.entity, marker.position, marker.size) == true) {
   
                        UnityEngine.Debug.Log("Spawn placement accepted");
                        UnityEngine.Debug.DrawLine(marker.position, marker.position + UnityEngine.Vector3.up.XY(), UnityEngine.Color.red, 2f);
                        
                        var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                        playerResources.resources.gold -= marker.actionInfo.cost.gold;
                        playerResources.resources.wood -= marker.actionInfo.cost.wood;

                        this.unitsFeature.SpawnUnit(playerEntity.entity, marker.unitInfo.unitTypeId, marker.position, marker.actionInfo.cost);

                    }

                    this.world.RemoveComponents<PlayerEntity, Warcraft.Components.Player.PlayerPlaceBuildingComponent>(playerEntity.entity);

                }

            }
            
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
                
                var currentGhoster = this.unitsFeature.SpawnUnit(activePlayer, marker.unitInfo.unitTypeId, UnityEngine.Vector3.zero, new ResourcesStorage(), ghost: true);
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

                                unit.position = this.mapFeature.GetWorldBuildingPosition(marker.worldPosition, unit.size);
                                var ghosterComponent = this.world.GetComponent<UnitEntity, UnitGhosterComponent>(unit.entity);
                                ghosterComponent.isValid = ghosterComponent.actionInfo.IsEnabled(playerEntity) &&
                                                           this.pathfindingFeature.IsValid(unit.position, unit.size) &&
                                                           this.fogOfWarFeature.IsRevealed(playerOwnerComponent.player, unit.position, unit.size);

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

                                unit.position = this.mapFeature.GetWorldBuildingPosition(marker.worldPosition, unit.size);
                                
                                if (this.world.GetEntityData(activePlayer, out Warcraft.Entities.PlayerEntity playerEntity) == true) {

                                    var unitInfo = this.world.GetComponent<UnitEntity, UnitInfoComponent>(unit.entity).unitInfo;

                                    var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerPlaceBuildingComponent>(activePlayer);
                                    comp.player = activePlayer;
                                    comp.position = unit.position;
                                    comp.size = unit.size;
                                    comp.actionInfo = ghosterComponent.actionInfo;
                                    comp.unitInfo = unitInfo;
                                    
                                    this.world.RemoveEntity<UnitEntity>(unit.entity);
                                    
                                }

                            }
                            
                        }

                    }

                }
                
            }

        }

        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {
            
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