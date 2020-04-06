#if STATES_HISTORY_MODULE_SUPPORT
using System.Collections.Generic;

namespace ME.ECS {
    
    public partial interface IWorldBase {

        Tick GetCurrentTick();
        void Simulate(double time);
        void Simulate(Tick toTick);

    }

    public partial interface IWorld<TState> where TState : class, IState<TState>, new() {

        void SetStatesHistoryModule(StatesHistory.IStatesHistoryModule<TState> module);

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        private StatesHistory.IStatesHistoryModule<TState> statesHistoryModule;
        public void SetStatesHistoryModule(StatesHistory.IStatesHistoryModule<TState> module) {

            this.statesHistoryModule = module;

        }

        public Tick GetCurrentTick() {

            if (this.statesHistoryModule != null) {

                return this.statesHistoryModule.GetTick();

            }

            return default;

        }

        partial void PlayPlugin1ForTick(Tick tick) {
            
            if (this.statesHistoryModule != null) this.statesHistoryModule.PlayEventsForTick(tick);
            
        }

        void IWorldBase.Simulate(double time) {

            if (this.statesHistoryModule != null) {

                this.timeSinceStart = time;
                this.statesHistoryModule.Simulate(this.GetCurrentTick(), this.statesHistoryModule.GetTickByTime(time));
                
            }

        }

        void IWorldBase.Simulate(Tick toTick) {

            if (this.statesHistoryModule != null) {

                this.timeSinceStart = toTick * this.tickTime;
                this.statesHistoryModule.Simulate(this.GetCurrentTick(), toTick);
                
            }

        }

    }

}

namespace ME.ECS.StatesHistory {

    using ME.ECS.Collections;
    
    [System.Serializable]
    public struct HistoryEvent {

        // Header
        /// <summary>
        /// Event tick
        /// </summary>
        public ulong tick;
        /// <summary>
        /// Global event order (for example: you have 30 players on the map, each has it's own index)
        /// </summary>
        public int order;
        /// <summary>
        /// Local event order (order would be the first, then localOrder applies)
        /// </summary>
        public int localOrder;
        
        // Data
        /// <summary>
        /// Object Id to be called on (see NetworkModule::RegisterObject)
        /// </summary>
        public int objId;
        /// <summary>
        /// Group Id of objects (see NetworkModule::RegisterObject).
        /// One object could be registered in different groups at the same time.
        /// 0 by default (Common group)
        /// </summary>
        public int groupId;
        /// <summary>
        /// Rpc Id is a method Id (see NetworkModule::RegisterRPC) 
        /// </summary>
        public int rpcId;
        /// <summary>
        /// Parameters of method
        /// </summary>
        public object[] parameters;

        public bool storeInHistory;

        public override string ToString() {
            
            return "Tick: " + this.tick + ", Order: " + this.order + ", LocalOrder: " + this.localOrder + ", objId: " + this.objId + ", groupId: " + this.groupId + ", rpcId: " + this.rpcId + ", parameters: " + (this.parameters != null ? this.parameters.Length : 0);
            
        }

    }

    public interface IEventRunner {

        void RunEvent(HistoryEvent historyEvent);

    }

    public interface IStatesHistoryModuleBase {

        void BeginAddEvents();
        void EndAddEvents();

        System.Collections.IDictionary GetData();
        ME.ECS.Network.IStatesHistory GetDataStates();

        Tick GetTick();
        void SetTick(Tick tick);

        void PlayEventsForTick(Tick tick);
        void RunEvent(HistoryEvent historyEvent);

        void SetEventRunner(IEventRunner eventRunner);

        void Simulate(Tick currentTick, Tick targetTick);
        
        void SetSyncHash(Tick tick, int hash);

        int GetEventsAddedCount();
        int GetEventsPlayedCount();

    }

    public interface IStatesHistoryModule<TState> : IStatesHistoryModuleBase, IModule<TState> where TState : class, IState<TState>, new() {

        void AddEvents(IList<HistoryEvent> historyEvents);
        void AddEvent(HistoryEvent historyEvent);

        int GetStateHash(IState<TState> state);
        
