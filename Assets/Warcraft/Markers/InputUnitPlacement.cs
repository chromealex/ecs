using ME.ECS;

namespace Warcraft.Markers {

    public struct InputUnitPlacement : IMarker {

        public Entity selectedUnit;
        public ActionsGraphNode actionInfo;
        public UnitInfo unitInfo;

    }

}