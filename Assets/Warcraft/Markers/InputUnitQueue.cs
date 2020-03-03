using ME.ECS;

namespace Warcraft.Markers {

    public struct InputUnitQueue : IMarker {

        public Entity selectedUnit;
        public ActionsGraphNode actionInfo;
        public UnitInfo unitInfo;

    }

}