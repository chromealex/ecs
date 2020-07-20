namespace ME.ECS.Pathfinding {
    
    public interface IPathModifier {

        Path Run(Path path, Constraint constraint);

    }

    public abstract class PathModifierSeeker : UnityEngine.MonoBehaviour {

        public abstract TMod GetModifier<TMod>() where TMod : IPathModifier;

    }

    public struct PathModifierEmpty : IPathModifier {

        public Path Run(Path path, Constraint constraint) {

            return path;

        }

    }

}