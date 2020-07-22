
namespace ME.ECS.Pathfinding.Features.Pathfinding.Components {

    public struct IsPathfinding : IStructComponent {}
    public struct BuildAllGraphs : IStructComponent {}
    public struct HasPathfindingInstance : IStructComponent {}
    
    public struct CalculatePath : IStructComponent {

        public UnityEngine.Vector3 from;
        public UnityEngine.Vector3 to;
        public ME.ECS.Pathfinding.Constraint constraint;

    }

    public struct IsPathBuilt : IStructComponent {}

    public class Path : IComponentCopyable {

        public ME.ECS.Pathfinding.PathCompleteState result;
        public ME.ECS.Collections.BufferArray<UnityEngine.Vector3> path;
        public ME.ECS.Collections.BufferArray<ME.ECS.Pathfinding.Node> nodes;

        public void CopyFrom(IComponentCopyable other) {

            var _other = (Path)other;
            this.result = _other.result;
            ArrayUtils.Copy(in _other.path, ref this.path); 
            ArrayUtils.Copy(in _other.nodes, ref this.nodes); 

        }

        void IPoolableRecycle.OnRecycle() {

            this.result = default;
            PoolArray<UnityEngine.Vector3>.Recycle(ref this.path);
            PoolArray<ME.ECS.Pathfinding.Node>.Recycle(ref this.nodes);
        
        }

    }

    public class PathfindingInstance : IComponentCopyable {

        public ME.ECS.Pathfinding.Pathfinding pathfinding;
        
        public void CopyFrom(IComponentCopyable other) {

            var _other = (PathfindingInstance)other;
            if (this.pathfinding == null && _other.pathfinding == null) {

                return;

            }
            
            if (this.pathfinding == null && _other.pathfinding != null) {
                
                this.pathfinding = _other.pathfinding.Clone();
                
            } else {

                this.pathfinding.CopyFrom(_other.pathfinding); 

            }
            
        }

        void IPoolableRecycle.OnRecycle() {

            if (this.pathfinding != null) this.pathfinding.Recycle();
            this.pathfinding = null;

        }

    }

}