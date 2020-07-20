### Pathfinding package

Very fast deterministic pathfinding for ME.ECS with per-path custom agent size support.
> It is a part of ME.ECS (pools and math used)

#### Features:
* 3D Grid Graph (each node has 18 connections)
* Tags
* Areas
* Penalty
* Path modifier which allows you to build direct paths without node-based paths
* Graph modifiers to build carves, change tags for nodes at runtime

#### Quick start

##### Scene setup

1. Create an GameObject on the scene with Pathfinding component.
2. Create child GameObject with GridGraph component.
3. Add created graph to Pathfinding.
4. Set up GridGraph on Pathfinding component (grid section).
5. Press "Build" button on certain graph or "Build All" at the bottom of Pathfinding component.
6. Now you are ready to build a path.

##### Building path

```csharp
// Get current active Pathfinding component
var active = ME.ECS.Pathfinding.Pathfinding.active;

// Set up path constrains
var constraint = ME.ECS.Pathfinding.Constraint.Default;
constraint.checkWalkability = true; // we are going to check node's walkability (default value is true)
constraint.walkable = true; // we are interested in walkable nodes only (default value is true)

constraint.checkArea = true;
constraint.areaMask = -1; // we are going to move on any area (area is the block bordered with unwalkable nodes)

constraint.checkTags = true;
constraint.tagsMask = -1; // we are going to move through any tags (by default this value is -1, so you can set checkTags = false)

constraint.agentSize = new UnityEngine.Vector3(agentSizeX, agentSizeY, agentSizeZ);
constraint.graphMask = (1 << 0); // we are going to find path on the first graph only (by default this value is -1 (any), so you can leave it -1)

// Build a path with custom path modifier
// PathCornersModifier could build direct path and cut corners, so your path will be more user-friendly.
var pathModifier = new ME.ECS.Pathfinding.PathCornersModifier();
// You can use path modifier or don't pass this parameter: CalculatePath(p1, p2, constrant)
var path = active.CalculatePath(fromPosition, toPosition, constraint, pathModifier);

if (path.result == ME.ECS.Pathfinding.PathCompleteState.Complete) {

  // Path found
  // Now you have 2 node lists: path.nodes and path.nodesModified
  // If you use any path modifier you have got nodesModified list filled with new nodes and if not this list will be null

} else {
  
  // Path not found (this means you are trying to search nodes with wrong constrains)
  // In general all paths are possible because Pathfinding try to search nearest suitable nodes instead of returns NotExist state. But if you pass wrong constrants and no node will suitable for this - so the path couldn't been calculated.
  
}

// Do not forget to recycle path data when you grab it
path.Recycle();
