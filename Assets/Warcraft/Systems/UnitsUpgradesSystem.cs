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
        
        private IFilter<Warcraft.WarcraftState, Warcraft.Entities.PlayerEntity> playersUpgradingQueueFilter;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            Filter<Warcraft.WarcraftState, PlayerEntity>.Create(ref this.playersUpgradingQueueFilter, "playersUpgradingQueueFilter").WithComponent<Warcraft.Components.Player.PlayerUpgradeBuildingComponent>().Push();
            
        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            if (this.playersFeature == null) this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            if (this.unitsFeature == null) this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            
            foreach (var index in state.players) {

                ref var playerEntity = ref state.players[index];
                if (this.playersUpgradingQueueFilter.Contains(playerEntity) == true) {
                    
                    var marker = this.world.GetComponent<PlayerEntity, Warcraft.Components.Player.PlayerUpgradeBuildingComponent>(playerEntity.entity);
                    if (marker.actionInfo.IsEnabled(playerEntity) == true) {

                        var inProgress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(marker.selectedUnit);
                        if (inProgress == null) {

                            var playerResources = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity.entity);
                            playerResources.resources.gold -= marker.actionInfo.cost.gold;
                            playerResources.resources.wood -= marker.actionInfo.cost.wood;

                            this.unitsFeature.UpgradeUnit(marker.selectedUnit, marker.unitInfo.unitTypeId, marker.actionInfo.cost);

                        }

                    }
                    this.world.RemoveComponents<PlayerEntity, Warcraft.Components.Player.PlayerUpgradeBuildingComponent>(playerEntity.entity);

                }

            }
            
            if (this.upgradeMarkerExists == true) {

                this.upgradeMarkerExists = false;
                var marker = this.upgradeMarker;
                
                var activePlayer = this.playersFeature.GetActivePlayer();
                var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerUpgradeBuildingComponent>(activePlayer);
                comp.player = activePlayer;
                comp.actionInfo = marker.actionInfo;
                comp.selectedUnit = marker.selectedUnit;
                comp.unitInfo = marker.unitInfo;

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