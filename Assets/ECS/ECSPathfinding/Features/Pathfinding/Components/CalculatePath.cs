
namespace ME.ECS.ECSPathfinding.Features.Pathfinding.Components {

    public struct IsPathfinding : IStructComponent {}
    public struct BuildAllGraphs : IStructComponent {}
    public struct HasPathfindingInstance : IStructComponent {}
    
    public struct CalculatePath : IStructComponent {

        public UnityEngine.Vector3 from;
        public UnityEngine.Vector3 to;
        public ME.ECS.Pathfinding.Constraint constraint;

    }

    public struct IsPathBuilt : IStructComponent {}

    public struct PathIndex : IStructComponent {

        public int value;

    }
    
    public class Path : IComponentCopyable {
    
        public ME.ECS.Collections.BufferArray<UnityEngine.Vector3> path;
        public ME.ECS.Collections.BufferArray<ME.ECS.Pathfinding.Node> nodes;

        public void CopyFrom(IComponentCopyable other) {

            var _other = (Path)other;
            ArrayUtils.Copy(in _other.path, ref this.path); 
            ArrayUtils.Copy(in _other.nodes, ref this.nodes); 

        }

        void IPoolableRecycle.OnRecycle() {
        
            PoolArray<UnityEngine.Vector3>.Recycle(ref this.path);
            PoolArray<ME.ECS.Pathfinding.Node>.Recycle(ref this.nodes);
        
        }

    }

}