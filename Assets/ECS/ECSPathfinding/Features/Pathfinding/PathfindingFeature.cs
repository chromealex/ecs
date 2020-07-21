
namespace ME.ECS.ECSPathfinding.Features {

    using ME.ECS.Pathfinding;
    using Pathfinding.Components; using Pathfinding.Modules; using Pathfinding.Systems; using Pathfinding.Markers;
    
    namespace Pathfinding.Components {}
    namespace Pathfinding.Modules {}
    namespace Pathfinding.Systems {}
    namespace Pathfinding.Markers {}
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class PathfindingFeature : Feature {

        private ME.ECS.Pathfinding.Pathfinding pathfinding;
        
        internal ME.ECS.Pathfinding.Pathfinding GetInstance() {

            return this.pathfinding;

        }
        
        public void SetInstance(ME.ECS.Pathfinding.Pathfinding pathfinding) {

            this.pathfinding = pathfinding;
            
        }
        
        protected override void OnConstruct() {

            PathfindingComponentsInitializer.Init(ref this.world.GetStructComponents());
            ComponentsInitializerWorld.Register(PathfindingComponentsInitializer.InitEntity);
            
            var entity = new Entity("Pathfinding");
            entity.SetData(new IsPathfinding());
            
            this.AddSystem<SetPathfindingInstanceSystem>();
            this.AddSystem<BuildGraphsSystem>();
            this.AddSystem<BuildPathSystem>();

        }

        protected override void OnDeconstruct() {
            
            ComponentsInitializerWorld.UnRegister(PathfindingComponentsInitializer.InitEntity);
            
        }

        public void GetNodesInBounds(System.Collections.Generic.List<Node> output, UnityEngine.Bounds bounds) {
         
            this.pathfinding.GetNodesInBounds(output, bounds);
            
        }

        public bool BuildNodePhysics(Node node) {

            return this.pathfinding.BuildNodePhysics(node);

        }
        
        public T GetGraphByIndex<T>(int index) where T : Graph {

            return this.pathfinding.graphs[index] as T;

        }
        
        public Node GetNearest(UnityEngine.Vector3 worldPosition) {

            return this.pathfinding.GetNearest(worldPosition, Constraint.Default);

        }

        public Node GetNearest(UnityEngine.Vector3 worldPosition, Constraint constraint) {
            
            return this.pathfinding.GetNearest(worldPosition, constraint);
            
        }

    }

}