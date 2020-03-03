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
        
        public bool IsInBounds(Vector2 worldPos, Vector2Int size, Vector2 testPos) {

            var worldSize = this.GetWorldPositionFromMap(size);
            var bounds = new Rect(worldPos - worldSize * 0.5f, worldSize);
            return bounds.Contains(testPos);

        }

        public bool IsInBounds(Vector2 worldPos, Vector2Int size, Vector2 testPos, Vector2 testSize) {

            var worldSize = this.GetWorldPositionFromMap(size);
            var bounds = new Rect(worldPos - worldSize * 0.5f, worldSize);
            var testBounds = new Rect(testPos - testSize * 0.5f, testSize);
            return bounds.Overlaps(testBounds);

        }

        public void ClampCamera(Vector2 cameraSize, ref Vector3 position) {

            var worldSize = this.GetWorldPositionFromMap(this.mapInfo.mapSize);
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

        public Vector2 GetWorldCellPosition(Vector2 center, Vector2Int size) {

            return this.GetWorldPositionFromMap(this.GetMapPositionFromWorld(center), addHalfCellX: size.x % 2 != 0, addHalfCellY: size.y % 2 != 0);
            
        }

        public Vector2 GetWorldEntrancePosition(Vector2 position, Vector2Int size, Vector2Int entrance) {

            var worldSize = this.GetWorldPositionFromMap(size);
            var entranceWorld = this.GetWorldPositionFromMap(entrance, true, true);
            var bottomLeftWorld = position - worldSize * 0.5f;
            return bottomLeftWorld + entranceWorld;

        }

        public Vector2 GetWorldPositionFromInput(Vector2 input) {

            var cameraFeature = this.world.GetFeature<CameraFeature>();
            var camera = cameraFeature.GetCamera();
            var worldPoint = camera.ScreenToWorldPoint(input.XY());
            return worldPoint.XY();

        }

        public Vector2 GetWorldPositionFromMap(Vector2Int pos, bool addHalfCellX = false, bool addHalfCellY = false) {
            
            var cellSize = this.grid.cellSize;
            var result = new Vector2(pos.x * cellSize.x, pos.y * cellSize.y);
            result += new Vector2(addHalfCellX == true ? cellSize.x * 0.5f : 0f, addHalfCellY == true ? cellSize.y * 0.5f : 0f);
            
            return result;

        }

        public Vector2Int GetMapPositionFromWorld(Vector2 worldPos) {

            var cellSize = this.grid.cellSize;
            return new Vector2Int(Mathf.FloorToInt(worldPos.x / cellSize.x), Mathf.FloorToInt(worldPos.y / cellSize.y));

        }

    }

}