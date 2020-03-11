using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class AISystem : ISystem<TState> {

        private Warcraft.Features.MapFeature mapFeature;
        private Warcraft.Features.PathfindingFeature pathfindingFeature;
        private Warcraft.Features.FogOfWarFeature fogOfWarFeature;
        
        private IFilter<TState, PlayerEntity> aiPlayers;
        private IFilter<TState, UnitEntity> aiPeasants;
        private IFilter<TState, UnitEntity> aiCastles;
        private IFilter<TState, UnitEntity> aiBuildings;
        private IFilter<TState, UnitEntity> aiGoldMines;
        private IFilter<TState, UnitEntity> aiCharacters;

        private UnitInfo noUnitSelected;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            Filter<TState, PlayerEntity>.Create(ref this.aiPlayers, "aiPlayers").WithComponent<AIComponent>().Push();
            Filter<TState, UnitEntity>.Create(ref this.aiPeasants, "aiPeasants").WithComponent<UnitPeasantComponent>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, UnitEntity>.Create(ref this.aiCastles, "aiCastles").WithComponent<UnitPlayerOwnerComponent>().WithComponent<CastleComponent>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, UnitEntity>.Create(ref this.aiBuildings, "aiBuildings").WithComponent<UnitPlayerOwnerComponent>().WithoutComponent<CharacterComponent>().WithoutComponent<CastleComponent>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, UnitEntity>.Create(ref this.aiGoldMines, "aiGoldMines").WithComponent<GoldMineComponent>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, UnitEntity>.Create(ref this.aiCharacters, "aiCharacters").WithComponent<UnitPlayerOwnerComponent>().WithComponent<CharacterComponent>().WithoutComponent<UnitPeasantComponent>().WithoutComponent<CharacterManualTarget>().WithComponent<Warcraft.Components.CharacterStates.CharacterIdleState>().WithoutComponent<UnitDeathState>().Push();

            this.noUnitSelected = UnityEngine.Resources.Load<UnitInfo>("Units/_NoUnit");

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {

            if (this.mapFeature == null) this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            if (this.pathfindingFeature == null) this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();
            if (this.fogOfWarFeature == null) this.fogOfWarFeature = this.world.GetFeature<Warcraft.Features.FogOfWarFeature>();
            
            foreach (var index in state.units) {

                ref var unit = ref state.units[index];
                if (this.aiCharacters.Contains(unit) == true) {

                    var playerComp = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                    if (this.aiPlayers.Contains(playerComp.player) == false) continue;
                    
                    var toPos = this.world.GetRandomInSphere(unit.position, 2f);
                    
                    this.world.RemoveComponents<UnitEntity, CharacterAutoTarget>(unit.entity);
                    var target = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                    target.target = toPos;
                    this.pathfindingFeature.StopMovement(unit.entity, repath: true);
                    
                }

                if (this.aiBuildings.Contains(unit) == true) {
                    
                    var playerComp = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                    if (this.aiPlayers.Contains(playerComp.player) == false) continue;
                    this.world.GetEntityData(playerComp.player, out PlayerEntity playerData);
                    var unitInfo = this.world.GetComponent<UnitEntity, UnitInfoComponent>(unit.entity);

                    if (unitInfo.unitInfo.actions == null) continue;

                    var actionsGraph = unitInfo.unitInfo.actions.roots;
                    foreach (var action in actionsGraph) {

                        if (action is ActionBuildingNode actionBuildingInfo) {

                            if (actionBuildingInfo.isUpgrade == true) {
                                
                                // Upgrade building
                                if (actionBuildingInfo.building != null && this.world.GetRandomRange(0, 50) == 5) {

                                    if (action.IsEnabled(playerData) == false) continue;
                                
                                    var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerUpgradeBuildingComponent>(playerComp.player);
                                    comp.player = playerComp.player;
                                    comp.actionInfo = actionBuildingInfo;
                                    comp.selectedUnit = unit.entity;
                                    comp.unitInfo = actionBuildingInfo.building;
                                    //found = true;
                                    break;

                                }

                            } else {

                                // Build chars
                                if (actionBuildingInfo.building != null && actionBuildingInfo.building is CharacterUnitInfo && this.world.GetRandomRange(0, 10) == 5) {

                                    if (action.IsEnabled(playerData) == false) continue;

                                    var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerBuildingQueueComponent>(playerComp.player);
                                    comp.player = playerComp.player;
                                    comp.actionInfo = actionBuildingInfo;
                                    comp.selectedUnit = unit.entity;
                                    comp.unitInfo = actionBuildingInfo.building;
                                    //found = true;
                                    break;

                                }

                            }

                        }

                    }
                    
                } else if (this.aiCastles.Contains(unit) == true) {
                    
                    var playerComp = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                    if (this.aiPlayers.Contains(playerComp.player) == false) continue;
                    this.world.GetEntityData(playerComp.player, out PlayerEntity playerData);
                    var unitInfo = this.world.GetComponent<UnitEntity, UnitInfoComponent>(unit.entity);
                    
                    var peasantsCount = 0;
                    foreach (var idx in state.units) {

                        ref var peasant = ref state.units[idx];
                        if (this.aiGoldMines.Contains(peasant) == true) {
                    
                            this.TryToPlaceBuildings(unit, playerData, playerComp, null);

                        }

                        if (this.aiPeasants.Contains(peasant) == true) {
                            
                            var playerCompPeasant = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                            if (this.aiPlayers.Contains(playerCompPeasant.player) == false) continue;

                            ++peasantsCount;

                        }

                    }

                    //var found = false;
                    var actionsGraph = unitInfo.unitInfo.actions.roots;
                    foreach (var action in actionsGraph) {

                        if (action is ActionBuildingNode actionBuildingInfo) {

                            if (actionBuildingInfo.isUpgrade == true) {
                                
                                // Upgrade castle
                                if (actionBuildingInfo.building != null && this.world.GetRandomRange(0, 50) == 5) {

                                    if (action.IsEnabled(playerData) == false) continue;
                                
                                    var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerUpgradeBuildingComponent>(playerComp.player);
                                    comp.player = playerComp.player;
                                    comp.actionInfo = actionBuildingInfo;
                                    comp.selectedUnit = unit.entity;
                                    comp.unitInfo = actionBuildingInfo.building;
                                    //found = true;
                                    break;

                                }

                            } else {

                                // Build chars
                                if (actionBuildingInfo.building != null && actionBuildingInfo.building is CharacterUnitInfo && this.world.GetRandomRange(0, 10) == 5) {

                                    if (action.IsEnabled(playerData) == false) continue;

                                    if (peasantsCount <= 15) {

                                        var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerBuildingQueueComponent>(playerComp.player);
                                        comp.player = playerComp.player;
                                        comp.actionInfo = actionBuildingInfo;
                                        comp.selectedUnit = unit.entity;
                                        comp.unitInfo = actionBuildingInfo.building;
                                        //found = true;
                                        break;

                                    }

                                }

                            }

                        }

                    }

                    this.TryToPlaceBuildings(unit, playerData, playerComp, unitInfo);
                    
                }

            }

        }

        private void TryToPlaceBuildings(UnitEntity unit, PlayerEntity playerData, UnitPlayerOwnerComponent playerComp, UnitInfoComponent unitInfo) {
            
            // No castle actions - move into buildings
            var defaultActionsGraph = this.noUnitSelected.actions;
            foreach (var node in defaultActionsGraph.roots) {

                if (node is ActionGroupNode groupNode) {

                    var next = groupNode.GetNext();
                    foreach (var building in next) {

                        if (building is ActionBuildingNode buildingNode) {

                            if (buildingNode.building != null) {
                                
                                // Place building
                                if (this.world.GetRandomRange(0, 50) == 5) {

                                    if (buildingNode.IsEnabled(playerData) == false) continue;
                                    
                                    //UnityEngine.Debug.Log("Try to place building: " + buildingNode.building);

                                    var buildingSize = buildingNode.building.viewSource.size;
                                    var point = this.mapFeature.GetWorldBuildingPosition(this.world.GetRandomInSphere(unit.position.XY(), unitInfo != null ? unitInfo.unitInfo.sightRange : 5f).XY(), buildingSize);
                                    
                                    if (this.pathfindingFeature.IsValid(point, buildingSize + UnityEngine.Vector2Int.one * 2) == false) continue;
                                    if (this.fogOfWarFeature.IsRevealed(playerComp.player, point, buildingSize) == false) continue;

                                    var comp = this.world.AddComponent<PlayerEntity, Warcraft.Components.Player.PlayerPlaceBuildingComponent>(playerComp.player);
                                    comp.player = playerComp.player;
                                    comp.position = point;
                                    comp.size = buildingSize;
                                    comp.actionInfo = buildingNode;
                                    comp.unitInfo = buildingNode.building;
                                    
                                    //found = true;
                                    break;

                                }
                                
                            }

                        }
                        
                    }

                }

            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {}
        
    }
    
}