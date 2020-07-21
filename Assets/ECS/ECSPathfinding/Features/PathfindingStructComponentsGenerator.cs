
namespace ME.ECS {

    public static class PathfindingComponentsInitializer {
    
        public static void Init(ref ME.ECS.StructComponentsContainer structComponentsContainer) {
    
            structComponentsContainer.Validate<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.CalculatePath>();
            structComponentsContainer.Validate<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.IsPathfinding>();
            structComponentsContainer.Validate<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.PathIndex>();
            structComponentsContainer.Validate<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.BuildAllGraphs>();
            structComponentsContainer.Validate<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.HasPathfindingInstance>();
            structComponentsContainer.Validate<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.IsPathBuilt>();

        }

        public static void InitEntity(Entity entity) {

            entity.ValidateData<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.CalculatePath>();
            entity.ValidateData<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.IsPathfinding>();
            entity.ValidateData<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.PathIndex>();
            entity.ValidateData<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.BuildAllGraphs>();
            entity.ValidateData<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.HasPathfindingInstance>();
            entity.ValidateData<ME.ECS.ECSPathfinding.Features.Pathfinding.Components.IsPathBuilt>();
            
        }

    }

}
