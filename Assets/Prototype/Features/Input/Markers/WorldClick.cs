using ME.ECS;

namespace Prototype.Features.Input.Markers {
    
    public struct WorldClick : IMarker {

        public UnityEngine.Vector3 worldPos;

    }

    public struct WorldDragBegin : IMarker {

        public UnityEngine.Vector3 worldPos;

    }

    public struct WorldDrag : IMarker {

        public UnityEngine.Vector3 fromWorldPos;
        public UnityEngine.Vector3 toWorldPos;

    }

    public struct WorldDragEnd : IMarker {

        public UnityEngine.Vector3 fromWorldPos;
        public UnityEngine.Vector3 toWorldPos;

    }

}