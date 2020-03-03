using EntityId = System.Int32;
using Tick = System.UInt64;
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
        
        public Storage<SharedEntity> shared;
        public Storage<PlayerEntity> players;
        public Storage<UnitEntity> units;
        public Storage<ForestEntity> forest;
        public Storage<CameraEntity> cameras;
        public Storage<SelectionEntity> selections;
        public Storage<SelectionRectEntity> selectionRects;
        // TODO: Place your storages here

        public Components<SharedEntity, WarcraftState> sharedComponents;
        public Components<PlayerEntity, WarcraftState> playerComponents;
        public Components<UnitEntity, WarcraftState> unitComponents;
        public Components<ForestEntity, WarcraftState> forestComponents;
        public Components<CameraEntity, WarcraftState> cameraComponents;
        public Components<SelectionEntity, WarcraftState> selectionComponents;
        public Components<SelectionRectEntity, WarcraftState> selectionRectComponents;
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
            world.Register(ref this.forest, freeze, restore);
            world.Register(ref this.cameras, freeze, restore);
            world.Register(ref this.selections, freeze, restore);
            world.Register(ref this.selectionRects, freeze, restore);
            // TODO: Register your storages here
            
            world.Register(ref this.sharedComponents, freeze, restore);
            world.Register(ref this.playerComponents, freeze, restore);
            world.Register(ref this.unitComponents, freeze, restore);
            world.Register(ref this.forestComponents, freeze, restore);
            world.Register(ref this.cameraComponents, freeze, restore);
            world.Register(ref this.selectionComponents, freeze, restore);
            world.Register(ref this.selectionRectComponents, freeze, restore);
            // TODO: Register your components here
            
        }

        void IState<WarcraftState>.CopyFrom(WarcraftState other) {

            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            // TODO: Copy your custom data here

            this.filters.CopyFrom(other.filters);
            
            this.shared.CopyFrom(other.shared);
            this.players.CopyFrom(other.players);
            this.units.CopyFrom(other.units);
            this.forest.CopyFrom(other.forest);
            this.cameras.CopyFrom(other.cameras);
            this.selections.CopyFrom(other.selections);
            this.selectionRects.CopyFrom(other.selectionRects);
            // TODO: Call CopyFrom on your storages

            this.sharedComponents.CopyFrom(other.sharedComponents);
            this.playerComponents.CopyFrom(other.playerComponents);
            this.unitComponents.CopyFrom(other.unitComponents);
            this.forestComponents.CopyFrom(other.forestComponents);
            this.cameraComponents.CopyFrom(other.cameraComponents);
            this.selectionComponents.CopyFrom(other.selectionComponents);
            this.selectionRectComponents.CopyFrom(other.selectionRectComponents);
            // TODO: Call CopyFrom on your components

        }

        void IPoolableRecycle.OnRecycle() {
            
            WorldUtilities.Release(ref this.filters);
            
            // TODO: Release your custom data here
            
            WorldUtilities.Release(ref this.shared);
            WorldUtilities.Release(ref this.players);
            WorldUtilities.Release(ref this.units);
            WorldUtilities.Release(ref this.forest);
            WorldUtilities.Release(ref this.cameras);
            WorldUtilities.Release(ref this.selections);
            WorldUtilities.Release(ref this.selectionRects);
            // TODO: Call WorldUtilities.Release on your storages

            WorldUtilities.Release(ref this.sharedComponents);
            WorldUtilities.Release(ref this.playerComponents);
            WorldUtilities.Release(ref this.unitComponents);
            WorldUtilities.Release(ref this.forestComponents);
            WorldUtilities.Release(ref this.cameraComponents);
            WorldUtilities.Release(ref this.selectionComponents);
            WorldUtilities.Release(ref this.selectionRectComponents);
            // TODO: Call WorldUtilities.Release on your components

        }

    }

}