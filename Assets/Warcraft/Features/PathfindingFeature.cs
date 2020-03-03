using ME.ECS;
using UnityEngine;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class PathfindingFeature : Feature<TState> {

        private MapInfo mapInfo;
        
        protected override void OnConstruct(ref ConstructParameters parameters) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            this.mapInfo = mapFeature.mapInfo;
            var mapSize = this.mapInfo.mapSize;
            var cellSize = this.mapInfo.mapGrid.cellSize;

            var worldCenter = new UnityEngine.Vector3(mapSize.x * cellSize.x * 0.5f, mapSize.y * cellSize.y * 0.5f, 0f);
            var gridGraph = AstarPath.active.data.AddGraph(typeof(Pathfinding.GridGraph)) as Pathfinding.GridGraph;
            gridGraph.rotation = new UnityEngine.Vector3(-90f, 0f, 0f);
            gridGraph.center = worldCenter;
            gridGraph.SetDimensions(mapSize.x, mapSize.y, cellSize.x);
            gridGraph.collision.collisionCheck = false;
            gridGraph.collision.heightCheck = false;
            gridGraph.neighbours = Pathfinding.NumNeighbours.Eight;
            AstarPath.active.Scan(gridGraph);
            
            for (int x = 0; x < mapSize.x; ++x) {

                for (int y = 0; y < mapSize.y; ++y) {

                    var node = gridGraph.GetNode(x, y);
                    var isWalkable = true;
                    foreach (var tilemap in this.mapInfo.mapGrid.tilemaps) {

                        var tile = tilemap.GetTile(new UnityEngine.Vector3Int(x, y, 0));
                        foreach (var tileInfo in this.mapInfo.mapGrid.tilePathfindingData) {

                            if (tileInfo.tile == tile) {

                                if (tileInfo.walkable == false) isWalkable = false;
                                break;

                            }

                        }

                    }

                    node.Walkable = isWalkable;
                    
                }

            }

        }

        protected override void OnDeconstruct() {
            
        }

        public void SetWalkability(Vector2Int coord, bool state) {

            /*var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            var node = graph.GetNode(coord.x, coord.y);
            if (node != null) node.Walkable = state;*/
            this.SetWalkability(coord, Vector2Int.one, state);

        }

        public void SetWalkability(Vector2Int coord, Vector2Int size, bool state) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            var cellSize = mapFeature.grid.cellSize;
            var worldSize = mapFeature.GetWorldPositionFromMap(size);
            var worldSize3D = (worldSize - cellSize * 0.5f).XY();
            worldSize3D.z = 10f;
            var guo = new Pathfinding.GraphUpdateObject(new Bounds((mapFeature.GetWorldPositionFromMap(coord) + worldSize * 0.5f).XY(), worldSize3D));
            guo.modifyWalkability = true;
            guo.setWalkability = state;
            
            /*var v1 = new Vector3(guo.bounds.min.x, guo.bounds.min.y, 0f);
            var v2 = new Vector3(guo.bounds.min.x, guo.bounds.max.y, 0f);
            var v3 = new Vector3(guo.bounds.max.x, guo.bounds.max.y, 0f);
            var v4 = new Vector3(guo.bounds.max.x, guo.bounds.min.y, 0f);
            Debug.DrawLine(v1, v2, Color.red, 50f);
            Debug.DrawLine(v2, v3, Color.red, 50f);
            Debug.DrawLine(v3, v4, Color.red, 50f);
            Debug.DrawLine(v4, v1, Color.red, 50f);*/

            AstarPath.active.UpdateGraphs(guo, this.world.GetTimeSinceStart());
            
            /*
            var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {
                    
                    var node = graph.GetNode(coord.x + x, coord.y + y);
                    if (node != null) node.Walkable = state;

                }
                
            }*/

            //AstarPath.active.FloodFill();

        }

        public Vector2 GetWalkableNodeByDirection(Vector2 position, int startDirection) {

            var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            var startNode = graph.GetNearest(position.XY()).node as Pathfinding.GridNode;
            
            var dir = 0;
            for (int i = startDirection; i < startDirection + 8; ++i) {

                dir = i;
                if (dir >= 8) dir -= 8;
                var node = startNode.GetNeighbourAlongDirection(dir);
                if (node != null && node.Walkable == true) {

                    return ((Vector3)node.position).XY();

                }

            }

            return position;

        }

        public bool IsValid(Vector2 worldPosition, Vector2Int size) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            var mapWorldCenter = mapFeature.GetMapPositionFromWorld(mapFeature.GetWorldCellPosition(worldPosition, size));
            var coord = mapWorldCenter - new Vector2Int(size.x / 2, size.y / 2);
            var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {
                    
                    var node = graph.GetNode(coord.x + x, coord.y + y);
                    if (node == null || node.Walkable == false) return false;

                }
                
            }

            return true;

        }

        public void StopMovement(Entity entity) {
            
            this.world.RemoveComponents<PathfindingPathComponent>(entity);
            
        }

        public UnityEngine.Vector2 MoveTowards(Entity entity, UnityEngine.Vector2 from, ref UnityEngine.Vector2 to, float movementDelta) {

            if (this.world.HasComponent<UnitEntity, PathfindingPathComponent>(entity) == true) {

                var comp = this.world.GetComponent<UnitEntity, PathfindingPathComponent>(entity);
                if (comp.index < comp.nodes.Count - 1) {

                    //var current = comp.nodes[comp.index];
                    var target = comp.nodes[comp.index + 1];

                    //var posFrom = ((Vector3)current.position).XY();
                    var posTo = ((Vector3)target.position).XY();

                    var nextStep = UnityEngine.Vector2.MoveTowards(from, posTo, movementDelta);
                    if ((nextStep - posTo).sqrMagnitude <= 0.01f) {

                        ++comp.index;

                    }

                    return nextStep;

                } else {
                    
                    var result = (Vector3)comp.nodes[comp.nodes.Count - 1].position;
                    this.world.RemoveComponents<PathfindingPathComponent>(entity);
                    return result.XY();

                }

            } else {

                var result = PoolList<Pathfinding.GraphNode>.Spawn(10);
                
                var mapFeature = this.world.GetFeature<MapFeature>();
                var mapFrom = mapFeature.GetMapPositionFromWorld(from);
                var mapTo = mapFeature.GetMapPositionFromWorld(to);
                var pathState = this.GetPath(result, mapFrom, mapTo);
                
                if (pathState != Pathfinding.PathCompleteState.Complete || this.IsPathValid(result, from, to) == false) {
                    
                    PoolList<Pathfinding.GraphNode>.Recycle(ref result);
                    return from;
                    
                }
                
                to = ((Vector3)result[result.Count - 1].position).XY();
                var comp = this.world.AddComponent<UnitEntity, PathfindingPathComponent>(entity);
                comp.index = 0;
                comp.nodes = result;
                comp.@from = from;
                comp.to = to;

                return from;

            }

        }

        public bool IsPathValid(System.Collections.Generic.List<Pathfinding.GraphNode> path, Vector2 from, Vector2 to) {
            
            var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            //var endNode = graph.GetNearest(to.XY()).node;
            var lastPoint = ((Vector3)path[path.Count - 1].position);
            var maxNodeDistance = graph.nodeSize * 1.5f;
            return (to - lastPoint.XY()).sqrMagnitude <= maxNodeDistance * maxNodeDistance * 2f;
            /*var endPathNode = graph.GetNearest(lastPoint).node as Pathfinding.GridNode;
            if (endPathNode == endNode) return true;
            if (endPathNode.EdgeNode == true) return false;
            
            for (int i = 0; i < 8; ++i) {

                //var nb = endPathNode.GetNeighbourAlongDirection(i);
                //if (nb == null) continue;
                
                var nIndex = endPathNode.NodeInGridIndex + graph.neighbourOffsets[i];
                if (nIndex >= 0 && nIndex < graph.nodes.Length) {

                    var conn = graph.nodes[nIndex];
                    if (endNode == conn) return true;

                }

            }

            return false;*/

        }

        public bool IsPathExists(Vector2 from, Vector2 to) {

            return this.IsPathPossible(AstarPath.active.GetNearest(from.XY()).node, AstarPath.active.GetNearest(to.XY()).node);
            /*if (hasPath == false) return false;
            
            var result = PoolList<Pathfinding.GraphNode>.Spawn(10);
                
            var mapFeature = this.world.GetFeature<MapFeature>();
            var mapFrom = mapFeature.GetMapPositionFromWorld(from);
            var mapTo = mapFeature.GetMapPositionFromWorld(to);
            var pathState = this.GetPath(result, mapFrom, mapTo);
            
            if (pathState != Pathfinding.PathCompleteState.Complete || this.IsPathValid(result, from, to) == false) {
                    
                PoolList<Pathfinding.GraphNode>.Recycle(ref result);
                return false;
                    
            }

            PoolList<Pathfinding.GraphNode>.Recycle(ref result);
            return true;*/

        }

        private bool IsPathPossible(Pathfinding.GraphNode startNode, Pathfinding.GraphNode endNode) {
            
            var hasPath = Pathfinding.PathUtilities.IsPathPossible(startNode, endNode);
            if (hasPath == false) {

                var endNodeGn = endNode as Pathfinding.GridNode;
                if (endNodeGn.EdgeNode == true) return false;
                
                var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
                for (int i = 0; i < 8; ++i) {

                    //var nb = endNodeGn.GetNeighbourAlongDirection(i);
                    //if (nb == null) continue;
                    
                    var conn = graph.nodes[endNodeGn.NodeInGridIndex + graph.neighbourOffsets[i]];
                    if (conn == null) continue;
                    
                    if (Pathfinding.PathUtilities.IsPathPossible(startNode, conn) == true) return true;

                }

            }

            return hasPath;

        }

        private Pathfinding.PathCompleteState GetPath(System.Collections.Generic.List<Pathfinding.GraphNode> result, UnityEngine.Vector2Int from, UnityEngine.Vector2Int to) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            var worldFrom = mapFeature.GetWorldPositionFromMap(from, true, true).XY();
            var worldTo = mapFeature.GetWorldPositionFromMap(to, true, true).XY();
            var path = Pathfinding.ABPath.Construct(worldFrom, worldTo);
            AstarPath.StartPath(path);
            AstarPath.BlockUntilCalculated(path);
            //result.Add(AstarPath.active.GetNearest(worldFrom).node);
            result.AddRange(path.path);
            //result.Add(AstarPath.active.GetNearest(worldTo).node);
            var completeState = path.CompleteState;
            Pathfinding.PathPool.Pool(path);

            return completeState;

        }

    }

}