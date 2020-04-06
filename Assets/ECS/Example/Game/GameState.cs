#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace ME.Example.Game {

    using ME.ECS;
    using ME.Example.Game.Entities;
    
    public class State : IState<State> {

        public EntityId entityId { get; set; }
        public Tick tick { get; set; }
        public RandomState randomState { get; set; }
        
        public UnityEngine.Vector3 worldPosition;

        public FiltersStorage filters;

        public Storage<SharedEntity> shared;
        public Storage<Point> points;
        public Storage<Unit> units;
        public Storage<PlayerZone> playerZones;

        public Components<SharedEntity, State> sharedComponents;
        public Components<Point, State> pointComponents;
        public Components<Unit, State> unitComponents;
        public Components<PlayerZone, State> playerZonesComponents;

        int IStateBase.GetHash() {

            return this.pointComponents.Count ^ this.unitComponents.Count;

        }

        void IState<State>.Initialize(IWorld<State> world, bool freeze, bool restore) {

            world.Register(ref this.filters, freeze, restore);
            
            world.Register(ref this.shared, freeze, restore);
            world.Register(ref this.points, freeze, restore);
            world.Register(ref this.units, freeze, restore);
            world.Register(ref this.playerZones, freeze, restore);

            world.Register(ref this.sharedComponents, freeze, restore);
            world.Register(ref this.pointComponents, freeze, restore);
            world.Register(ref this.unitComponents, freeze, restore);
            world.Register(ref this.playerZonesComponents, freeze, restore);

        }

        void IState<State>.CopyFrom(State other) {

            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            this.worldPosition = other.worldPosition;
            
            this.filters.CopyFrom(other.filters);
            
            this.shared.CopyFrom(other.shared);
            this.points.CopyFrom(other.points);
            this.units.CopyFrom(other.units);
            this.playerZones.CopyFrom(other.playerZones);

            this.sharedComponents.CopyFrom(other.sharedComponents);
            this.pointComponents.CopyFrom(other.pointComponents);
            this.unitComponents.CopyFrom(other.unitComponents);
            this.playerZonesComponents.CopyFrom(other.playerZonesComponents);

        }

        void IPoolableRecycle.OnRecycle() {
            
            WorldUtilities.Release(ref this.filters);
            
            WorldUtilities.Release(ref this.shared);
            WorldUtilities.Release(ref this.points);
            WorldUtilities.Release(ref this.units);
            WorldUtilities.Release(ref this.playerZones);

            WorldUtilities.Release(ref this.sharedComponents);
            WorldUtilities.Release(ref this.pointComponents);
            WorldUtilities.Release(ref this.unitComponents);
            WorldUtilities.Release(ref this.playerZonesComponents);

        }

    }

}