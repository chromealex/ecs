using UnityEngine;
using ViewId = System.UInt64;

namespace ME.Example.Game.Components.UI {

    public struct UIAddUnit : ME.ECS.IMarker {

        public Color color;
        public int count;
        public ViewId viewSourceId;
        public ViewId viewSourceId2;
        public bool topSpawn;

    }

}