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

        byte[] Serialize<T>() where T : State;

        void Deserialize<T>(byte[] bytes) where T : State;
    }

    public interface IState : IStateBase, IPoolableRecycle {

    }

    public abstract class State : IState {

        [ME.ECS.Serializer.SerializeField]
        public int entityId { get; set; }
        [ME.ECS.Serializer.SerializeField]
        public Tick tick { get; set; }
        [ME.ECS.Serializer.SerializeField]
        public RandomState randomState { get; set; }

        // [ME.ECS.Serializer.SerializeField]
        public FiltersStorage filters;
        [ME.ECS.Serializer.SerializeField]
        internal StructComponentsContainer structComponents;
        [ME.ECS.Serializer.SerializeField]
        internal Storage storage;
        [ME.ECS.Serializer.SerializeField]
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

        public virtual byte[] Serialize<T>() where T : State {
         
            var serializers = ME.ECS.Serializer.ECSSerializers.GetSerializers();
            serializers.Add(new ME.ECS.Serializer.BufferArraySerializer());
            return ME.ECS.Serializer.Serializer.Pack((T)this, serializers);
            
        }

        public virtual void Deserialize<T>(byte[] bytes) where T : State {
            
            var serializers = ME.ECS.Serializer.ECSSerializers.GetSerializers();
            serializers.Add(new ME.ECS.Serializer.BufferArraySerializer());
            ME.ECS.Serializer.Serializer.Unpack(bytes, serializers, (T)this);
            
        }

    }

}