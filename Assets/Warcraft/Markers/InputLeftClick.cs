using ME.ECS;

namespace Warcraft.Markers {
    
    public struct InputLeftClick : IMarker {

        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2 worldPosition;

    }

    public struct InputRightClick : IMarker {

        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2 worldPosition;

    }

}