using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    using Warcraft.Components.PeasantStates;
    
    public class PeasantsSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {

        private const float REACH_DESTINATION_DISTANCE = 0.001f;
        
        private IFilter<TState, Warcraft.Entities.UnitEntity> forestEntities;
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
        private Warcraft.Features.FogOfWarFeature fowFeature;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();
            this.fowFeature = this.world.GetFeature<Warcraft.Features.FogOfWarFeature>();
            
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.forestEntities, "forestEntities").WithComponent<ForestComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.castlesEntities, "castlesEntities").WithComponent<CastleComponent>().WithComponent<UnitCompleteComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.goldMineEntities, "goldMineEntities").WithComponent<GoldMineComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.buildingsInProgressEntities, "buildingsInProgressEntities").WithComponent<UnitBuildingProgress>().WithoutComponent<CharacterComponent>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantIdleEntities, "peasantIdleEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantIdleState>().WithoutComponent<UnitBuildingProgress>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantGoToWorkEntities, "peasantGoToWorkEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantGoToWorkState>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantWorkingEntities, "peasantWorkingEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantWorkingState>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantGoToCastleEntities, "peasantGoToCastleEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantGoToCastleState>().WithoutComponent<PeasantWorkingState>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.peasantWorkingInCastleEntities, "peasantWorkingInCastleEntities").WithComponent<UnitPeasantComponent>().WithComponent<PeasantGoToCastleState>().WithComponent<PeasantWorkingState>().WithoutComponent<UnitDeathState>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {

            state.peasantsMindTimer += deltaTime;

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];

                if (this.peasantWorkingInCastleEntities.Contains(unit) == true) {
                    
                    unit.workingTimer += deltaTime;
                    if (unit.workingTimer >= 1f) {

                        unit.workingTimer = 0f;
                        
                        this.world.RemoveComponents<UnitEntity, PeasantGoToCastleState>(unit.entity);

                        var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                        PlayerEntity playerData;
                        this.world.GetEntityData(playerOwner.player, out playerData);

                        var resValueWood = this.world.GetComponent<UnitEntity, PeasantCarryWood>(unit.entity);
                        var resValueGold = this.world.GetComponent<UnitEntity, PeasantCarryGold>(unit.entity);

                        var comp = this.world.GetComponent<PlayerEntity, PlayerResourcesComponent>(playerOwner.player);
                        if (resValueWood != null) comp.resources.wood += resValueWood.value;
                        if (resValueGold != null) comp.resources.gold += resValueGold.value;

                        this.world.RemoveComponents<UnitEntity, PeasantCarryGold>(unit.entity);
                        this.world.RemoveComponents<UnitEntity, PeasantCarryWood>(unit.entity);

                        this.world.RemoveComponents<UnitEntity, UnitHiddenView>(unit.entity);
                        this.world.RemoveComponents<UnitEntity, PeasantWorkingState>(unit.entity);

                        this.world.AddComponent<UnitEntity, PeasantIdleState>(unit.entity);

                    }

                } else if (this.peasantGoToCastleEntities.Contains(unit) == true) {
                    
                    // Put resource to player
                    var unitSpeed = this.world.GetComponent<UnitEntity, UnitSpeedComponent>(unit.entity);
                    if (this.pathfindingFeature.MoveTowards(unit.entity, ref unit.position, ref unit.jobTargetPos, unitSpeed.speed * deltaTime, deltaTime, addLastNode: true) == true) {

                        this.world.AddComponent<UnitEntity, UnitHiddenView>(unit.entity);
                        this.world.AddComponent<UnitEntity, PeasantWorkingState>(unit.entity);

                    }

                } else if (this.peasantWorkingEntities.Contains(unit) == true) {

                    var timer = 3f;
                    if (unit.jobTargetType == 3) {
                    
                        var progress = this.world.GetComponent<UnitEntity, UnitBuildingProgress>(unit.jobTarget);
                        progress.progress += deltaTime;
                        timer = progress.time;

                    }

                    unit.workingTimer += deltaTime;
                    if (unit.workingTimer >= timer) {

                        unit.workingTimer = 0f;
                        var carryToCastle = false;

                        if (unit.jobTargetType == 3) {
                            
                            this.world.RemoveComponents<UnitEntity, UnitBuildingProgress>(unit.jobTarget);

                            var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.jobTarget);
                            var unitInfo = this.world.GetComponent<UnitEntity, UnitInfoComponent>(unit.jobTarget);
                            var units = this.world.AddOrGetComponent<PlayerEntity, PlayerUnitsComponent>(playerOwner.player);
                            units.AddUnit(unitInfo.unitInfo.unitTypeId);

                            this.world.AddOrGetComponent<UnitEntity, UnitCompleteComponent>(unit.jobTarget);
                            
                            var workPlace = this.world.GetComponent<UnitEntity, BuildingInProgressCountAtWorkPlace>(unit.jobTarget);
                            --workPlace.count;

                            var compMax = this.world.GetComponent<UnitEntity, UnitMaxPerBuildingInProgress>(unit.jobTarget);
                            if (compMax != null) {
                                
                                --compMax.current;
                                
                            }

                        } else if (unit.jobTargetType == 1) {

                            var workPlace = this.world.GetComponent<UnitEntity, UnitCountAtWorkPlace>(unit.jobTarget);
                            --workPlace.count;

                            var comp = this.world.GetComponent<UnitEntity, UnitLifesComponent>(unit.jobTarget);
                            if (comp != null) {

                                --comp.lifes;
                                if (comp.lifes <= 0) {

                                    this.world.GetEntityData(unit.jobTarget, out UnitEntity treeEntityData);
                                    this.pathfindingFeature.SetWalkability(this.mapFeature.GetMapPositionFromWorld(treeEntityData.position), true, Entity.Empty);
                                    this.world.RemoveEntity<UnitEntity>(unit.jobTarget);
                                    
                                }

                            }
                            
                            var compMax = this.world.GetComponent<UnitEntity, UnitMaxPerWorkingPlace>(unit.jobTarget);
                            if (compMax != null) {
                                
                                --compMax.current;
                                if (comp.lifes < compMax.max) compMax.max = comp.lifes;

                            }
                            
                            var resValue = this.world.AddComponent<UnitEntity, PeasantCarryWood>(unit.entity);
                            resValue.value = 5;

                            carryToCastle = true;

                        } else if (unit.jobTargetType == 2) {

                            var workPlace = this.world.GetComponent<UnitEntity, UnitCountAtWorkPlace>(unit.jobTarget);
                            --workPlace.count;

                            var comp = this.world.GetComponent<UnitEntity, GoldMineComponent>(unit.jobTarget);
                            if (comp != null) {

                                --comp.capacity;

                            }
                            
                            var compMax = this.world.GetComponent<UnitEntity, UnitMaxPerWorkingPlace>(unit.jobTarget);
                            if (compMax != null) {
                                
                                --compMax.current;
                                if (comp.capacity < compMax.max) compMax.max = comp.capacity;

                            }

                            var resValue = this.world.AddComponent<UnitEntity, PeasantCarryGold>(unit.entity);
                            resValue.value = 10;

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

                        this.world.RemoveComponents<UnitEntity, UnitHiddenView>(unit.entity);
                        this.world.RemoveComponents<UnitEntity, PeasantWorkingState>(unit.entity);
                        
                    }

                } else if (this.peasantGoToWorkEntities.Contains(unit) == true) {

                    var unitSpeed = this.world.GetComponent<UnitEntity, UnitSpeedComponent>(unit.entity);
                    if (this.pathfindingFeature.MoveTowards(unit.entity, ref unit.position, ref unit.jobTargetPos, unitSpeed.speed * deltaTime, deltaTime, addLastNode: (unit.jobTargetType != 1)) == true) {
                        
                        this.world.RemoveComponents<UnitEntity, PeasantGoToWorkState>(unit.entity);
                        this.world.AddComponent<UnitEntity, PeasantWorkingState>(unit.entity);

                        if (unit.jobTargetType == 3) {
                            
                            var workPlace = this.world.AddOrGetComponent<UnitEntity, BuildingInProgressCountAtWorkPlace>(unit.jobTarget);
                            ++workPlace.count;

                            this.world.AddComponent<UnitEntity, UnitHiddenView>(unit.entity);

                        } else if (unit.jobTargetType == 1) {
                            
                            var workPlace = this.world.AddOrGetComponent<UnitEntity, UnitCountAtWorkPlace>(unit.jobTarget);
                            ++workPlace.count;

                        } else if (unit.jobTargetType == 2) {
                            
                            var workPlace = this.world.AddOrGetComponent<UnitEntity, UnitCountAtWorkPlace>(unit.jobTarget);
                            ++workPlace.count;

                            this.world.AddComponent<UnitEntity, UnitHiddenView>(unit.entity);

                        }

                    }

                } else if (this.peasantIdleEntities.Contains(unit) == true) {

                    if (state.peasantsMindTimer < 1f) continue;

                    var unitWorldPosition = unit.position;
                    
                    // Looking for the job
                    var playerOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                    PlayerEntity playerData;
                    this.world.GetEntityData(playerOwner.player, out playerData);

                    var found = false;
                    var dist = float.MaxValue;
                    {

                        UnitMaxPerBuildingInProgress compMax;
                        foreach (var buildingEntity in this.buildingsInProgressEntities.GetData()) {

                            var unitOwner = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(buildingEntity);
                            if (unitOwner.player != playerOwner.player) continue;

                            if (this.world.GetEntityData(buildingEntity, out UnitEntity buildingData) == true) {

                                ref var worldPos = ref buildingData.position;
                                var halfSize = this.mapFeature.GetWorldSize(buildingData.size) * 0.5f;
                                for (int bx = 0; bx < buildingData.size.x; ++bx) {
                                 
                                    for (int by = 0; by < buildingData.size.y; ++by) {

                                        var mapOffet = new UnityEngine.Vector2Int(bx, by);
                                        var buildingPoint = worldPos - halfSize + this.mapFeature.GetWorldSize(mapOffet);
                                        //if (this.fowFeature.IsRevealed(playerOwner.player, this.mapFeature.GetMapPositionFromWorld(buildingPoint)) == false) continue;
                                        
                                        var d = (buildingPoint - unitWorldPosition).sqrMagnitude;
                                        if (d <= dist) {

                                            compMax = this.world.GetComponent<UnitEntity, UnitMaxPerBuildingInProgress>(buildingEntity);
                                            if (compMax != null) {

                                                if (compMax.current >= compMax.max) continue;

                                            }
                                    
                                            if (this.pathfindingFeature.IsPathExists(unitWorldPosition, buildingPoint) == false) continue;

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

                            compMax = this.world.GetComponent<UnitEntity, UnitMaxPerBuildingInProgress>(unit.jobTarget);
                            if (compMax != null) {

                                ++compMax.current;

                            }

                        }

                    }

                    if (found == false) {

                        var fullValue = playerData.forestPercent + playerData.goldPercent;
                        var value = this.world.GetRandomRange(0f, fullValue);
                        if (value < playerData.forestPercent) {

                            UnitMaxPerWorkingPlace compMax;

                            // Search for nearest forest
                            dist = float.MaxValue;
                            foreach (var forestIndex in state.units) {

                                ref var forest = ref state.units[forestIndex];
                                if (this.forestEntities.Contains(forest) == true) {

                                    ref var worldPos = ref forest.position;
                                    
                                    var fow = this.world.GetComponent<UnitEntity, UnitFogOfWarComponent>(forest.entity);
                                    if (fow.IsRevealed(playerData.index) == false) continue;

                                    //if (this.fowFeature.IsRevealed(playerOwner.player, mapPosition) == false) continue;

                                    var d = (worldPos - unitWorldPosition).sqrMagnitude;
                                    if (d <= dist) {

                                        compMax = this.world.GetComponent<UnitEntity, UnitMaxPerWorkingPlace>(forest.entity);
                                        if (compMax != null) {

                                            if (compMax.current >= compMax.max) continue;

                                        }
                                        
                                        if (this.HasCastleNearby(playerData.index, worldPos) == false) continue;

                                        if (this.pathfindingFeature.IsPathExists(unitWorldPosition, worldPos) == false) continue;
                                        
                                        unit.jobTarget = forest.entity;
                                        unit.jobTargetType = 1;
                                        unit.jobTargetPos = worldPos;
                                        dist = d;
                                        found = true;

                                    }

                                }

                            }

                            if (found == true) {

                                compMax = this.world.GetComponent<UnitEntity, UnitMaxPerWorkingPlace>(unit.jobTarget);
                                if (compMax != null) {

                                    ++compMax.current;

                                }

                            }

                        } else {

                            UnitMaxPerWorkingPlace compMax;

                            // Search for nearest gold mine
                            dist = float.MaxValue;
                            foreach (var goldIndex in state.units) {

                                ref var goldMine = ref state.units[goldIndex];
                                if (this.goldMineEntities.Contains(goldMine) == true) {

                                    ref var worldPos = ref goldMine.position;
                                    
                                    var fow = this.world.GetComponent<UnitEntity, UnitFogOfWarComponent>(goldMine.entity);
                                    if (fow.IsRevealed(playerData.index) == false) continue;

                                    //if (this.fowFeature.IsRevealedAny(playerOwner.player, this.mapFeature.GetCellLeftBottomPosition(worldPos, goldMine.size), goldMine.size) == false) continue;

                                    var d = (worldPos - unitWorldPosition).sqrMagnitude;
                                    if (d <= dist) {

                                        var comp = this.world.GetComponent<UnitEntity, GoldMineComponent>(goldMine.entity);
                                        if (comp != null) {

                                            if (comp.capacity <= 0) continue;

                                        }

                                        compMax = this.world.GetComponent<UnitEntity, UnitMaxPerWorkingPlace>(goldMine.entity);
                                        if (compMax != null) {

                                            if (compMax.current >= compMax.max) continue;

                                        }

                                        if (this.HasCastleNearby(playerData.index, goldMine.position) == false) continue;

                                        var entrancePos = this.mapFeature.GetWorldEntrancePosition(goldMine.position, goldMine.size, goldMine.entrance);
                                        if (this.pathfindingFeature.IsPathExists(unitWorldPosition, entrancePos) == false) continue;

                                        unit.jobTarget = goldMine.entity;
                                        unit.jobTargetType = 2;
                                        unit.jobTargetPos = entrancePos;
                                        dist = d;
                                        found = true;

                                    }

                                }

                            }

                            if (found == true) {

                                compMax = this.world.GetComponent<UnitEntity, UnitMaxPerWorkingPlace>(unit.jobTarget);
                                if (compMax != null) {

                                    ++compMax.current;

                                }

                            }

                        }

                    }

                    if (found == true) {

                        this.world.RemoveComponents<UnitEntity, PeasantIdleState>(unit.entity);
                        this.world.AddComponent<UnitEntity, PeasantGoToWorkState>(unit.entity);

                    }
                    
                }
                
                //this.world.UpdateFilters(unit);

            }

            if (state.peasantsMindTimer >= 1f) {
                
                state.peasantsMindTimer -= 1f;
                
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

        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {}
        
    }
    
}