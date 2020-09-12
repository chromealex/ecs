using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS.Pathfinding {

    using ME.ECS.Collections;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class GridGraph : Graph {
        
        public enum Direction : byte {

            Up = 0,
            Down = 1,
            Forward = 2,
            Right = 3,
            Backward = 4,
            Left = 5,
            
            RightForward = 6,
            RightBackward = 7,
            LeftBackward = 8,
            LeftForward = 9,
            
            RightUpForward = 10,
            RightUpBackward = 11,
            LeftUpBackward = 12,
            LeftUpForward = 13,
            
            RightDownForward = 14,
            RightDownBackward = 15,
            LeftDownBackward = 16,
            LeftDownForward = 17,
            
        }

        public enum DrawMode : byte {

            None = 0,
            Solid,
            Areas,
            Penalty,
            Tags,

        }

        public enum ConnectionsType : byte {

            All = 0,
            NoDirectional,
            DirectionalOnly,
            DirectionalIfHasDirect,

        }
        
        public Vector3Int size = new Vector3Int(100, 100, 100);
        public float nodeSize = 1f;
        
        public float initialPenalty = 100f;
        public float diagonalCostFactor = 0.9f;
        public ConnectionsType connectionsType = ConnectionsType.All;

        public float agentHeight;
        
        public LayerMask checkMask;
        public LayerMask collisionMask;
        public float collisionCheckRadius;

        public DrawMode drawMode;
        public bool drawNonwalkableNodes;
        public bool drawConnections;
        public bool drawConnectionsToUnwalkable;
        
        private struct CopyNode : IArrayElementCopy<Node> {

            public void Copy(Node from, ref Node to) {

                if (to == null) to = PoolClass<GridNode>.Spawn();
                to.CopyFrom(from);

            }

            public void Recycle(Node item) {

                var g = (GridNode)item;
                PoolClass<GridNode>.Recycle(ref g);
                
            }

        }

        public override void OnRecycle() {

            this.size = default;
            this.nodeSize = default;
            this.initialPenalty = default;
            this.diagonalCostFactor = default;
            this.connectionsType = default;
            this.agentHeight = default;
            this.checkMask = default;
            this.collisionMask = default;
            this.collisionCheckRadius = default;

            if (this.nodes != null) {

                Debug.Log("Recycle: " + this.nodes.Count);
                for (int i = 0; i < this.nodes.Count; ++i) {

                    var node = (GridNode)this.nodes[i];
                    PoolClass<GridNode>.Recycle(ref node);

                }

                this.nodes.Clear();

            }

        }

        public override void Recycle() {
            
            this.OnRecycle();

        }

        public override void OnCopyFrom(Graph other) {
            
            var gg = (GridGraph)other;
            this.size = gg.size;
            this.nodeSize = gg.nodeSize;
            this.initialPenalty = gg.initialPenalty;
            this.diagonalCostFactor = gg.diagonalCostFactor;
            this.connectionsType = gg.connectionsType;
            this.agentHeight = gg.agentHeight;
            this.checkMask = gg.checkMask;
            this.collisionMask = gg.collisionMask;
            this.collisionCheckRadius = gg.collisionCheckRadius;
            
            ArrayUtils.Copy(other.nodes, ref this.nodes, new CopyNode());
            
        }

        public override void RemoveNode(ref Node node, bool bruteForceConnections = false) {

            base.RemoveNode(ref node, bruteForceConnections);
            
            var g = (GridNode)node;
            PoolClass<GridNode>.Recycle(ref g);
            node = null;

        }

        public bool HasConnectionByDirection(int sourceIndex, Direction direction, bool walkableOnly = true) {
            
            var node = this.GetNodeByIndex<GridNode>(sourceIndex);
            var conn = node.connections[(int)direction];
            var idx = conn.index;
            if (idx >= 0) {

                if (walkableOnly == true) {

                    return this.nodes[idx].walkable == true;

                }

                return true;

            }

            return false;

        }

        public void ResetConnections(int sourceIndex) {
            
            var connection = Node.Connection.NoConnection;
            var node = this.GetNodeByIndex<GridNode>(sourceIndex);
            for (int i = 0; i < node.connections.Length; ++i) {

                node.connections[i] = connection;

            }
            
        }
        
        public void SetupConnectionByDirection(int sourceIndex, Direction direction) {

            var connection = Node.Connection.NoConnection;
            var node = this.GetNodeByIndex<GridNode>(sourceIndex);
            var target = GridGraphUtilities.GetIndexByDirection(this, sourceIndex, direction);
            if (target >= 0) {
            
                var targetNode = this.GetNodeByIndex<GridNode>(target);
                var cost = (node.worldPosition - targetNode.worldPosition).sqrMagnitude;
                connection.cost = cost * (GridGraphUtilities.IsDiagonalDirection(direction) == true ? this.diagonalCostFactor : 1f) + targetNode.penalty;
                connection.index = target;
                
            }

            node.connections[(int)direction] = connection;

        }

        public override Node GetNearest(Vector3 worldPosition, Constraint constraint) {

            return this.GetNearest<GridNode>(worldPosition, constraint);

        }

        public override T GetNearest<T>(Vector3 worldPosition, Constraint constraint) {

            if (this.nodes == null) return default;
            
            var clamped = new Vector3(
                Mathf.Clamp(worldPosition.x - this.graphCenter.x, -this.nodeSize * this.size.x * 0.5f + this.nodeSize * 0.5f, this.nodeSize * this.size.x * 0.5f - this.nodeSize * 0.5f),
                Mathf.Clamp(worldPosition.y - this.graphCenter.y, -this.agentHeight * this.size.y * 0.5f, this.agentHeight * this.size.y * 0.5f),
                Mathf.Clamp(worldPosition.z - this.graphCenter.z, -this.nodeSize * this.size.z * 0.5f + this.nodeSize * 0.5f, this.nodeSize * this.size.z * 0.5f - this.nodeSize * 0.5f));

            var x = (int)((clamped.x + this.nodeSize * this.size.x * 0.5f) / this.nodeSize);
            var y = (int)((clamped.y + this.agentHeight * this.size.y * 0.5f) / this.agentHeight);
            var z = (int)((clamped.z + this.nodeSize * this.size.z * 0.5f) / this.nodeSize);

            for (int idx = 0; idx < this.nodes.Count; ++idx) {

                var p = ME.ECS.MathUtils.GetSpiralPointByIndex(new Vector2Int(x, z), idx);
                var i = GridGraphUtilities.GetIndexByPosition(this, new Vector3Int(p.x, y, p.y));
                var node = this.GetNodeByIndex<T>(i);
                if (node == null) continue;
                if (node.IsSuitable(constraint) == true) return node;

            }

            return default;

        }

        public override void GetNodesInBounds(ListCopyable<Node> result, Bounds bounds) {

            var min = bounds.min;
            var max = bounds.max;

            var minNode = this.GetNearest<GridNode>(min, Constraint.Empty);
            if (minNode == null) return;
            var maxNode = this.GetNearest<GridNode>(max, Constraint.Empty);
            if (maxNode == null) return;
            
            for (int y = minNode.position.y; y <= maxNode.position.y; ++y) {

                for (int x = minNode.position.x; x <= maxNode.position.x; ++x) {

                    for (int z = minNode.position.z; z <= maxNode.position.z; ++z) {

                        var index = GridGraphUtilities.GetIndexByPosition(this, new Vector3Int(z, y, x));
                        var n = this.nodes[index];
                        if (bounds.Contains(n.worldPosition) == true) {

                            result.Add(n);

                        }

                    }

                }

            }

        }

        protected override void DrawGizmos() {

            if (this.nodes == null) return;
            
            var center = this.graphCenter;
            
            var borderColor = new Color(1f, 1f, 1f, 1f);
            Gizmos.color = borderColor;
            Gizmos.DrawWireCube(center, new Vector3(this.size.x * this.nodeSize, this.size.y * this.agentHeight, this.size.z * this.nodeSize));

            if (this.drawMode != DrawMode.None) {

                var nodeBorderColor = new Color(0.2f, 0.5f, 1f, 0.2f);
                var nodeColor = new Color(0.2f, 0.5f, 1f, 0.2f);
                var nodeBorderColorWalkableWorld = new Color(0.2f, 0.5f, 1f, 0.6f);
                var nodeColorWalkableWorld = new Color(0.2f, 0.5f, 1f, 0.6f);
                var nodeBorderColorUnwalkable = new Color(1f, 0.2f, 0.2f, 0.4f);
                var nodeColorUnwalkable = new Color(1f, 0.2f, 0.2f, 0.4f);
                for (int j = 0; j < this.nodes.Count; ++j) {

                    var node = (GridNode)this.nodes[j];
                    var x = node.position.z;
                    var y = node.position.y;
                    var z = node.position.x;
                    //var nodePosition = new Vector3(x * this.nodeSize + this.nodeSize * 0.5f, y * this.agentHeight + this.agentHeight * 0.5f, z * this.nodeSize + this.nodeSize * 0.5f);
                    var worldPos = node.worldPosition; //center + nodePosition;

                    if (this.drawMode == DrawMode.Solid) { } else if (this.drawMode == DrawMode.Areas) {

                        nodeColor = this.GetAreaColor(node.area);
                        nodeBorderColor = nodeColor;
                        nodeBorderColor.a = 0.6f;

                        nodeColorWalkableWorld = nodeColor;
                        nodeBorderColorWalkableWorld = nodeBorderColor;

                    } else if (this.drawMode == DrawMode.Penalty) {

                        nodeColor = this.GetPenaltyColor(node.penalty);
                        nodeBorderColor = nodeColor;
                        nodeBorderColor.a = 0.6f;

                        nodeColorWalkableWorld = nodeColor;
                        nodeBorderColorWalkableWorld = nodeBorderColor;

                    } else if (this.drawMode == DrawMode.Tags) {

                        nodeColor = this.GetAreaColor(node.tag);
                        nodeBorderColor = nodeColor;
                        nodeBorderColor.a = 0.6f;

                        nodeColorWalkableWorld = nodeColor;
                        nodeBorderColorWalkableWorld = nodeBorderColor;

                    }

                    if (node.walkable == true || this.drawNonwalkableNodes == true) {

                        Gizmos.color = (node.walkable == true ? nodeColor : nodeColorUnwalkable);
                        Gizmos.DrawCube(worldPos, new Vector3(this.nodeSize, this.agentHeight, this.nodeSize));

                        Gizmos.color = (node.walkable == true ? nodeBorderColor : nodeBorderColorUnwalkable);
                        Gizmos.DrawWireCube(worldPos, new Vector3(this.nodeSize, this.agentHeight, this.nodeSize));

                    }

                    if (node.walkable == true) {

                        Gizmos.color = nodeColorWalkableWorld;
                        Gizmos.DrawCube(worldPos, new Vector3(0.8f, 0f, 0.8f) * this.nodeSize);

                        Gizmos.color = nodeBorderColorWalkableWorld;
                        Gizmos.DrawWireCube(worldPos, new Vector3(0.8f, 0f, 0.8f) * this.nodeSize);

                    }


                    if (this.drawConnections == true) { // Draw connections

                        if (node.walkable == true) {

                            Gizmos.color = new Color(1f, 0.9215686f, 0.01568628f, 0.2f);
                            for (int k = 0; k < node.connections.Length; ++k) {

                                var conn = node.connections[k];
                                var n = this.GetNodeByIndex<GridNode>(conn.index);
                                if (n != null && (this.drawConnectionsToUnwalkable == true || n.walkable == true)) {

                                    Gizmos.DrawRay(node.worldPosition + Vector3.up * 0.1f, (n.worldPosition - node.worldPosition) * 0.5f + Vector3.up * 0.1f);
                                    Gizmos.DrawRay(node.worldPosition, (n.worldPosition - node.worldPosition) * 0.5f);

                                }

                            }

                        }

                    }

                }

            }

        }
        
        protected override void Validate() {

            if (this.size.x <= 0) this.size.x = 1;
            if (this.size.y <= 0) this.size.y = 1;
            if (this.size.z <= 0) this.size.z = 1;

        }

        protected override void BuildConnections() {

            var noConnection = Node.Connection.NoConnection;
            for (var i = 0; i < this.nodes.Count; ++i) {

                //if (i != this.size.x && i != 0 && i != this.nodes.Length - 1 && i != this.size.x - 1 && i != (50 + this.size.x * this.size.z) && i != (150 + this.size.x * this.size.z * 2) &&
                //    i != this.size.z * this.size.x) continue;
                
                var node = this.nodes[i];
                var connections = node.GetConnections();
                this.ResetConnections(node.index);

                if (this.connectionsType != ConnectionsType.DirectionalOnly) {

                    this.SetupConnectionByDirection(node.index, Direction.Up);
                    this.SetupConnectionByDirection(node.index, Direction.Down);
                    this.SetupConnectionByDirection(node.index, Direction.Forward);
                    this.SetupConnectionByDirection(node.index, Direction.Right);
                    this.SetupConnectionByDirection(node.index, Direction.Backward);
                    this.SetupConnectionByDirection(node.index, Direction.Left);

                } else {

                    connections[0] = connections[1] = connections[2] = connections[3] = connections[4] = connections[5] = noConnection;

                }

                if (this.connectionsType == ConnectionsType.All || this.connectionsType == ConnectionsType.DirectionalOnly) {

                    this.SetupConnectionByDirection(node.index, Direction.RightForward);
                    this.SetupConnectionByDirection(node.index, Direction.RightBackward);
                    this.SetupConnectionByDirection(node.index, Direction.LeftBackward);
                    this.SetupConnectionByDirection(node.index, Direction.LeftForward);

                    this.SetupConnectionByDirection(node.index, Direction.RightUpForward);
                    this.SetupConnectionByDirection(node.index, Direction.RightUpBackward);
                    this.SetupConnectionByDirection(node.index, Direction.LeftUpBackward);
                    this.SetupConnectionByDirection(node.index, Direction.LeftUpForward);

                    this.SetupConnectionByDirection(node.index, Direction.RightDownForward);
                    this.SetupConnectionByDirection(node.index, Direction.RightDownBackward);
                    this.SetupConnectionByDirection(node.index, Direction.LeftDownBackward);
                    this.SetupConnectionByDirection(node.index, Direction.LeftDownForward);

                } else if (this.connectionsType == ConnectionsType.DirectionalIfHasDirect) {

                    if (this.HasConnectionByDirection(node.index, Direction.Forward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Right) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.RightForward);

                    }
                    
                    if (this.HasConnectionByDirection(node.index, Direction.Backward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Right) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.RightBackward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Forward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Left) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.LeftForward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Backward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Left) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.LeftBackward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Backward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Right) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Down) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.RightDownBackward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Backward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Left) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Down) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.LeftDownBackward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Forward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Right) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Down) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.RightDownForward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Forward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Left) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Down) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.LeftDownForward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Backward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Right) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Up) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.RightUpBackward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Backward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Left) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Up) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.LeftUpBackward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Forward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Right) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Up) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.RightUpForward);

                    }

                    if (this.HasConnectionByDirection(node.index, Direction.Forward) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Left) == true ||
                        this.HasConnectionByDirection(node.index, Direction.Up) == true) {

                        this.SetupConnectionByDirection(node.index, Direction.LeftUpForward);

                    }

                }

            }

        }

        protected override void RunModifiersAfterConnections() {
            
            for (var i = 0; i < this.modifiers.Count; ++i) {
                
                if (this.modifiers[i].enabled == true) this.modifiers[i].modifier.ApplyAfterConnections(this);
                
            }

        }

        protected override void RunModifiersBeforeConnections() {
            
            for (var i = 0; i < this.modifiers.Count; ++i) {
                
                if (this.modifiers[i].enabled == true) this.modifiers[i].modifier.ApplyBeforeConnections(this);
                
            }

        }

        public override bool BuildNodePhysics(Node node) {
             
            var worldPos = node.worldPosition;

            if (this.checkMask == 0) {

                node.worldPosition = worldPos;
                node.walkable = true;
                return true;

            }
            
            var raycastResult = false;
            RaycastHit hit;
            if (this.collisionCheckRadius <= 0f) {

                raycastResult = Physics.Raycast(worldPos + Vector3.up * (this.agentHeight * 0.5f), Vector3.down, out hit, this.agentHeight, this.checkMask);

            } else {

                raycastResult = Physics.SphereCast(new Ray(worldPos + Vector3.up * (this.agentHeight * 0.5f - this.collisionCheckRadius), Vector3.down),
                                                   this.collisionCheckRadius, out hit, this.agentHeight - this.collisionCheckRadius * 2f, this.checkMask);

            }

            if (raycastResult == true) {

                node.worldPosition = hit.point;

                if ((this.collisionMask & (1 << hit.collider.gameObject.layer)) != 0) {

                    node.walkable = false;

                } else {

                    return true;

                }

            } else {

                node.walkable = false;

            }

            return false;

        }

        protected override void BuildNodes() {
            
            var nodes = new List<Node>(this.size.x * this.size.y * this.size.z);
            this.nodes = nodes;

            var center = this.graphCenter - new Vector3(this.size.x * this.nodeSize * 0.5f, this.size.y * this.agentHeight * 0.5f, this.size.z * this.nodeSize * 0.5f);
            
            var i = 0;
            for (int y = 0; y < this.size.y; ++y) {

                for (int x = 0; x < this.size.x; ++x) {
                
                    for (int z = 0; z < this.size.z; ++z) {

                        var nodePosition = new Vector3(x * this.nodeSize + this.nodeSize * 0.5f, y * this.agentHeight + this.agentHeight * 0.5f, z * this.nodeSize + this.nodeSize * 0.5f);
                        var worldPos = center + nodePosition;

                        var node = PoolClass<GridNode>.Spawn();
                        node.graph = this;
                        node.index = i;
                        node.position = new Vector3Int(z, y, x);
                        node.walkable = true;
                        node.worldPosition = worldPos;
                        node.penalty = this.initialPenalty;
                        this.nodes.Add(node);
                        
                        this.BuildNodePhysics(node);

                        ++i;

                    }
                    
                }
                
            }

        }

    }

    public static class GridGraphUtilities {
        
        public static void DirUp(Vector3Int size, GridNode node, ref int index) {

            if (index == -1) return;
            
            index += size.x * size.z;
            if (node.position.y >= size.y - 1) index = -1;
            
        }

        public static void DirDown(Vector3Int size, GridNode node, ref int index) {

            if (index == -1) return;
            
            index -= size.x * size.z;
            if (node.position.y <= 0) index = -1;
            
        }

        public static void DirRight(Vector3Int size, GridNode node, ref int index) {

            if (index == -1) return;
            
            index -= 1;
            if (node.position.x <= 0) index = -1;
            
        }

        public static void DirLeft(Vector3Int size, GridNode node, ref int index) {

            if (index == -1) return;
            
            index += 1;
            if (node.position.x >= size.z - 1) index = -1;
            
        }

        public static void DirForward(Vector3Int size, GridNode node, ref int index) {

            if (index == -1) return;
            
            index += size.z;
            if (node.position.z >= size.x - 1) index = -1;
            
        }

        public static void DirBackward(Vector3Int size, GridNode node, ref int index) {

            if (index == -1) return;
            
            index -= size.z;
            if (node.position.z <= 0) index = -1;
            
        }
        
        public static bool IsDiagonalDirection(GridGraph.Direction direction) {

            return (int)direction >= 6;

        }

        public static int GetIndexByDirection(GridGraph graph, int sourceIndex, GridGraph.Direction direction) {

            var node = graph.GetNodeByIndex<GridNode>(sourceIndex);
            
            switch (direction) {
                
                case GridGraph.Direction.LeftDownForward:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirDown(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.RightDownForward:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirDown(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.LeftDownBackward:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirDown(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.RightDownBackward:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirDown(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.LeftUpForward:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirUp(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.RightUpForward:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirUp(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.LeftUpBackward:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirUp(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.RightUpBackward:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirUp(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.LeftForward:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.RightForward:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.LeftBackward:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.RightBackward:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.Left:
                    GridGraphUtilities.DirLeft(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.Right:
                    GridGraphUtilities.DirRight(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.Forward:
                    GridGraphUtilities.DirForward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.Backward:
                    GridGraphUtilities.DirBackward(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.Up:
                    GridGraphUtilities.DirUp(graph.size, node, ref sourceIndex);
                    break;

                case GridGraph.Direction.Down:
                    GridGraphUtilities.DirDown(graph.size, node, ref sourceIndex);
                    break;

            }
            
            return sourceIndex;

        }

        public static int GetIndexByPosition(GridGraph graph, Vector3Int position) {

            var x = position.x;
            var y = position.y;
            var z = position.z;
            var idx = y * graph.size.x * graph.size.z + (x * graph.size.z) + z;
            return idx;

        }
        
    }

    [System.Serializable]
    public class GridNode : Node {

        public Vector3Int position;

        public readonly Connection[] connections = new Connection[6 + 4 + 4 + 4];

        public override Connection[] GetConnections() {

            return this.connections;

        }

        public override void CopyFrom(Node other) {
            
            base.CopyFrom(other);

            var g = (GridNode)other;
            this.position = g.position;
            for (int i = 0; i < this.connections.Length; ++i) {

                this.connections[i] = g.connections[i];

            }

        }

    }
    
}
