using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Markers;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsUpgradesSystem : ISystem<TState> {
        
        private InputUnitUpgrade upgradeMarker;
        private bool upgradeMarkerExists;

        private Warcraft.Features.PlayersFeature playersFeature;
        private Warcraft.Features.UnitsFeature unitsFeature;
        
        public IWorld<TState> world { get; set; }
        
        void ISystemBase.OnConstruct() {}
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            if (this.playersFeature == null) this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            if (this.unitsFeature == null) this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            
            if (this.upgradeMarkerExists == true) {

                this.upgradeMarkerExists = false;
                var marker = this.upgradeMarker;
                
                var activePlayer = this.playersFeature.GetActivePlayer();
                if (this.world.GetEntityData(activePlayer, out Warcraft.Entities.PlayerEntity playerEntity) == true) {

                    if (marker.actionInfo.IsEnabled(playerEntity) == true) {

                        var inProgress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(marker.selectedUnit);
                        if (inProgress == null) {

                            var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                            playerResources.resources.gold -= marker.actionInfo.cost.gold;
                            playerResources.resources.wood -= marker.actionInfo.cost.wood;

                            this.unitsFeature.UpgradeUnit(marker.selectedUnit, marker.unitInfo.unitTypeId, marker.actionInfo.cost);

                        }

                    }

                }
                
            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {
            
            if (this.world.GetMarker(out InputUnitUpgrade marker) == true) {

                this.upgradeMarker = marker;
                this.upgradeMarkerExists = true;

            }

        }
        
    }
    
}