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
            
            this.SetWalkability(coord, state, Entity.Empty);
            
        }

        public void SetWalkability(Vector2Int coord, bool state, Entity entity) {

            /*var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            var node = graph.GetNode(coord.x, coord.y);
            if (node != null) node.Walkable = state;*/
            this.SetWalkability(coord, Vector2Int.one, state, entity);

        }

        public void SetWalkability(Vector2Int coord, Vector2Int size, bool state, Entity entity) {
            
            var mapFeature = this.world.GetFeature<MapFeature>();
            this.SetWalkability(mapFeature.GetWorldPositionFromMap(coord), size, state, entity);
            
        }
                
        public void SetWalkability(Vector2 center, Vector2Int size, bool state, Entity entity) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            //var cellSize = mapFeature.grid.cellSize;
            var worldSize = mapFeature.GetWorldSize(size);
            var worldSize3D = worldSize.XY();
            worldSize3D.z = 10f;
            var guo = new Pathfinding.GraphUpdateObject(new Bounds(center.XY(), worldSize3D));
            guo.modifyWalkability = true;
            guo.setWalkability = state;
            //guo.trackChangedNodes = true;
            
            /*var v1 = new Vector3(guo.bounds.min.x, guo.bounds.min.y, 0f);
            var v2 = new Vector3(guo.bounds.min.x, guo.bounds.max.y, 0f);
            var v3 = new Vector3(guo.bounds.max.x, guo.bounds.max.y, 0f);
            var v4 = new Vector3(guo.bounds.max.x, guo.bounds.min.y, 0f);
            Debug.DrawLine(v1, v2, Color.red, 50f);
            Debug.DrawLine(v2, v3, Color.red, 50f);
            Debug.DrawLine(v3, v4, Color.red, 50f);
            Debug.DrawLine(v4, v1, Color.red, 50f);*/

            AstarPath.active.UpdateGraphs(guo, this.world.GetTimeSinceStart());
            AstarPath.active.FlushGraphUpdates();

            var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            var leftBottomPos = mapFeature.GetWorldLeftBottomPosition(center, size);
            var leftBottomMapPos = mapFeature.GetMapPositionFromWorld(leftBottomPos);
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {
                    
                    var node = graph.GetNode(leftBottomMapPos.x + x, leftBottomMapPos.y + y);
                    if (node != null) {
                        
                        node.userData = entity;
                        
                    }

                }
                
            }

            /*
            if (guo.changedNodes != null) {

                foreach (var node in guo.changedNodes) {

                    node.userData = entity;

                }

            }
            Pathfinding.Util.ListPool<Pathfinding.GraphNode>.Release(ref guo.changedNodes);*/
            
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
            var mapWorldCenter = mapFeature.GetMapPositionFromWorld(mapFeature.GetWorldBuildingPosition(worldPosition, size));
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

        public void StopMovement(Entity entity, bool repath) {

            if (this.world.HasComponent<UnitEntity, PathfindingPathComponent>(entity) == true) {

                this.world.AddComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterStopState>(entity);

            } else {
                
                this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(entity);
                this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterAttackState>(entity);
                this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterStopState>(entity);

                this.world.GetEntityData(entity, out UnitEntity unitData);
                var idle = this.world.AddComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                idle.idleNode = AstarPath.active.GetNearest(unitData.position.XY()).node;
                if (idle.idleNode.userData == Entity.Empty || idle.idleNode.userData == entity) {

                    idle.idleNode.Walkable = false;
                    idle.idleNode.userData = entity;

                }
                
            }

            /*this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
            this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(entity);
            this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterAttackState>(entity);

            this.world.GetEntityData(entity, out UnitEntity unitData);
            var idle = this.world.AddComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
            idle.idleNode = AstarPath.active.GetNearest(unitData.position.XY()).node;
            if (idle.idleNode.userData == Entity.Empty || idle.idleNode.userData == entity) {

                idle.idleNode.Walkable = false;
                idle.idleNode.userData = entity;

            }
            
            if (this.world.HasComponent<UnitEntity, PathfindingPathComponent>(entity) == true) {

                if (repath == true) {

                    var comp = this.world.GetComponent<UnitEntity, PathfindingPathComponent>(entity);
                    var current = comp.nodes[comp.index];
                    var target = (comp.index < comp.nodes.Count - 1 ? comp.nodes[comp.index + 1] : null);
                    this.ResetUnitWalkStep(entity, current, target == null ? current : target);
                    this.world.RemoveComponents<UnitEntity, PathfindingPathComponent>(entity);

                }

            }*/

        }

        public void ResetUnitWalkStep(Entity entity, Pathfinding.GraphNode from, Pathfinding.GraphNode to) {

            this.SetUnitWalkStep(entity, from, true);
            if (from.userData == entity) from.userData = Entity.Empty;
            this.SetUnitWalkStep(entity, to, true);
            if (to.userData == entity) to.userData = Entity.Empty;

        }

        public void ResetUnitWalkStep(Entity entity, Pathfinding.GraphNode from) {

            this.SetUnitWalkStep(entity, from, true);
            if (from.userData == entity) from.userData = Entity.Empty;

        }

        public void SetUnitWalkStep(Entity entity, Pathfinding.GraphNode from, Pathfinding.GraphNode to) {

            this.SetUnitWalkStep(entity, from, true);
            this.SetUnitWalkStep(entity, to, false);

        }

        public void SetUnitWalkStep(Entity entity, Pathfinding.GraphNode to, bool state) {

            if (to.userData == entity || to.userData == Entity.Empty) {
                
                if (state == true) to.Walkable = true;
                
                //to.Walkable = state;
                //to.Penalty = (state == true ? 0u : 2000u);
                if (state == false) {

                    to.userData = entity;
                    
                } else {

                    to.userData = Entity.Empty;
                
                }

            }

        }

        private bool OnTargetReached(ref Vector2 from, Entity entity, PathfindingPathComponent comp, bool checkLastPoint) {
            
            var current = comp.nodes[comp.index];
            var resultNode = comp.nodes[comp.nodes.Count - 1];
            var result = (Vector3)resultNode.position;
            this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(entity);
            var idle = this.world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
            idle.idleNode = resultNode;
            if (idle.idleNode.userData == Entity.Empty || idle.idleNode.userData == entity) {

                idle.idleNode.userData = entity;

            }
            
            this.ResetUnitWalkStep(entity, current, resultNode);
            if (checkLastPoint == true) {

                if (resultNode.userData == Entity.Empty || resultNode.userData == entity) {

                    resultNode.userData = entity;
                    resultNode.Walkable = false;
                    
                }
                        
            } else {

                if (resultNode.userData == Entity.Empty || resultNode.userData == entity) {

                    resultNode.userData = entity;
                    resultNode.Walkable = true;

                }

            }
                    
            this.world.RemoveComponents<UnitEntity, PathfindingPathComponent>(entity);
                    
            from = result.XY();
            return true;

        }

        public bool MoveTowards(Entity entity,
                                ref UnityEngine.Vector2 from,
                                ref UnityEngine.Vector2 to,
                                float movementDelta,
                                float deltaTime,
                                bool checkLastPoint = false,
                                bool addLastNode = false) {

            return this.MoveTowards(entity, ref from, ref to, movementDelta, deltaTime, out _, checkLastPoint, addLastNode);

        }

        public bool MoveTowards(Entity entity,
                                ref UnityEngine.Vector2 from,
                                ref UnityEngine.Vector2 to,
                                float movementDelta,
                                float deltaTime,
                                out bool failed,
                                bool checkLastPoint = false,
                                bool addLastNode = false) {

            failed = false;
            
            if (this.world.HasComponent<UnitEntity, PathfindingPathComponent>(entity) == true) {

                var comp = this.world.GetComponent<UnitEntity, PathfindingPathComponent>(entity);
                //var skipCheck = comp.skipEntityCheck;
                if (comp.index < comp.nodes.Count - 1) {

                    var target = comp.nodes[comp.index + 1];
                    if (addLastNode == false || comp.index + 1 < comp.nodes.Count - 1) {

                        if (target.Walkable == false && target.userData != entity) {
                            //skipCheck != target.userData && (target.Walkable == false || (target.userData != Entity.Empty && target.userData != entity))) {

                            comp.waitingTimer += deltaTime;
                            if (comp.waitingTimer >= 0f && comp.waitingTimer < 0.1f) {

                                return false;

                            }

                            // target node is closed, waiting for this node will empty
                            /*var repath = false;
                            comp.waitingTimer += deltaTime;
                            if (comp.waitingTimer >= 0.05f && comp.waitingTimer < 0.5f) {

                                if (this.world.HasComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(target.userData) == true) {

                                    var targetPath = this.world.GetComponent<UnitEntity, PathfindingPathComponent>(target.userData);
                                    if (targetPath != null && targetPath.index < targetPath.nodes.Count - 1) {
                                        
                                        var targetNode = targetPath.nodes[targetPath.index + 1];
                                        if (targetNode == current) {
                                            
                                            // Target unit moving to my current node - skip next target check
                                            comp.skipEntityCheck = target.userData;
                                            return from;

                                        } else {
                                            
                                            // target unit has different current->target than current unit - just waiting while the node will be empty

                                        }

                                    } else {
                                        
                                        // target char has no path calculated yet (just waiting for) - forcing re-path
                                        this.ResetUnitWalkStep(entity, current, target);
                                        repath = true;

                                    }

                                    // Just wait for a while

                                } else {
                                    
                                    // Target is not a char or char is not moving - waiting for re-path
                                    repath = true;

                                }

                            } else if (comp.waitingTimer >= 2f) {

                                // time is up - reset
                                this.ResetUnitWalkStep(entity, current, target);
                                repath = true;
                                
                            }

                            this.StopMovement(entity, repath);
                            return from;*/
                            //this.world.RemoveComponents<UnitEntity, PathfindingPathComponent>(entity);
                            this.StopMovement(entity, repath: true);
                            this.world.RemoveComponents<UnitEntity, PathfindingPathComponent>(entity);
                            return this.MoveTowards(entity, ref from, ref to, movementDelta, deltaTime, checkLastPoint, addLastNode);

                        }

                    }

                    comp.waitingTimer = 0f;

                    //var posFrom = ((Vector3)current.position).XY();
                    var posTo = ((Vector3)target.position).XY();

                    /*if (target.userData == Entity.Empty || target.userData == entity) {

                        target.userData = entity;

                    }*/

                    var nextStep = UnityEngine.Vector2.MoveTowards(from, posTo, movementDelta);
                    if ((nextStep - posTo).sqrMagnitude <= 0.001f) {

                        comp.skipEntityCheck = Entity.Empty;
                        if (this.world.HasComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterStopState>(entity) == true) {

                            this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(entity);
                            this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterStopState>(entity);
                            var idleComp = this.world.AddComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                            idleComp.idleNode = target;
                            this.world.RemoveComponents<UnitEntity, PathfindingPathComponent>(entity);
                            from = posTo;
                            return false;

                        }

                        ++comp.index;
                        
                        if (comp.index < comp.nodes.Count - 1) {

                            this.SetUnitWalkStep(entity, target, comp.nodes[comp.index + 1]);

                        } else {

                            return this.OnTargetReached(ref from, entity, comp, checkLastPoint);

                        }

                    }

                    var idle = this.world.GetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                    if (idle != null && (idle.idleNode.userData == Entity.Empty || idle.idleNode.userData == entity)) {

                        this.ResetUnitWalkStep(entity, idle.idleNode);
                        idle.idleNode.Walkable = true;
                        idle.idleNode.userData = Entity.Empty;

                    }

                    this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                    this.world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(entity);

                    from = nextStep;
                    return false;

                } else {

                    return this.OnTargetReached(ref from, entity, comp, checkLastPoint);
                    
                }

            } else {

                var prevFrom = from;
                if (this.IsPathExists(ref from, to) == false) {

                    failed = true;
                    return false;
                    
                }
                
                var result = PoolList<Pathfinding.GraphNode>.Spawn(10);
                
                var mapFeature = this.world.GetFeature<MapFeature>();
                var mapFrom = mapFeature.GetMapPositionFromWorld(from);
                var mapTo = mapFeature.GetMapPositionFromWorld(to);
                var pathState = this.GetPath(result, mapFrom, mapTo);
                
                if (pathState != Pathfinding.PathCompleteState.Complete || (checkLastPoint == true && this.IsPathValid(result, from, to) == false)) {
                    
                    PoolList<Pathfinding.GraphNode>.Recycle(ref result);
                    from = prevFrom;
                    failed = true;
                    return false;

                }

                if (prevFrom != from) {

                    var startNodeInfo = AstarPath.active.GetNearest(prevFrom);
                    if (result[0] != startNodeInfo.node) result.Insert(0, startNodeInfo.node);

                }

                if (addLastNode == true) {

                    var lastNode = AstarPath.active.GetNearest(to.XY());
                    if (lastNode.node != result[result.Count - 1]) result.Add(lastNode.node);

                }

                to = ((Vector3)result[result.Count - 1].position).XY();
                var comp = this.world.AddComponent<UnitEntity, PathfindingPathComponent>(entity);
                comp.index = 0;
                comp.nodes = result;
                comp.@from = prevFrom;
                comp.to = to;
                
                var idle = this.world.GetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                if (idle != null && idle.idleNode != null && (idle.idleNode.userData == Entity.Empty || idle.idleNode.userData == entity)) {

                    this.ResetUnitWalkStep(entity, idle.idleNode);
                    idle.idleNode.Walkable = true;
                    idle.idleNode.userData = Entity.Empty;

                }

                this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterIdleState>(entity);
                this.world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterMoveState>(entity);

                from = prevFrom;
                return false;

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

            var node = AstarPath.active.GetNearest(from.XY()).node;
            return this.IsPathPossible(ref node, AstarPath.active.GetNearest(to.XY()).node);
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

        public bool IsPathExists(ref Vector2 from, Vector2 to) {

            var node = AstarPath.active.GetNearest(from.XY()).node;
            var result = this.IsPathPossible(ref node, AstarPath.active.GetNearest(to.XY()).node);
            from = ((Vector3)node.position).XY();
            return result;

        }

        private bool IsPathPossible(ref Pathfinding.GraphNode startNode, Pathfinding.GraphNode endNode) {
            
            var hasPath = Pathfinding.PathUtilities.IsPathPossible(startNode, endNode);
            if (hasPath == false) {

                var endNodeGn = endNode as Pathfinding.GridNode;
                if (endNodeGn.EdgeNode == true) return false;

                var startNodeGn = startNode as Pathfinding.GridNode;
                var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
                if (startNodeGn.Walkable == false) {

                    var dist = float.MaxValue;
                    var found = false;
                    for (int i = 0; i < 8; ++i) {

                        var idx = startNodeGn.NodeInGridIndex + graph.neighbourOffsets[i];
                        if (idx < 0 || idx >= graph.nodes.Length) continue;
                        var conn = graph.nodes[idx];
                        if (conn == null || conn.Walkable == false) continue;

                        var d = (conn.position - endNode.position).sqrMagnitude;
                        if (d < dist) {

                            found = true;
                            if (Pathfinding.PathUtilities.IsPathPossible(conn, endNode) == true) {

                                startNode = conn;
                                dist = d;

                            }

                        }

                    }

                    if (found == false) {
                        
                        // all neighbours are non-walkable - return true
                        return true;

                    }

                }

                for (int i = 0; i < 8; ++i) {

                    var idx = endNodeGn.NodeInGridIndex + graph.neighbourOffsets[i];
                    if (idx < 0 || idx >= graph.nodes.Length) continue;
                    var conn = graph.nodes[idx];
                    if (conn == null) continue;
                    
                    if (Pathfinding.PathUtilities.IsPathPossible(startNode, conn) == true) return true;

                }

            }

            return hasPath;

        }

        private Pathfinding.PathCompleteState GetPath(System.Collections.Generic.List<Pathfinding.GraphNode> result, UnityEngine.Vector2Int from, UnityEngine.Vector2Int to) {

            var mapFeature = this.world.GetFeature<MapFeature>();
            var worldFrom = mapFeature.GetWorldPositionFromMap(from).XY();
            var worldTo = mapFeature.GetWorldPositionFromMap(to).XY();
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