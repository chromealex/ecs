#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace Prototype {

    using ME.ECS;
    
    public class PrototypeState : IState<PrototypeState> {

        public EntityId entityId { get; set; }
        public Tick tick { get; set; }
        public RandomState randomState { get; set; }

        public FiltersStorage filters;
        public StructComponentsContainer structComponents;

        public System.Collections.Generic.Dictionary<int, Entity> playerEntities;
        
        public Storage<SharedEntity> shared;
        public Storage<Prototype.Entities.Player> players;
        public Storage<Prototype.Entities.Unit> units;
        public Storage<Prototype.Entities.Map> maps;

        public Components<SharedEntity, PrototypeState> sharedComponents;
        public Components<Prototype.Entities.Player, PrototypeState> playersComponents;
        public Components<Prototype.Entities.Unit, PrototypeState> unitsComponents;
        public Components<Prototype.Entities.Map, PrototypeState> mapsComponents;
        
        int IStateBase.GetHash() {

            return this.sharedComponents.Count;

        }

        void IState<PrototypeState>.Initialize(IWorld<PrototypeState> world, bool freeze, bool restore) {

            world.Register(ref this.filters, freeze, restore);
            world.Register(ref this.structComponents, freeze, restore);
            
            this.playerEntities = PoolDictionary<int, Entity>.Spawn(2);

            world.Register(ref this.shared, freeze, restore);
            world.Register(ref this.players, freeze, restore);
            world.Register(ref this.units, freeze, restore);
            world.Register(ref this.maps, freeze, restore);
            
            world.Register(ref this.sharedComponents, freeze, restore);
            world.Register(ref this.playersComponents, freeze, restore);
            world.Register(ref this.unitsComponents, freeze, restore);
            world.Register(ref this.mapsComponents, freeze, restore);
            
        }

        void IState<PrototypeState>.CopyFrom(PrototypeState other) {

            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            if (this.playerEntities != null) PoolDictionary<int, Entity>.Recycle(ref this.playerEntities);
            this.playerEntities = PoolDictionary<int, Entity>.Spawn(other.playerEntities.Count);
            foreach (var item in other.playerEntities) {
                
                this.playerEntities.Add(item.Key, item.Value);
                
            }

            this.filters.CopyFrom(other.filters);
            this.structComponents.CopyFrom(other.structComponents);
            
            this.shared.CopyFrom(other.shared);
            this.players.CopyFrom(other.players);
            this.units.CopyFrom(other.units);
            this.maps.CopyFrom(other.maps);

            this.sharedComponents.CopyFrom(other.sharedComponents);
            this.playersComponents.CopyFrom(other.playersComponents);
            this.unitsComponents.CopyFrom(other.unitsComponents);
            this.mapsComponents.CopyFrom(other.mapsComponents);

        }

        void IPoolableRecycle.OnRecycle() {
            
            WorldUtilities.Release(ref this.filters);
            WorldUtilities.Release(ref this.structComponents);
            
            PoolDictionary<int, Entity>.Recycle(ref this.playerEntities);
            
            WorldUtilities.Release(ref this.shared);
            WorldUtilities.Release(ref this.players);
            WorldUtilities.Release(ref this.units);
            WorldUtilities.Release(ref this.maps);

            WorldUtilities.Release(ref this.sharedComponents);
            WorldUtilities.Release(ref this.playersComponents);
            WorldUtilities.Release(ref this.unitsComponents);
            WorldUtilities.Release(ref this.mapsComponents);

        }

    }

}