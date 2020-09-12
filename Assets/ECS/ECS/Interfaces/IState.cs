#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace ME.ECS {

    public interface IStateBase {

        int entityId { get; set; }
        Tick tick { get; set; }
        RandomState randomState { get; set; }

        int GetHash();

        void Initialize(World world, bool freeze, bool restore);
        
    }

    public interface IState : IStateBase, IPoolableRecycle {

    }

    public abstract class State : IState {

        public int entityId { get; set; }
        public Tick tick { get; set; }
        public RandomState randomState { get; set; }

        internal FiltersStorage filters;
        internal StructComponentsContainer structComponents;
        internal Storage storage;
        internal Components components;
        
        /// <summary>
        /// Return most unique hash
        /// </summary>
        /// <returns></returns>
        public virtual int GetHash() {

            return this.components.GetHash() ^ this.structComponents.Count;//^ this.structComponents.GetCustomHash();

        }

        public virtual void Initialize(World world, bool freeze, bool restore) {
            
            world.Register(ref this.filters, freeze, restore);
            world.Register(ref this.structComponents, freeze, restore);
            world.Register(ref this.storage, freeze, restore);
            world.Register(ref this.components, freeze, restore);

        }

        public virtual void CopyFrom(State other) {
            
            this.entityId = other.entityId;
            this.tick = other.tick;
            this.randomState = other.randomState;

            this.filters.CopyFrom(other.filters);
            this.structComponents.CopyFrom(other.structComponents);
            this.storage.CopyFrom(other.storage);
            this.components.CopyFrom(other.components);

        }

        public virtual void OnRecycle() {
            
            WorldUtilities.Release(ref this.filters);
            WorldUtilities.Release(ref this.structComponents);
            WorldUtilities.Release(ref this.storage);
            WorldUtilities.Release(ref this.components);

        }

    }

}