using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    using Warcraft.Components.PeasantStates;
    
    public class PeasantsSystem : ISystem<TState> {

        private const float REACH_DESTINATION_DISTANCE = 0.001f;
        
        private IFilter<TState, Warcraft.Entities.ForestEntity> forestEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> castlesEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> goldMineEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> buildingsInProgressEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> peasantIdleEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> peasantGoToWorkEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> peasantWorkingEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> peasantGoToCastleEntities;
        private IFilter<TState, Warcraft.Entities.UnitEntity> peasantWorkingInCastleEntities;
        
        private Warcraft.Features.MapFeature mapFeature;
        private Warcraft.Features.PathfindingFeature pathfindingFeature;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();
            
            Filter<TState, Warcraft.Entities.ForestEntity>.Create(ref this.forestEntities, "forestEntities").Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.castlesEntities, "castlesEntities").WithComponent<CastleComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.goldMineEntities, "goldMineEntities").WithComponent<GoldMineComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.buildingsInProgressEntities, "buildingsInProgressEntities").WithComponent<UnitBuildingProgress>().WithoutComponent<CharacterComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantIdleEntities, "peasantIdleEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantIdleState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantGoToWorkEntities, "peasantGoToWorkEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantGoToWorkState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantWorkingEntities, "peasantWorkingEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantWorkingState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantGoToCastleEntities, "peasantGoToCastleEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantGoToCastleState>().WithoutComponent<PeasantWorkingState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantWorkingInCastleEntities, "peasantWorkingInCastleEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantGoToCastleState>().WithComponent<PeasantWorkingState>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];

                if (this.peasantWorkingInCastleEntities.Contains(unit) == true) {
                    
                    unit.workingTimer += deltaTime;
                    if (unit.workingTimer >= 1f) {

                        unit.workingTimer = 0f;
                        
                        this.world.RemoveComponents<PeasantGoToCastleState>(unit.entity);

                        var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        PlayerEntity playerData;
                        this.world.GetEntityData(playerOwner.player, out playerData);

                        var resValueWood = this.world.GetComponent<UnitEntity, PeasantCarryWood>(unit.entity);
                        var resValueGold = this.world.GetComponent<UnitEntity, PeasantCarryGold>(unit.entity);

                        var comp = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerOwner.player);
                        if (resValueWood != null) comp.resources.wood += resValueWood.value;
                        if (resValueGold != null) comp.resources.gold += resValueGold.value;

                        this.world.RemoveComponents<PeasantCarryGold>(unit.entity);
                        this.world.RemoveComponents<PeasantCarryWood>(unit.entity);

                        this.world.RemoveComponents<UnitHiddenView>(unit.entity);
                        this.world.RemoveComponents<PeasantWorkingState>(unit.entity);

                        this.world.AddComponent<UnitEntity, PeasantIdleState>(unit.entity);

                    }

                } else if (this.peasantGoToCastleEntities.Contains(unit) == true) {
                    
                    // Put resource to player
                    var unitSpeed = this.world.GetComponent<UnitEntity, UnitSpeedComponent>(unit.entity);
                    unit.position = this.pathfindingFeature.MoveTowards(unit.entity, unit.position, ref unit.jobTargetPos, unitSpeed.speed * deltaTime);
                    if ((unit.position - unit.jobTargetPos).sqrMagnitude <= PeasantsSystem.REACH_DESTINATION_DISTANCE * PeasantsSystem.REACH_DESTINATION_DISTANCE) {

                        this.world.AddComponent<UnitEntity, UnitHiddenView>(unit.entity);
                        this.world.AddComponent<UnitEntity, PeasantWorkingState>(unit.entity);

                    }

                } else if (this.peasantWorkingEntities.Contains(unit) == true) {

                    var timer = 3f;
                    if (unit.jobTargetType == 3) {
                    
                        var progress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(unit.jobTarget);
                        progress.progress += deltaTime;
                        timer = progress.time;
                        if (progress.progress >= progress.time) {
                            
                            this.world.RemoveComponents<UnitBuildingProgress>(unit.jobTarget);
                            
                        }

                    }

                    unit.workingTimer += deltaTime;
                    if (unit.workingTimer >= timer) {

                        unit.workingTimer = 0f;
                        var carryToCastle = false;

                        if (unit.jobTargetType == 3) {
                            
                            var workPlace = this.world.GetComponent<UnitEntity, BuildingInProgressCountAtWorkPlace>(unit.jobTarget);
                            --workPlace.count;

                            var compMax = this.world.GetComponent<UnitEntity, UnitsMaxPerBuildingInProgress>(unit.jobTarget);
                            if (compMax != null) {
                                
                                --compMax.current;
                                
                            }

                        } else if (unit.jobTargetType == 1) {

                            var workPlace = this.world.GetComponent<ForestEntity, ForestCountAtWorkPlace>(unit.jobTarget);
                            --workPlace.count;

                            var comp = this.world.GetComponent<ForestEntity, ForestLifesComponent>(unit.jobTarget);
                            if (comp != null) {

                                --comp.lifes;
                                if (comp.lifes <= 0) {

                                    this.world.RemoveEntity<ForestEntity>(unit.jobTarget);
                                    
                                }

                            }
                            
                            var compMax = this.world.GetComponent<ForestEntity, ForestUnitsMaxPerTree>(unit.jobTarget);
                            if (compMax != null) {
                                
                                --compMax.current;
                                if (comp.lifes < compMax.max) compMax.max = comp.lifes;

                            }
                            
                            var resValue = this.world.AddComponent<UnitEntity, PeasantCarryWood>(unit.entity);
                            resValue.value = 1;

                            carryToCastle = true;

                        } else if (unit.jobTargetType == 2) {

                            var workPlace = this.world.GetComponent<UnitEntity, UnitCountAtWorkPlace>(unit.jobTarget);
                            --workPlace.count;

                            var comp = this.world.GetComponent<UnitEntity, GoldMineComponent>(unit.jobTarget);
                            if (comp != null) {

                                --comp.capacity;

                            }
                            
                            var compMax = this.world.GetComponent<UnitEntity, GoldMineUnitsMaxPerMine>(unit.jobTarget);
                            if (compMax != null) {
                                
                                --compMax.current;
                                if (comp.capacity < compMax.max) compMax.max = comp.capacity;

                            }

                            var resValue = this.world.AddComponent<UnitEntity, PeasantCarryGold>(unit.entity);
                            resValue.value = 1;

                            carryToCastle = true;

                        }

                        if (carryToCastle == true) {

                            var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                            PlayerEntity playerData;
                            this.world.GetEntityData(playerOwner.player, out playerData);

                            var dist = float.MaxValue;
                            foreach (var idx in state.units) {

                                ref var unitCastle = ref state.units[idx];
                                if (this.castlesEntities.Contains(unitCastle) == true) {

                                    var castlePlayerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unitCastle.entity);
                                    PlayerEntity castlePlayerData;
                                    this.world.GetEntityData(castlePlayerOwner.player, out castlePlayerData);
                                    if (playerData.index != castlePlayerData.index) continue;

                                    ref var worldPosition = ref unitCastle.position;
                                    var d = (worldPosition - unit.position).sqrMagnitude;
                                    if (d <= dist) {

                                        unit.jobTarget = unitCastle.entity;
                                        unit.jobTargetPos = this.mapFeature.GetWorldEntrancePosition(unitCastle.position, unitCastle.size, unitCastle.entrance);
                                        dist = d;

                                    }

                                }

                            }
                            
                            this.world.AddComponent<UnitEntity, PeasantGoToCastleState>(unit.entity);

                        } else {
                            
                            this.world.AddComponent<UnitEntity, PeasantIdleState>(unit.entity);
                            
                        }

                        this.world.RemoveComponents<UnitHiddenView>(unit.entity);
                        this.world.RemoveComponents<PeasantWorkingState>(unit.entity);
                        
                    }

                } else if (this.peasantGoToWorkEntities.Contains(unit) == true) {

                    var unitSpeed = this.world.GetComponent<UnitEntity, UnitSpeedComponent>(unit.entity);
                    unit.position = this.pathfindingFeature.MoveTowards(unit.entity, unit.position, ref unit.jobTargetPos, unitSpeed.speed * deltaTime);
                    if ((unit.position - unit.jobTargetPos).sqrMagnitude <= PeasantsSystem.REACH_DESTINATION_DISTANCE * PeasantsSystem.REACH_DESTINATION_DISTANCE) {
                        
                        this.world.RemoveComponents<PeasantGoToWorkState>(unit.entity);
                        this.world.AddComponent<UnitEntity, PeasantWorkingState>(unit.entity);

                        if (unit.jobTargetType == 3) {
                            
                            var workPlace = this.world.AddOrGetComponent<UnitEntity, BuildingInProgressCountAtWorkPlace>(unit.jobTarget);
                            ++workPlace.count;

                            this.world.AddComponent<UnitEntity, UnitHiddenView>(unit.entity);

                        } else if (unit.jobTargetType == 1) {
                            
                            var workPlace = this.world.AddOrGetComponent<ForestEntity, ForestCountAtWorkPlace>(unit.jobTarget);
                            ++workPlace.count;

                        } else if (unit.jobTargetType == 2) {
                            
                            var workPlace = this.world.AddOrGetComponent<UnitEntity, UnitCountAtWorkPlace>(unit.jobTarget);
                            ++workPlace.count;

                            this.world.AddComponent<UnitEntity, UnitHiddenView>(unit.entity);

                        }

                    }

                } else if (this.peasantIdleEntities.Contains(unit) == true) {

                    var unitMapPosition = unit.position;
                    
                    // Looking for the job
                    var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                    PlayerEntity playerData;
                    this.world.GetEntityData(playerOwner.player, out playerData);

                    var found = false;
                    var dist = float.MaxValue;
                    {

                        UnitsMaxPerBuildingInProgress compMax;
                        foreach (var buildingEntity in this.buildingsInProgressEntities.GetData()) {

                            var unitOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(buildingEntity);
                            if (unitOwner.player != playerOwner.player) continue;

                            if (this.world.GetEntityData(buildingEntity, out UnitEntity buildingData) == true) {

                                ref var worldPos = ref buildingData.position;
                                var halfSize = this.mapFeature.GetWorldPositionFromMap(buildingData.size) * 0.5f;
                                for (int bx = 0; bx < buildingData.size.x; ++bx) {
                                 
                                    for (int by = 0; by < buildingData.size.y; ++by) {

                                        var buildingPoint = worldPos - halfSize + this.mapFeature.GetWorldPositionFromMap(new UnityEngine.Vector2Int(bx, by), true, true);
                                        var d = (buildingPoint - unitMapPosition).sqrMagnitude;
                                        if (d <= dist) {

                                            compMax = this.world.GetComponent<UnitEntity, UnitsMaxPerBuildingInProgress>(buildingEntity);
                                            if (compMax != null) {

                                                if (compMax.current >= compMax.max) continue;

                                            }
                                    
                                            if (this.pathfindingFeature.IsPathExists(unitMapPosition, buildingPoint) == false) continue;

                                            unit.jobTarget = buildingEntity;
                                            unit.jobTargetType = 3;
                                            unit.jobTargetPos = buildingPoint;
                                            found = true;
                                            dist = d;

                                        }

                                    }
                                    
                                }

                            }

                        }
                        
                        if (found == true) {

                            compMax = this.world.GetComponent<UnitEntity, UnitsMaxPerBuildingInProgress>(unit.jobTarget);
                            if (compMax != null) {

                                ++compMax.current;

                            }

                        }

                    }

                    if (found == false) {

                        var fullValue = playerData.forestPercent + playerData.goldPercent;
                        var value = this.world.GetRandomRange(0f, fullValue);
                        if (value < playerData.forestPercent) {

                            ForestUnitsMaxPerTree compMax;

                            // Search for nearest forest
                            dist = float.MaxValue;
                            foreach (var forestIndex in state.forest) {

                                ref var forest = ref state.forest[forestIndex];
                                if (this.forestEntities.Contains(forest) == true) {

                                    ref var mapPosition = ref forest.position;
                                    var worldPos = this.mapFeature.GetWorldPositionFromMap(mapPosition, true, true);
                                    var d = (worldPos - unitMapPosition).sqrMagnitude;
                                    if (d <= dist) {

                                        compMax = this.world.GetComponent<ForestEntity, ForestUnitsMaxPerTree>(forest.entity);
                                        if (compMax != null) {

                                            if (compMax.current >= compMax.max) continue;

                                        }
                                        
                                        if (this.HasCastleNearby(playerData.index, worldPos) == false) continue;

                                        if (this.pathfindingFeature.IsPathExists(unitMapPosition, worldPos) == false) continue;
                                        
                                        unit.jobTarget = forest.entity;
                                        unit.jobTargetType = 1;
                                        unit.jobTargetPos = worldPos;
                                        dist = d;
                                        found = true;

                                    }

                                }

                            }

                            if (found == true) {

                                compMax = this.world.GetComponent<ForestEntity, ForestUnitsMaxPerTree>(unit.jobTarget);
                                if (compMax != null) {

                                    ++compMax.current;

                                }

                            }

                        } else {

                            GoldMineUnitsMaxPerMine compMax;

                            // Search for nearest gold mine
                            dist = float.MaxValue;
                            foreach (var goldIndex in state.units) {

                                ref var goldMine = ref state.units[goldIndex];
                                if (this.goldMineEntities.Contains(goldMine) == true) {

                                    ref var mapPosition = ref goldMine.position;
                                    var d = (mapPosition - unitMapPosition).sqrMagnitude;
                                    if (d <= dist) {

                                        var comp = this.world.GetComponent<UnitEntity, GoldMineComponent>(goldMine.entity);
                                        if (comp != null) {

                                            if (comp.capacity <= 0) continue;

                                        }

                                        compMax = this.world.GetComponent<UnitEntity, GoldMineUnitsMaxPerMine>(goldMine.entity);
                                        if (compMax != null) {

                                            if (compMax.current >= compMax.max) continue;

                                        }

                                        if (this.HasCastleNearby(playerData.index, goldMine.position) == false) continue;

                                        unit.jobTarget = goldMine.entity;
                                        unit.jobTargetType = 2;
                                        unit.jobTargetPos = this.mapFeature.GetWorldEntrancePosition(goldMine.position, goldMine.size, goldMine.entrance);
                                        dist = d;
                                        found = true;

                                    }

                                }

                            }

                            if (found == true) {

                                compMax = this.world.GetComponent<UnitEntity, GoldMineUnitsMaxPerMine>(unit.jobTarget);
                                if (compMax != null) {

                                    ++compMax.current;

                                }

                            }

                        }

                    }

                    if (found == true) {

                        this.world.RemoveComponents<PeasantIdleState>(unit.entity);
                        this.world.AddComponent<UnitEntity, PeasantGoToWorkState>(unit.entity);

                    }
                    
                }
                
                this.world.UpdateFilters(unit);

            }

        }

        private bool HasCastleNearby(int playerIndex, UnityEngine.Vector2 position) {

            const float maxDistance = 5f;
            foreach (var castleEntity in this.castlesEntities.GetData()) {

                if (this.world.GetEntityData(castleEntity, out UnitEntity entData) == true) {

                    var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(entData.entity);
                    PlayerEntity playerData;
                    this.world.GetEntityData(playerOwner.player, out playerData);
                    if (playerData.index != playerIndex) continue;
                    
                    if ((entData.position - position).sqrMagnitude <= maxDistance * maxDistance) {

                        return true;

                    }

                }
                                      
            }

            return false;

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {}
        
    }
    
}