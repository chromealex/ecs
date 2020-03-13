using ME.ECS;
using UnityEngine;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Entities;
    
    public class MapFeature : Feature<TState> {

        public MapInfo mapInfo;
        public MapGrid grid;
        
        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.mapInfo = Resources.Load<MapInfo>("Maps/TestMap/TestMap");
            
            var grid = Grid.Instantiate(this.mapInfo.mapGrid);
            grid.transform.position = Vector3.zero;
            this.grid = grid;

        }

        protected override void OnDeconstruct() {
            
        }

        public void CutDownTree(Vector2Int mapPosition) {
            
            this.grid.forestTilemap.SetTile(new Vector3Int(mapPosition.x, mapPosition.y, 0), this.mapInfo.forestFelledTile);
            
        }

        public Vector2Int ClampToMap(Vector2Int point) {

            point.x = Mathf.Clamp(point.x, 0, this.mapInfo.mapSize.x);
            point.y = Mathf.Clamp(point.y, 0, this.mapInfo.mapSize.y);
            return point;

        }

        public void ClampCamera(Vector2 cameraSize, ref Vector3 position) {

            var worldSize = this.GetWorldSize(this.mapInfo.mapSize);
            var worldCenter = worldSize * 0.5f;
            var bounds = new Bounds(new Vector3(worldCenter.x, worldCenter.y, 0f), new Vector3(worldSize.x - cameraSize.x, worldSize.y - cameraSize.y, 0f));
            if (position.x <= bounds.min.x) {

                position.x = bounds.min.x;

            }
            
            if (position.y <= bounds.min.y) {
                
                position.y = bounds.min.y;

            }
            
            if (position.x >= bounds.max.x) {

                position.x = bounds.max.x;

            }
            
            if (position.y >= bounds.max.y) {

                position.y = bounds.max.y;

            }

        }

        public int GetOrder(Vector2 position) {

            var orderCellSize = this.mapInfo.mapGrid.cellSize.y / 10f;
            var order = -Mathf.RoundToInt(position.y / orderCellSize);
            return order;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsInBounds(Vector2 worldPos, Vector2Int size, Vector2 testPos) {

            var worldSize = this.GetWorldSize(size);
            var bounds = new Rect(worldPos - worldSize * 0.5f, worldSize);
            return bounds.Contains(testPos);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsInBounds(Vector2 worldPos, Vector2Int size, Vector2 testPos, Vector2 testSize) {

            var worldSize = this.GetWorldSize(size);
            var bounds = new Rect(worldPos - worldSize * 0.5f, worldSize);
            var testBounds = new Rect(testPos - testSize * 0.5f, testSize);
            return bounds.Overlaps(testBounds);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2 GetWorldBuildingPosition(Vector2 center, Vector2Int size) {

            var cell = this.GetMapPositionFromWorld(center - new Vector2(0.001f, 0.001f));
            var worldPos = this.GetWorldPositionFromMap(cell);
            var cellSize = this.mapInfo.mapGrid.cellSize;
            
            if (size.x % 2 == 0) {

                worldPos.x += cellSize.x * 0.5f;

            }

            if (size.y % 2 == 0) {
                
                worldPos.y += cellSize.y * 0.5f;
                
            }

            return worldPos;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2 GetWorldEntrancePosition(Vector2 position, Vector2Int size, Vector2Int entrance) {

            var worldSize = this.GetWorldSize(size);
            var entranceWorld = this.GetWorldPositionFromMap(entrance);
            var bottomLeftWorld = position - worldSize * 0.5f;
            return bottomLeftWorld + entranceWorld;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2 GetWorldLeftBottomPosition(Vector2 position, Vector2Int size) {
            
            var worldSize = this.GetWorldSize(size);
            var bottomLeftWorld = position - worldSize * 0.5f;
            return bottomLeftWorld + new Vector2(this.mapInfo.mapGrid.cellSize.x * 0.5f, this.mapInfo.mapGrid.cellSize.y * 0.5f);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2Int GetCellLeftBottomPosition(Vector2 position, Vector2Int size) {
            
            return this.GetMapPositionFromWorld(this.GetWorldLeftBottomPosition(position, size));
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2Int GetCellLeftBottomPosition(Vector2Int position, Vector2Int size) {
            
            return this.GetMapPositionFromWorld(this.GetWorldLeftBottomPosition(this.GetWorldPositionFromMap(position), size));
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2 GetWorldPositionFromInput(Vector2 input) {

            var cameraFeature = this.world.GetFeature<CameraFeature>();
            var camera = cameraFeature.GetCamera();
            var worldPoint = camera.ScreenToWorldPoint(input.XY());
            return worldPoint.XY();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2 GetWorldPositionFromMap(Vector2Int pos) {
            
            var cellSize = this.grid.cellSize;
            return new Vector2(pos.x * cellSize.x + cellSize.x * 0.5f, pos.y * cellSize.y + cellSize.y * 0.5f);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2 GetWorldSize(Vector2Int size) {
            
            var cellSize = this.grid.cellSize;
            return new Vector2(size.x * cellSize.x, size.y * cellSize.y);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Vector2Int GetMapPositionFromWorld(Vector2 worldPos) {

            var cellSize = this.grid.cellSize;
            return new Vector2Int(Mathf.FloorToInt(worldPos.x / cellSize.x), Mathf.FloorToInt(worldPos.y / cellSize.y));

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsExists(Vector2Int cell) {

            return cell.x > 0 && cell.y > 0 && cell.x < this.mapInfo.mapSize.x && cell.y < this.mapInfo.mapSize.y;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void GetCellsInRange(Vector2Int center, float range, System.Collections.Generic.List<Vector2Int> result) {

            var rangeSize = this.GetMapPositionFromWorld(new Vector2(range, range));
            var cellRange = rangeSize.x;
            var cellRangeSqr = cellRange * cellRange;
            var bottomLeft = center - rangeSize;
            var topRight = center + rangeSize;
            for (int x = bottomLeft.x; x < topRight.x; ++x) {
             
                for (int y = bottomLeft.y; y < topRight.y; ++y) {
                    
                    var cellPos = new Vector2Int(x, y);
                    if (this.IsExists(cellPos) == true && (center - cellPos).sqrMagnitude <= cellRangeSqr) {
                        
                        result.Add(cellPos);
                        
                    }

                }
                
            }

        }

    }

}