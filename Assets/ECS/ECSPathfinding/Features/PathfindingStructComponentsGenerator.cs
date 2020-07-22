
namespace ME.ECS {

    public static class PathfindingComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.Pathfinding.Features.Pathfinding.Components.CalculatePath>();
            structComponentsContainer.Validate<ME.ECS.Pathfinding.Features.Pathfinding.Components.IsPathfinding>(true);
            structComponentsContainer.Validate<ME.ECS.Pathfinding.Features.Pathfinding.Components.BuildAllGraphs>(true);
            structComponentsContainer.Validate<ME.ECS.Pathfinding.Features.Pathfinding.Components.HasPathfindingInstance>(true);
            structComponentsContainer.Validate<ME.ECS.Pathfinding.Features.Pathfinding.Components.IsPathBuilt>(true);

        }

        public static void InitEntity(Entity entity) {

            entity.ValidateData<ME.ECS.Pathfinding.Features.Pathfinding.Components.CalculatePath>();
            entity.ValidateData<ME.ECS.Pathfinding.Features.Pathfinding.Components.IsPathfinding>(true);
            entity.ValidateData<ME.ECS.Pathfinding.Features.Pathfinding.Components.BuildAllGraphs>(true);
            entity.ValidateData<ME.ECS.Pathfinding.Features.Pathfinding.Components.HasPathfindingInstance>(true);
            entity.ValidateData<ME.ECS.Pathfinding.Features.Pathfinding.Components.IsPathBuilt>();
            
        }

    }

}
