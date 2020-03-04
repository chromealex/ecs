using ME.ECS;
using UnityEngine;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class PlayersFeature : Feature<TState, ConstructParameters<int>> {

        private System.Collections.Generic.Dictionary<int, Entity> players = new System.Collections.Generic.Dictionary<int, Entity>();
        private int activePlayer;
        
        protected override void OnConstruct(ref ConstructParameters<int> parameters) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            var unitsFeature = this.world.GetFeature<UnitsFeature>();

            this.activePlayer = parameters.p1;
            
            var mapInfo = mapFeature.mapInfo;

            { // Players

                var uiViewId = this.world.RegisterViewSource<PlayerEntity>(Resources.Load<Warcraft.Views.UI.PlayerResourcesUI>("UI/PlayerResources"));
                var uiUnitSelectedViewId = this.world.RegisterViewSource<PlayerEntity>(Resources.Load<Warcraft.Views.UI.PlayerSelectedUnitUI>("UI/PlayerUnitInfo"));

                foreach (var pd in mapInfo.playersData) {

                    var playerEntity = this.world.AddEntity(new PlayerEntity() {
                        index = pd.playerIndex,
                        goldPercent = 0.5f,
                        forestPercent = 0.5f
                    });
                    
                    this.players.Add(pd.playerIndex, playerEntity);

                    var comp = this.world.AddComponent<PlayerEntity, PlayerResourcesComponent>(playerEntity);
                    comp.resources = new ResourcesStorage() {
                        gold = 500,
                        wood = 2000,
                    };

                    if (this.activePlayer == pd.playerIndex) {

                        this.world.InstantiateView<PlayerEntity>(uiViewId, playerEntity);
                        this.world.InstantiateView<PlayerEntity>(uiUnitSelectedViewId, playerEntity);

                    }

                }

            }

            { // Spawn data
                
                foreach (var sd in mapInfo.mapGrid.spawnPoints) {

                    var pos = sd.transform.position.XY();
                    unitsFeature.SpawnUnit(this.players[sd.playerIndex], sd.unitInfo.unitTypeId, pos, new ResourcesStorage(), placeComplete: true);
                    
                }
                
            }

        }

        protected override void OnDeconstruct() {
            
        }

        public Entity GetActivePlayer() {

            return this.players[this.activePlayer];

        }

    }

}