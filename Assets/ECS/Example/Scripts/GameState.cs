using EntityId = System.Int32;
using Tick = System.UInt64;
using RandomState = UnityEngine.Random.State;

namespace ME.Example.Game {

    using ME.ECS;
    using ME.Example.Game.Entities;
    
    public class State : IState<State> {

        public EntityId entityId { get; set; }
        public Tick tick { get; set; }
        public RandomState randomState { get; set; }

        public Filter<Point> points;
        public Filter<Unit> units;

        public Components<Point, State> pointComponents;
        public Components<Unit, State> unitComponents;

        int IStateBase.GetHash() {

            return this.pointComponents.Count ^ this.unitComponents.Count;

        }

        void IState<State>.Initialize(IWorld<State> world, bool freeze, bool restore) {

            world.Register(ref this.points, freeze, restore);
            world.Register(ref this.units, freeze, restore);

            world.Register(ref this.pointComponents, freeze, restore);
            world.Register(ref this.unitComponents, freeze, restore);

        }

        void IState<State>.CopyFrom(State other) {

            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            this.points.CopyFrom(other.points);
            this.units.CopyFrom(other.units);

            this.pointComponents.CopyFrom(other.pointComponents);
            this.unitComponents.CopyFrom(other.unitComponents);

        }

        void IPoolableRecycle.OnRecycle() {

            WorldUtilities.Release(ref this.points);
            WorldUtilities.Release(ref this.units);

            WorldUtilities.Release(ref this.pointComponents);
            WorldUtilities.Release(ref this.unitComponents);

        }

    }

}