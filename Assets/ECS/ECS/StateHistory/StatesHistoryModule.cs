using Tick = System.UInt64; 

namespace ME.ECS.StatesHistory {

    public class CircularQueue<T> where T : class {

        private T[] data;
        private uint capacity;
        private uint indexer;

        public CircularQueue(uint capacity) {

            this.data = new T[capacity];
            this.capacity = capacity;
            this.indexer = 0;

        }
        
        public T Add(T data) {

            T result = null;
            if (this.data[this.indexer] != null) {

                result = this.data[this.indexer];

            }

            this.data[this.indexer] = data;
            ++this.indexer;
            if (this.indexer >= this.capacity) {

                this.indexer = 0;

            }

            return result;

        }
        
    }

    public abstract class StatesHistoryModule<TState> : IModule<TState>, IModuleValidation where TState : class, IState<TState> {

        private CircularQueue<TState> states;
        private Tick currentTick;
        private Tick maxTick;

        public IWorld<TState> world { get; set; }

        protected virtual uint GetQueueCapacity() {

            return 10u;

        }

        protected virtual uint GetTicksPerState() {

            return 100u;

        }

        public Tick GetTickByTime(double seconds) {

            var tick = (seconds / this.world.GetTickTime());
            return (Tick)System.Math.Floor(tick);

        }

        public Tick GetTick() {

            return this.currentTick;

        }

        public void SetTick(Tick tick) {

            this.currentTick = tick;
            this.world.SetTimeSinceStart(tick * this.world.GetTickTime());
            if (tick > this.maxTick) this.maxTick = tick;

        }

        public virtual bool CouldBeAdded() {

            return this.world.GetTickTime() > 0f;

        }

        void IModule<TState>.OnConstruct() {
            
            this.states = new CircularQueue<TState>(this.GetQueueCapacity());
            this.world.SetStatesHistoryModule(this);

        }
        
        void IModule<TState>.OnDeconstruct() { }

        public void Update(TState state, float deltaTime) {

            if (this.prewarmed == false) {
                
                this.Prewarm();
                
            }
            
            var tick = this.GetTickByTime(this.world.GetTimeSinceStart());
            if (tick != this.currentTick) {

                this.currentTick = tick;
                state.tick = this.currentTick;

                if (tick > 0ul && this.currentTick % this.GetTicksPerState() == 0) {

                    this.StoreState(tick);

                }

            }

        }

        private bool prewarmed;
        private void Prewarm() {

            for (uint i = 0; i < this.GetQueueCapacity(); ++i) {
                
                this.StoreState(i * this.GetTicksPerState());
                
            }

            this.prewarmed = true;

        }

        private void StoreState(Tick tick) {

            var newState = this.world.CreateState();
            newState.Initialize(this.world, freeze: true, restore: false);
            var state = this.world.GetState();
            newState.CopyFrom(state);
            newState.tick = tick;
            var oldState = this.states.Add(newState);
            if (oldState != null) this.world.ReleaseState(ref oldState);

        }

    }

}