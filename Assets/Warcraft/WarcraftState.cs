#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace Warcraft {

    using ME.ECS;
    using Warcraft.Entities;
    
    public class WarcraftState : IState<WarcraftState> {

        public EntityId entityId { get; set; }
        public Tick tick { get; set; }
        public RandomState randomState { get; set; }

        public FiltersStorage filters;
        
        // TODO: Place your custom data here
        public float peasantsMindTimer;
        
        public Storage<SharedEntity> shared;
        public Storage<PlayerEntity> players;
        public Storage<UnitEntity> units;
        public Storage<CameraEntity> cameras;
        public Storage<SelectionEntity> selections;
        public Storage<SelectionRectEntity> selectionRects;
        public Storage<DebugEntity> debug;
        // TODO: Place your storages here

        public Components<SharedEntity, WarcraftState> sharedComponents;
        public Components<PlayerEntity, WarcraftState> playerComponents;
        public Components<UnitEntity, WarcraftState> unitComponents;
        public Components<CameraEntity, WarcraftState> cameraComponents;
        public Components<SelectionEntity, WarcraftState> selectionComponents;
        public Components<SelectionRectEntity, WarcraftState> selectionRectComponents;
        public Components<DebugEntity, WarcraftState> debugComponents;
        // TODO: Place your components here
        
        int IStateBase.GetHash() {

            // TODO: Return most unique hash here
            return this.sharedComponents.Count;

        }

        void IState<WarcraftState>.Initialize(IWorld<WarcraftState> world, bool freeze, bool restore) {

            world.Register(ref this.filters, freeze, restore);
            
            world.Register(ref this.shared, freeze, restore);
            world.Register(ref this.players, freeze, restore);
            world.Register(ref this.units, freeze, restore);
            world.Register(ref this.cameras, freeze, restore);
            world.Register(ref this.selections, freeze, restore);
            world.Register(ref this.selectionRects, freeze, restore);
            world.Register(ref this.debug, freeze, restore);
            // TODO: Register your storages here
            
            world.Register(ref this.sharedComponents, freeze, restore);
            world.Register(ref this.playerComponents, freeze, restore);
            world.Register(ref this.unitComponents, freeze, restore);
            world.Register(ref this.cameraComponents, freeze, restore);
            world.Register(ref this.selectionComponents, freeze, restore);
            world.Register(ref this.selectionRectComponents, freeze, restore);
            world.Register(ref this.debugComponents, freeze, restore);
            // TODO: Register your components here
            
        }

        void IState<WarcraftState>.CopyFrom(WarcraftState other) {

            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            // TODO: Copy your custom data here
            this.peasantsMindTimer = other.peasantsMindTimer;

            this.filters.CopyFrom(other.filters);
            
            this.shared.CopyFrom(other.shared);
            this.players.CopyFrom(other.players);
            this.units.CopyFrom(other.units);
            this.cameras.CopyFrom(other.cameras);
            this.selections.CopyFrom(other.selections);
            this.selectionRects.CopyFrom(other.selectionRects);
            this.debug.CopyFrom(other.debug);
            // TODO: Call CopyFrom on your storages

            this.sharedComponents.CopyFrom(other.sharedComponents);
            this.playerComponents.CopyFrom(other.playerComponents);
            this.unitComponents.CopyFrom(other.unitComponents);
            this.cameraComponents.CopyFrom(other.cameraComponents);
            this.selectionComponents.CopyFrom(other.selectionComponents);
            this.selectionRectComponents.CopyFrom(other.selectionRectComponents);
            this.debugComponents.CopyFrom(other.debugComponents);
            // TODO: Call CopyFrom on your components

        }

        void IPoolableRecycle.OnRecycle() {
            
            WorldUtilities.Release(ref this.filters);
            
            // TODO: Release your custom data here
            this.peasantsMindTimer = default;
            
            WorldUtilities.Release(ref this.shared);
            WorldUtilities.Release(ref this.players);
            WorldUtilities.Release(ref this.units);
            WorldUtilities.Release(ref this.cameras);
            WorldUtilities.Release(ref this.selections);
            WorldUtilities.Release(ref this.selectionRects);
            WorldUtilities.Release(ref this.debug);
            // TODO: Call WorldUtilities.Release on your storages

            WorldUtilities.Release(ref this.sharedComponents);
            WorldUtilities.Release(ref this.playerComponents);
            WorldUtilities.Release(ref this.unitComponents);
            WorldUtilities.Release(ref this.cameraComponents);
            WorldUtilities.Release(ref this.selectionComponents);
            WorldUtilities.Release(ref this.selectionRectComponents);
            WorldUtilities.Release(ref this.debugComponents);
            // TODO: Call WorldUtilities.Release on your components

        }

    }

}