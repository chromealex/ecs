using ME.ECS;
using ME.ECS.Views.Providers;
using ViewId = System.UInt64;
using UnityEngine;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Systems;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsFeature : Feature<TState> {

        private UnitInfos allUnits;
        private System.Collections.Generic.Dictionary<int, ViewId> unitInfoToViews = new System.Collections.Generic.Dictionary<int, ViewId>();
        private System.Collections.Generic.Dictionary<int, ViewId> unitInfoToViewsDead = new System.Collections.Generic.Dictionary<int, ViewId>();
        private System.Collections.Generic.Dictionary<int, UnitInfo> unitInfoToInfo = new System.Collections.Generic.Dictionary<int, UnitInfo>();
        
        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<UnitsSystem>();
            
            this.allUnits = UnityEngine.Resources.Load<UnitInfos>("Units/AllUnits");
            foreach (var unitInfo in this.allUnits.units) {
                
                this.unitInfoToViews.Add(unitInfo.unitTypeId, this.world.RegisterViewSource<UnitEntity>(unitInfo.viewSource));
                this.unitInfoToViewsDead.Add(unitInfo.unitTypeId, this.world.RegisterViewSource<UnitEntity>(unitInfo.viewSourceDead));
                this.unitInfoToInfo.Add(unitInfo.unitTypeId, unitInfo);
                
            }

        }

        protected override void OnDeconstruct() {
            
        }

        public Entity UpgradeUnit(Entity unitEntity, int unitTypeId, ResourcesStorage cost, bool placeComplete = false) {
            
            if (this.world.GetEntityData(unitEntity, out UnitEntity unitData) == true) {
                
                var unitInfo = this.unitInfoToInfo[unitTypeId];
                var comp = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unitEntity);
                var playerEntity = comp.player;
                
                this.world.DestroyAllViews<UnitEntity>(unitEntity);
                ViewId sourceId;
                if (this.unitInfoToViews.TryGetValue(unitTypeId, out sourceId) == true) {
                    
                    this.world.InstantiateView<UnitEntity>(sourceId, unitEntity);
                    
                }
                
                if (placeComplete == false) {
                    
                    var progress = this.world.AddOrGetComponent<UnitEntity, UnitBuildingProgress>(unitEntity);
                    progress.progress = 0f;
                    progress.time = unitInfo.buildingTime;
                    
                    var compMax = this.world.AddOrGetComponent<UnitEntity, UnitMaxPerBuildingInProgress>(unitEntity);
                    compMax.current = 0;
                    compMax.max = 1;
                    
                    this.world.RemoveComponents<UnitEntity, UnitCompleteComponent>(unitEntity);

                } else {
                    
                    var units = this.world.AddOrGetComponent<PlayerEntity, PlayerUnitsComponent>(playerEntity);
                    units.AddUnit(unitTypeId);

                    this.world.AddOrGetComponent<UnitEntity, UnitCompleteComponent>(unitEntity);

                }

                var unitCost = this.world.AddOrGetComponent<UnitEntity, UnitCostComponent>(unitEntity);
                unitCost.cost = cost;

                var unitType = this.world.AddOrGetComponent<UnitEntity, UnitInfoComponent>(unitEntity);
                unitType.unitInfo = unitInfo;

                var sightRange = this.world.AddOrGetComponent<UnitEntity, UnitSightRangeComponent>(unitEntity);
                sightRange.range = unitInfo.sightRange;
                
                unitInfo.OnUpgrade(this.world, unitEntity);
                
                return unitEntity;
                
            }
            
            return Entity.Empty;
            
        }

        public bool UnitDie(Entity unitEntity) {

            if (this.world.HasComponent<UnitEntity, UnitDeathState>(unitEntity) == true) return false;
            
            var mapFeature = this.world.GetFeature<MapFeature>();
            var pathfindingFeature = this.world.GetFeature<PathfindingFeature>();

            this.world.GetEntityData(unitEntity, out UnitEntity unitData);
            
            pathfindingFeature.ResetUnitWalkStep(unitEntity, AstarPath.active.GetNearest(unitData.position).node);

            var worldPos = mapFeature.GetWorldBuildingPosition(unitData.position, unitData.size);
            pathfindingFeature.SetWalkability(worldPos, unitData.size, true, unitEntity);

            var unitInfoComponent = this.world.AddComponent<UnitEntity, UnitInfoComponent>(unitEntity);
            var unitInfo = unitInfoComponent.unitInfo;

            var newEntity = this.world.AddEntity(new UnitEntity() { position = unitData.position });
            this.world.AddComponent<UnitEntity, UnitDeathState>(newEntity);
            unitInfoComponent = this.world.AddComponent<UnitEntity, UnitInfoComponent>(newEntity);
            unitInfoComponent.unitInfo = unitInfo;

            if (this.unitInfoToViews.TryGetValue(unitInfo.unitTypeId, out ViewId sourceId) == true) {
                
                this.world.InstantiateView<UnitEntity>(sourceId, newEntity);
                
            }

            this.world.RemoveEntity<UnitEntity>(unitEntity);
            
            return true;

        }

        public Entity SpawnUnit(Entity playerEntity, int unitTypeId, UnityEngine.Vector2 position, ResourcesStorage cost, bool ghost = false, bool placeComplete = false) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            
            var unitInfo = this.unitInfoToInfo[unitTypeId];
            var size = unitInfo.viewSource.size;
            var entrance = unitInfo.entrancePoint;
            //var cellSize = mapFeature.grid.cellSize;

            var worldPos = mapFeature.GetWorldBuildingPosition(position, size);
            //var cellPos = mapFeature.GetMapPositionFromWorld(worldPos + new Vector2(size.x % 2 == 0 ? cellSize.x * 0.5f : 0f, size.x % 2 == 0 ? cellSize.y * 0.5f : 0f));
            var leftBottomPos = mapFeature.GetCellLeftBottomPosition(worldPos, size);
            /*Debug.Log("Spawn: " + unitTypeId + " :: " + position.x + ", " + position.y + " :: " + worldPos.x + ", " + worldPos.y + " :: " + leftBottomPos.x + ", " + leftBottomPos.y);
            Debug.DrawLine(Vector3.zero, position.XY(), Color.white, 10f);
            Debug.DrawLine(Vector3.zero, worldPos.XY(), Color.cyan, 10f);
            Debug.DrawLine(Vector3.zero, mapFeature.GetWorldPositionFromMap(leftBottomPos).XY(), Color.red, 10f);*/
            var mapPosEntrance = leftBottomPos + entrance;
            
            var entity = this.world.AddEntity(new UnitEntity() { position = worldPos/*position + worldHalfSize * 0.5f*/, size = size, entrance = entrance, rotation = Quaternion.identity });
            var comp = this.world.AddComponent<UnitEntity, UnitPlayerOwnerComponent>(entity);
            comp.player = playerEntity;

            var unitInfoComponent = this.world.AddComponent<UnitEntity, UnitInfoComponent>(entity);
            unitInfoComponent.unitInfo = unitInfo;

            if (this.unitInfoToViews.TryGetValue(unitTypeId, out ViewId sourceId) == true) {
                
                this.world.InstantiateView<UnitEntity>(sourceId, entity);
                
            }

            if (unitInfo.walkable == false && ghost == false) {

                var pathfindingFeature = this.world.GetFeature<PathfindingFeature>();
                pathfindingFeature.SetWalkability(worldPos, size, false, entity);
                if (unitInfo.hasEntrancePoint == true) pathfindingFeature.SetWalkability(mapPosEntrance, new Vector2Int(1, 1), true, entity);

            }

            if (ghost == false) {

                if (placeComplete == false) {
                    
                    var progress = this.world.AddComponent<UnitEntity, UnitBuildingProgress>(entity);
                    progress.progress = 0f;
                    progress.time = unitInfo.buildingTime;

                    var compMax = this.world.AddComponent<UnitEntity, UnitMaxPerBuildingInProgress>(entity);
                    compMax.current = 0;
                    compMax.max = 1;
                    
                } else {
                    
                    var units = this.world.AddOrGetComponent<PlayerEntity, PlayerUnitsComponent>(playerEntity);
                    units.AddUnit(unitTypeId);

                    this.world.AddOrGetComponent<UnitEntity, UnitCompleteComponent>(entity);

                }

                var health = this.world.AddComponent<UnitEntity, UnitHealthComponent>(entity);
                health.value = unitInfo.health;
                health.maxValue = unitInfo.health;
                
                this.world.AddComponent<UnitEntity, UnitInteractableComponent>(entity);
                
                var sightRange = this.world.AddOrGetComponent<UnitEntity, UnitSightRangeComponent>(entity);
                sightRange.range = unitInfo.sightRange;

                var unitCost = this.world.AddOrGetComponent<UnitEntity, UnitCostComponent>(entity);
                unitCost.cost = cost;

                unitInfo.OnCreate(this.world, entity);

            } else {
                
                this.world.AddComponent<UnitEntity, UnitGhosterComponent>(entity);

            }

            return entity;

        }

    }

}