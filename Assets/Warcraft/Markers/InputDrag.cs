using ME.ECS;

namespace Warcraft.Markers {
    
    public struct InputDragBegin : IMarker {
        
        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2 worldPosition;

    }
    
    public struct InputDragMove : IMarker {
        
        public UnityEngine.Vector2 beginPosition;
        public UnityEngine.Vector2 beginWorldPosition;
        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2 worldPosition;

    }
    
    public struct InputDragEnd : IMarker {
        
        public UnityEngine.Vector2 beginPosition;
        public UnityEngine.Vector2 beginWorldPosition;
        public UnityEngine.Vector2 position;
        public UnityEngine.Vector2 worldPosition;

    }
    
}