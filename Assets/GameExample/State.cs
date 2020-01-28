using EntityId = System.Int32;
using Tick = System.UInt64;
using RandomState = UnityEngine.Random.State;

namespace ME.GameExample.Game {

    using ME.ECS;
    using ME.GameExample.Game.Entities;
    
    public class GameState : IState<GameState> {

        public EntityId entityId { get; set; }
        public Tick tick { get; set; }
        public RandomState randomState { get; set; }

        public UnityEngine.Vector3 worldPosition;
        
        public Filter<Point> points;
        public Filter<Unit> units;
        public Filter<Explosion> explosions;

        public Components<Point, GameState> pointComponents;
        public Components<Unit, GameState> unitComponents;
        public Components<Explosion, GameState> explosionComponents;

        int IStateBase.GetHash() {

            return this.pointComponents.Count ^ this.unitComponents.Count ^ this.explosionComponents.Count;

        }

        void IState<GameState>.Initialize(IWorld<GameState> world, bool freeze, bool restore) {

            world.Register(ref this.points, freeze, restore);
            world.Register(ref this.units, freeze, restore);
            world.Register(ref this.explosions, freeze, restore);

            world.Register(ref this.pointComponents, freeze, restore);
            world.Register(ref this.unitComponents, freeze, restore);
            world.Register(ref this.explosionComponents, freeze, restore);

        }

        void IState<GameState>.CopyFrom(GameState other) {

            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            this.worldPosition = other.worldPosition;
            
            this.points.CopyFrom(other.points);
            this.units.CopyFrom(other.units);
            this.explosions.CopyFrom(other.explosions);

            this.pointComponents.CopyFrom(other.pointComponents);
            this.unitComponents.CopyFrom(other.unitComponents);
            this.explosionComponents.CopyFrom(other.explosionComponents);

        }

        void IPoolableRecycle.OnRecycle() {

            WorldUtilities.Release(ref this.points);
            WorldUtilities.Release(ref this.units);
            WorldUtilities.Release(ref this.explosions);

            WorldUtilities.Release(ref this.pointComponents);
            WorldUtilities.Release(ref this.unitComponents);
            WorldUtilities.Release(ref this.explosionComponents);

        }

    }

}