using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour {

    [System.Serializable]
    public struct TilePathfindingData {

        public UnityEngine.Tilemaps.TileBase tile;
        public bool walkable;

    }

    public Grid grid;
    public Vector2 cellSize;
    public UnityEngine.Tilemaps.Tilemap[] tilemaps;
    public UnityEngine.Tilemaps.Tilemap forestTilemap;
    public UnityEngine.Tilemaps.Tilemap fowRevealedTilemap;
    public UnityEngine.Tilemaps.Tilemap fowTilemap;
    public TilePathfindingData[] tilePathfindingData; 
    public MapSpawnPoint[] spawnPoints;
    
    public void OnValidate() {

        this.spawnPoints = this.GetComponentsInChildren<MapSpawnPoint>(true);

    }

}
