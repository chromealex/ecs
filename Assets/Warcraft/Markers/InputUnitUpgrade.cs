using ME.ECS;

namespace Warcraft.Markers {

    public struct InputUnitUpgrade : IMarker {

        public Entity selectedUnit;
        public ActionsGraphNode actionInfo;
        public UnitInfo unitInfo;

    }

}