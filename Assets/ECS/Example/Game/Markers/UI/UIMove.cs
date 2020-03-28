using UnityEngine;

namespace ME.Example.Game.Components.UI {

    using ME.ECS;
    using ME.Example.Game.Entities;
    
    public struct UIMove : IMarker {

        public int pointId;
        public Color color;
        public float moveSide;

    }

}