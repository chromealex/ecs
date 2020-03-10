using ME.ECS;
using UnityEngine;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class ForestFeature : Feature<TState> {

        protected override void OnConstruct(ref ConstructParameters parameters) {
            
            var mapFeature = this.world.GetFeature<MapFeature>();
            var pathfindingFeature = this.world.GetFeature<PathfindingFeature>();

            var forestViewId = this.world.RegisterViewSource<UnitEntity>(mapFeature.mapInfo.foresetViewSource);
            
            var tilemap = mapFeature.grid.forestTilemap;
            foreach (var pos in tilemap.cellBounds.allPositionsWithin) {

                var tile = tilemap.GetTile(pos);
                if (tile == mapFeature.mapInfo.forestTile) {

                    var lifesCount = 3;
                    
                    var position = pos;
                    var pos2d = new Vector2Int(position.x, position.y);
                    var ent = this.world.AddEntity(new UnitEntity() { position = mapFeature.GetWorldPositionFromMap(pos2d), size = Vector2Int.one });
                    this.world.AddComponent<UnitEntity, ForestComponent>(ent);
                    var comp = this.world.AddComponent<UnitEntity, UnitLifesComponent>(ent);
                    comp.lifes = lifesCount;
                    var compMax = this.world.AddComponent<UnitEntity, UnitMaxPerWorkingPlace>(ent);
                    compMax.current = 0;
                    compMax.max = lifesCount;
                    this.world.AddComponent<UnitEntity, UnitCompleteComponent>(ent);
                    
                    pathfindingFeature.SetWalkability(pos2d, false);
                    
                    this.world.InstantiateView<UnitEntity>(forestViewId, ent);

                }

            }

        }

        protected override void OnDeconstruct() {
            
        }

    }

}