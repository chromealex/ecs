using ME.ECS;

namespace Warcraft.Markers {

    public struct InputUnitQueue : IMarker {

        public Entity selectedUnit;
        public ActionsGraphNode actionInfo;
        public UnitInfo unitInfo;

    }

    public struct InputUnitQueueCancel : IMarker {

        public Entity selectedUnit;
        public Entity unitInQueue;

    }

}