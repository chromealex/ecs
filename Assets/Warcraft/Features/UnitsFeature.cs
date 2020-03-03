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
        private System.Collections.Generic.Dictionary<int, UnitInfo> unitInfoToInfo = new System.Collections.Generic.Dictionary<int, UnitInfo>();
        
        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<UnitsSystem>();
            
            this.allUnits = UnityEngine.Resources.Load<UnitInfos>("Units/AllUnits");
            foreach (var unitInfo in this.allUnits.units) {
                
                this.unitInfoToViews.Add(unitInfo.unitTypeId, this.world.RegisterViewSource<UnitEntity>(unitInfo.viewSource));
                this.unitInfoToInfo.Add(unitInfo.unitTypeId, unitInfo);
                
            }

        }

        protected override void OnDeconstruct() {
            
        }

        public Entity UpgradeUnit(Entity unitEntity, int unitTypeId, bool placeComplete = false) {
            
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
                    
                    var compMax = this.world.AddOrGetComponent<UnitEntity, UnitsMaxPerBuildingInProgress>(unitEntity);
                    compMax.current = 0;
                    compMax.max = 1;
                    
                }
                
                var units = this.world.AddOrGetComponent<PlayerEntity, PlayerUnitsComponent>(playerEntity);
                units.AddUnit(unitTypeId);
                
                unitInfo.OnUpgrade(this.world, unitEntity);
                
                return unitEntity;
                
            }
            
            return Entity.Empty;
            
        }

        public Entity SpawnUnit(Entity playerEntity, int unitTypeId, UnityEngine.Vector2 position, bool ghost = false, bool placeComplete = false) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            
            var unitInfo = this.unitInfoToInfo[unitTypeId];
            var size = unitInfo.viewSource.size;
            var halfSize = unitInfo.viewSource.halfSize;
            var entrance = unitInfo.entrancePoint;
            
            var mapPos = mapFeature.GetMapPositionFromWorld(position);
            var worldPos = mapFeature.GetWorldCellPosition(position, size);
            var leftBottomPos = mapPos - halfSize;
            //var sizeWorld = mapFeature.GetWorldPositionFromMap(size);
            //var mapHalfSize = new Vector2Int(size.x / 2, size.y / 2);
            //var worldHalfSize = mapFeature.GetWorldPositionFromMap(mapHalfSize);
            var mapPosEntrance = leftBottomPos + entrance;//mapFeature.GetMapPositionFromWorld(mapFeature.GetWorldEntrancePosition(position, size, entrance));
            
            var entity = this.world.AddEntity(new UnitEntity() { position = worldPos/*position + worldHalfSize * 0.5f*/, size = size, entrance = entrance, rotation = Quaternion.identity });
            var comp = this.world.AddComponent<UnitEntity, UnitPlayerOwnerComponent>(entity);
            comp.player = playerEntity;

            var unitInfoComponent = this.world.AddComponent<UnitEntity, UnitInfoComponent>(entity);
            unitInfoComponent.unitInfo = unitInfo;

            ViewId sourceId;
            if (this.unitInfoToViews.TryGetValue(unitTypeId, out sourceId) == true) {
                
                this.world.InstantiateView<UnitEntity>(sourceId, entity);
                
            }

            if (unitInfo.walkable == false && ghost == false) {

                var pathfindingFeature = this.world.GetFeature<PathfindingFeature>();
                pathfindingFeature.SetWalkability(leftBottomPos, size, false);
                if (unitInfo.hasEntrancePoint == true) pathfindingFeature.SetWalkability(mapPosEntrance, new Vector2Int(1, 1), true);

            }

            if (ghost == false) {

                if (placeComplete == false) {
                    
                    var progress = this.world.AddComponent<UnitEntity, UnitBuildingProgress>(entity);
                    progress.progress = 0f;
                    progress.time = unitInfo.buildingTime;

                    var compMax = this.world.AddComponent<UnitEntity, UnitsMaxPerBuildingInProgress>(entity);
                    compMax.current = 0;
                    compMax.max = 1;
                    
                }

                var units = this.world.AddOrGetComponent<PlayerEntity, PlayerUnitsComponent>(playerEntity);
                units.AddUnit(unitTypeId);

                unitInfo.OnCreate(this.world, entity);

            } else {
                
                this.world.AddComponent<UnitEntity, UnitGhosterComponent>(entity);

            }

            return entity;

        }

    }

}