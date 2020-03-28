using ME.ECS;
using UnityEngine;

namespace ME.Example.Game.Components.UI {

    public struct UIAddUnit : ME.ECS.IMarker {

        public Color color;
        public int count;
        public ViewId viewSourceId;
        public ViewId viewSourceId2;

    }

}