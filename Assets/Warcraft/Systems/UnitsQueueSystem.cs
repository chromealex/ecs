using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Markers;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsQueueSystem : ISystem<TState> {
        
        private InputUnitQueue placeInQueueMarker;
        private bool placeInQueueMarkerExists;

        private Warcraft.Features.PlayersFeature playersFeature;
        private Warcraft.Features.UnitsFeature unitsFeature;
        private Warcraft.Features.PathfindingFeature pathfindingFeature;
        private Warcraft.Features.MapFeature mapFeature;
        
        private IFilter<Warcraft.WarcraftState, Warcraft.Entities.UnitEntity> unitsFilter;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            Filter<Warcraft.WarcraftState, UnitEntity>.Create(ref this.unitsFilter, "unitsFilter").WithComponent<UnitBuildingQueueComponent>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            if (this.playersFeature == null) this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            if (this.unitsFeature == null) this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            if (this.pathfindingFeature == null) this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();
            if (this.mapFeature == null) this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];
                if (this.unitsFilter.Contains(unit) == true) {

                    var queue = this.world.GetComponent<UnitEntity, UnitBuildingQueueComponent>(unit.entity);
                    if (queue.units != null && queue.units.Count > 0) {

                        var unitItem = queue.units[0];
                        var progress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(unitItem);
                        progress.progress += deltaTime;
                        if (progress.progress >= progress.time) {
                            
                            this.world.RemoveComponents<UnitBuildingProgress>(unitItem);
                            this.world.RemoveComponents<UnitHiddenView>(unitItem);
                            queue.units.RemoveAt(0);
                            
                            var target = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unitItem);
                            var entranceWorld = this.mapFeature.GetWorldEntrancePosition(unit.position, unit.size, unit.entrance);
                            target.target = entranceWorld + this.pathfindingFeature.GetWalkableNodeByDirection(entranceWorld, this.world.GetRandomRange(0, 8));

                        }
                        
                    }

                }

            }

            if (this.placeInQueueMarkerExists == true) {

                this.placeInQueueMarkerExists = false;
                var marker = this.placeInQueueMarker;
                
                var activePlayer = this.playersFeature.GetActivePlayer();
                if (this.world.GetEntityData(activePlayer, out PlayerEntity playerEntity) == true) {

                    if (marker.actionInfo.IsEnabled(playerEntity) == true) {

                        var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                        playerResources.resources.gold -= marker.actionInfo.cost.gold;
                        playerResources.resources.wood -= marker.actionInfo.cost.wood;

                        if (this.world.GetEntityData(marker.selectedUnit, out UnitEntity unitEntity) == true) {

                            var newEntity = this.unitsFeature.SpawnUnit(activePlayer, marker.unitInfo.unitTypeId, unitEntity.position);
                            this.world.AddComponent<UnitEntity, UnitHiddenView>(newEntity);

                            var queueComponent = this.world.AddOrGetComponent<UnitEntity, UnitBuildingQueueComponent>(marker.selectedUnit);
                            if (queueComponent.units == null) queueComponent.units = PoolList<Entity>.Spawn(3);
                            queueComponent.units.Add(newEntity);
                            
                        }
                        
                    }

                }
                
            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {
            
            if (this.world.GetMarker(out InputUnitQueue marker) == true) {

                this.placeInQueueMarker = marker;
                this.placeInQueueMarkerExists = true;

            }

        }
        
    }
    
}