        Tick GetTickByTime(double seconds);
        TState GetStateBeforeTick(Tick tick);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class StatesHistoryModule<TState> : IStatesHistoryModule<TState>, IModuleValidation where TState : class, IState<TState>, new() {

        private const int POOL_EVENTS_CAPACITY = 1000;
        private const int POOL_HISTORY_SIZE = 100;
        private const int POOL_HISTORY_CAPACITY = 2;
        private const int POOL_SYNCHASH_CAPACITY = 10;

        private const uint DEFAULT_QUEUE_CAPACITY = 10u;
        private const uint DEFAULT_TICKS_PER_STATE = 100u;
        
        //private StatesCircularQueue<TState> states;
        private ME.ECS.Network.StatesHistory<TState> statesHistory;
        private Dictionary<Tick, SortedList<long, HistoryEvent>> events;
        private Dictionary<Tick, int> syncHash;
        private Tick currentTick;
        private Tick maxTick;
        private bool prewarmed;
        private Tick beginAddEventsTick;
        private int beginAddEventsCount;
        private bool beginAddEvents;
        private IEventRunner eventRunner;

        private int statEventsAdded;
        private int statPlayedEvents;

        public IWorld<TState> world { get; set; }

        void IModuleBase.OnConstruct() {
            
            this.statesHistory = new ME.ECS.Network.StatesHistory<TState>(this.world, this.GetQueueCapacity());
            //this.states = new StatesCircularQueue<TState>(this.GetTicksPerState(), this.GetQueueCapacity());
            this.events = PoolDictionary<Tick, SortedList<long, HistoryEvent>>.Spawn(StatesHistoryModule<TState>.POOL_EVENTS_CAPACITY);
            this.syncHash = PoolDictionary<Tick, int>.Spawn(StatesHistoryModule<TState>.POOL_SYNCHASH_CAPACITY);
            PoolSortedList<int, HistoryEvent>.Prewarm(StatesHistoryModule<TState>.POOL_HISTORY_SIZE, StatesHistoryModule<TState>.POOL_HISTORY_CAPACITY);
            
            this.world.SetStatesHistoryModule(this);

        }

        void IModuleBase.OnDeconstruct() {
            
            this.statesHistory.DiscardAll();
            
            this.world.SetStatesHistoryModule(null);
            
            foreach (var item in this.events) {

                foreach (var hItem in item.Value) {

                    var val = hItem.Value;
                    PoolArray<object>.Recycle(ref val.parameters);

                }
                item.Value.Clear();

            }
            PoolDictionary<Tick, SortedList<long, HistoryEvent>>.Recycle(ref this.events);
            PoolDictionary<Tick, int>.Recycle(ref this.syncHash);

            //this.states.Recycle();
            //this.states = null;

        }

        void IStatesHistoryModuleBase.SetEventRunner(IEventRunner eventRunner) {

            this.eventRunner = eventRunner;

        }

        protected virtual uint GetQueueCapacity() {

            return StatesHistoryModule<TState>.DEFAULT_QUEUE_CAPACITY;

        }

        /// <summary>
        /// System will copy current state every N ticks
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetTicksPerState() {

            return StatesHistoryModule<TState>.DEFAULT_TICKS_PER_STATE;

        }

        public int GetEventsAddedCount() {

            return this.statEventsAdded;

        }

        public int GetEventsPlayedCount() {

            return this.statPlayedEvents;

        }

        public void BeginAddEvents() {

            this.beginAddEventsCount = 0;
            this.beginAddEventsTick = this.currentTick;
            this.beginAddEvents = true;

        }

        public void EndAddEvents() {

            if (this.beginAddEvents == true && this.beginAddEventsCount > 0) {
                
                this.Simulate(this.beginAddEventsTick, this.currentTick);

            }
            
            this.beginAddEvents = false;
            
        }

        public void AddEvents(IList<HistoryEvent> historyEvents) {
            
            this.BeginAddEvents();
            for (int i = 0, count = historyEvents.Count; i < count; ++i) this.AddEvent(historyEvents[i]);
            this.EndAddEvents();
            
        }

        public void AddEvent(HistoryEvent historyEvent) {

            ++this.statEventsAdded;
            
            this.ValidatePrewarm();

            if (historyEvent.tick <= Tick.Zero) {

                // Tick fix if it is zero
                historyEvent.tick = Tick.One;

            }

            SortedList<long, HistoryEvent> list;
            if (this.events.TryGetValue(historyEvent.tick, out list) == true) {
                
                list.Add(MathUtils.GetKey(historyEvent.order, historyEvent.localOrder), historyEvent);

            } else {

                list = PoolSortedList<long, HistoryEvent>.Spawn(StatesHistoryModule<TState>.POOL_HISTORY_CAPACITY);
                list.Add(MathUtils.GetKey(historyEvent.order, historyEvent.localOrder), historyEvent);
                this.events.Add(historyEvent.tick, list);

            }

            if (this.currentTick >= historyEvent.tick) {

                if (this.beginAddEvents == false) {

                    this.Simulate(historyEvent.tick, this.currentTick);

                } else {

                    ++this.beginAddEventsCount;
                    if (this.beginAddEventsTick > historyEvent.tick) {

                        this.beginAddEventsTick = historyEvent.tick;

                    }

                }

            }

        }

        public void Simulate(Tick currentTick, Tick targetTick) {

            TState state;
            var historyState = this.GetStateBeforeTick(currentTick);
            if (historyState != null) {
                
                // State found - simulate from this state to current tick
                state = historyState;
                
            } else {
                
                // Previous state was not found - need to rewind from initial state
                state = this.world.GetResetState();
                this.statPlayedEvents = 0;
                
            }
            
            this.world.GetState().CopyFrom(state);
            this.world.GetState().tick = targetTick;
            this.world.Simulate(state.tick, targetTick);
            this.world.SetFrameStartTick(targetTick);

        }

        public void SetSyncHash(Tick tick, int hash) {
            
            if (this.syncHash.ContainsKey(tick) == false) this.syncHash.Add(tick, hash);
            
        }
        
        private void CheckHash(Tick tick) {

            int hash;
            if (this.syncHash.TryGetValue(tick, out hash) == true) {

                var state = this.GetStateBeforeTick(tick);
                if (state == null) state = this.world.GetResetState();
                var localHash = this.GetStateHash(state);
                if (localHash != hash) {

                    UnityEngine.Debug.LogError(this.world.id + " Remote Hash: " + hash + ", Local Hash: " + localHash);

                }

            }

        }

        public int GetStateHash(IState<TState> state) {

            return state.entityId ^ (int)state.tick ^ state.GetHash();

        }

        public TState GetStateBeforeTick(Tick tick) {

            this.ValidatePrewarm();

            if (this.statesHistory.FindClosestEntry(tick, out var state) == true) {

                return state;

            }

            //TState state = default;
            //var minDelta = long.MaxValue;
            //var state = this.states.GetBefore(tick);
            /*for (int i = 0; i < arr.Length; ++i) {

                var item = arr[i];
                if (item != null && item.tick < tick) {

                    var delta = (long)(item.tick - tick);
                    if (delta < 0L) delta = -delta;
                    if (delta < minDelta) {

                        minDelta = delta;
                        state = item;

                    }

                }

            }*/

            return default;

        }

        public Tick GetTickByTime(double seconds) {

            var tick = (seconds / this.world.GetTickTime());
            return System.Math.Floor(tick);

        }

        public Tick GetTick() {

            return this.currentTick;

        }

        public void SetTick(Tick tick) {

            //this.currentTick = tick;
            if (tick > this.maxTick) this.maxTick = tick;

        }

        public virtual bool CouldBeAdded() {

            return this.world.GetTickTime() > 0f;

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {
            
        }

        void IModule<TState>.Update(TState state, float deltaTime) {

            this.ValidatePrewarm();
            
            var tick = this.GetTickByTime(this.world.GetTimeSinceStart());
            //if (tick != this.currentTick) {

                this.currentTick = tick;
                
            //}
            
            state.tick = this.currentTick;

        }

        public void PlayEventsForTick(Tick tick) {

            if (tick > 0UL && tick % this.GetTicksPerState() == 0) {

                this.StoreState(tick);

            }
            
            SortedList<long, HistoryEvent> list;
            if (this.events.TryGetValue(tick, out list) == true) {

                var values = list.Values;
                for (int i = 0, count = values.Count; i < count; ++i) {

                    this.RunEvent(values[i]);

                }

            }

            this.CheckHash(tick);

        }

        public ME.ECS.Network.IStatesHistory GetDataStates() {

            return this.statesHistory;

        }

        public System.Collections.IDictionary GetData() {

            return this.events;

        }

        public void RunEvent(HistoryEvent historyEvent) {

            if (historyEvent.storeInHistory == true) {
                
                ++this.statPlayedEvents;
                //UnityEngine.Debug.LogError("Run event tick: " + historyEvent.tick + ", method: " + historyEvent.localOrder + ", currentTick: " + this.currentTick);
                
            }
            
            if (this.eventRunner != null) this.eventRunner.RunEvent(historyEvent);
            
        }

        private void ValidatePrewarm() {
            
            if (this.prewarmed == false) {
                
                this.Prewarm();
                this.prewarmed = true;

            }

        }

        private void Prewarm() {

            /*this.states.BeginSet();
            for (uint i = 0; i < this.GetQueueCapacity(); ++i) {
                
                this.StoreState(i * this.GetTicksPerState(), isPrewarm: true);
                
            }
            this.states.EndSet();*/

        }

        private void StoreState(Tick tick, bool isPrewarm = false) {

            /*if (isPrewarm == false) {
                
                UnityEngine.Debug.LogWarning("StoreState: " + this.world.id + ", tick: " + tick);
                var state = this.world.GetState();
                //UnityEngine.Debug.Log("State tick: " + state.tick);
                //UnityEngine.Debug.Log("State entityId: " + state.entityId);
                //UnityEngine.Debug.Log("State random: " + state.randomState);
                UnityEngine.Debug.Log("State: " + state.ToString());
                
            }*/

            {
                //var newState = WorldUtilities.CreateState<TState>();
                //newState.Initialize(this.world, freeze: true, restore: false);
                /*var state = this.world.GetState();
                newState.CopyFrom(state);
                newState.tick = tick;
                this.states.Set(tick, newState);*/

                this.statesHistory.Store(tick, this.world.GetState());

            }

        }

    }

}
#endif