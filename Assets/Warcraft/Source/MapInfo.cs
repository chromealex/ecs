using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MapInfo : ScriptableObject {

    [System.Serializable]
    public struct PlayerData {

        public int playerIndex;

    }

    public UnityEngine.Tilemaps.TileBase forestTile;
    public UnityEngine.Tilemaps.TileBase forestFelledTile;
    public UnityEngine.Tilemaps.TileBase fogOfWarTile;
    public Warcraft.Views.ForestView foresetViewSource = new Warcraft.Views.ForestView();
    public Vector2Int mapSize;
    public MapGrid mapGrid;
    public PlayerData[] playersData;

}
