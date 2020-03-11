using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Markers;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsQueueSystem : ISystem<TState> {
        
        private InputUnitQueue placeInQueueMarker;
        private bool placeInQueueMarkerExists;

        private InputUnitQueueCancel placeInQueueCancelMarker;
        private bool placeInQueueCancelMarkerExists;

        private Warcraft.Features.PlayersFeature playersFeature;
        private Warcraft.Features.UnitsFeature unitsFeature;
        private Warcraft.Features.PathfindingFeature pathfindingFeature;
        private Warcraft.Features.MapFeature mapFeature;
        
        private IFilter<Warcraft.WarcraftState, Warcraft.Entities.UnitEntity> unitsFilter;
        private IFilter<Warcraft.WarcraftState, Warcraft.Entities.PlayerEntity> playersBuildingQueueFilter;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            Filter<Warcraft.WarcraftState, UnitEntity>.Create(ref this.unitsFilter, "unitsFilter").WithComponent<UnitBuildingQueueComponent>().Push();
            Filter<Warcraft.WarcraftState, PlayerEntity>.Create(ref this.playersBuildingQueueFilter, "playersBuildingQueueFilter").WithComponent<Warcraft.Components.Player.PlayerBuildingQueueComponent>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            if (this.playersFeature == null) this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            if (this.unitsFeature == null) this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            if (this.pathfindingFeature == null) this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();
            if (this.mapFeature == null) this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();

            foreach (var index in state.players) {

                ref var playerEntity = ref state.players[index];
                if (this.playersBuildingQueueFilter.Contains(playerEntity) == true) {
                    
                    var marker = this.world.GetComponent<PlayerEntity, Warcraft.Components.Player.PlayerBuildingQueueComponent>(playerEntity.entity);
                    if (marker.actionInfo.IsEnabled(playerEntity) == true) {

                        if (this.world.GetEntityData(marker.selectedUnit, out UnitEntity unitEntity) == true) {

                            var inProgress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(unitEntity.entity);
                            if (inProgress == null) {

                                var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                                playerResources.resources.gold -= marker.actionInfo.cost.gold;
                                playerResources.resources.wood -= marker.actionInfo.cost.wood;

                                var newEntity = this.unitsFeature.SpawnUnit(playerEntity.entity, marker.unitInfo.unitTypeId, this.mapFeature.GetWorldEntrancePosition(unitEntity.position, unitEntity.size, unitEntity.entrance), marker.actionInfo.cost);
                                this.world.AddComponent<UnitEntity, UnitHiddenView>(newEntity);

                                var queueComponent = this.world.AddOrGetComponent<UnitEntity, UnitBuildingQueueComponent>(marker.selectedUnit);
                                if (queueComponent.units == null) queueComponent.units = PoolList<Entity>.Spawn(3);
                                queueComponent.units.Add(newEntity);

                            }
                        
                        }
                    
                    }
                    this.world.RemoveComponents<PlayerEntity, Warcraft.Components.Player.PlayerBuildingQueueComponent>(playerEntity.entity);

                }

            }

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];
                if (this.unitsFilter.Contains(unit) == true) {

                    var queue = this.world.GetComponent<UnitEntity, UnitBuildingQueueComponent>(unit.entity);
                    if (queue.units != null && queue.units.Count > 0) {

                        var unitItem = queue.units[0];
                        var progress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(unitItem);
                        progress.progress += deltaTime;
                        if (progress.progress >= progress.time) {
                            
                            queue.units.RemoveAt(0);
                            
                            this.world.RemoveComponents<UnitEntity, UnitBuildingProgress>(unitItem);
                            this.world.RemoveComponents<UnitEntity, UnitHiddenView>(unitItem);
                            this.world.AddOrGetComponent<UnitEntity, UnitCompleteComponent>(unitItem);

                            var target = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unitItem);
                            var entranceWorld = this.mapFeature.GetWorldEntrancePosition(unit.position, unit.size, unit.entrance);
                            target.target = this.pathfindingFeature.GetWalkableNodeByDirection(entranceWorld, this.world.GetRandomRange(0, 8));

                        }
                        
                    }

                }

            }

            if (this.placeInQueueCancelMarkerExists == true) {

                this.placeInQueueCancelMarkerExists = false;
                var marker = this.placeInQueueCancelMarker;
                
                var activePlayer = this.playersFeature.GetActivePlayer();
                if (this.world.GetEntityData(activePlayer, out PlayerEntity playerEntity) == true) {

                    if (this.world.GetEntityData(marker.selectedUnit, out UnitEntity buildingEntity) == true) {

                        var queue = this.world.GetComponent<UnitEntity, UnitBuildingQueueComponent>(buildingEntity.entity);
                        if (queue != null) {

                            foreach (var unit in queue.units) {

                                if (unit == marker.unitInQueue) {

                                    var unitCost = this.world.GetComponent<UnitEntity, UnitCostComponent>(unit);
                                    
                                    var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                                    playerResources.resources.gold += unitCost.cost.gold;
                                    playerResources.resources.wood += unitCost.cost.wood;

                                    queue.units.Remove(unit);
                                    this.world.RemoveEntity<UnitEntity>(unit);
                                    break;

                                }

                            }

                        }
                        
                    }
                    
                }
                
            }

            if (this.placeInQueueMarkerExists == true) {

                this.placeInQueueMarkerExists = false;
                var marker = this.placeInQueueMarker;
                
                var activePlayer = this.playersFeature.GetActivePlayer();
                var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerBuildingQueueComponent>(activePlayer);
                comp.player = activePlayer;
                comp.actionInfo = marker.actionInfo;
                comp.selectedUnit = marker.selectedUnit;
                comp.unitInfo = marker.unitInfo;
                
            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {
            
            if (this.world.GetMarker(out InputUnitQueue marker) == true) {

                this.placeInQueueMarker = marker;
                this.placeInQueueMarkerExists = true;

            }

            if (this.world.GetMarker(out InputUnitQueueCancel markerCancel) == true) {

                this.placeInQueueCancelMarker = markerCancel;
                this.placeInQueueCancelMarkerExists = true;

            }

        }
        
    }
    
}